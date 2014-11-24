namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Owin;
    using Cedar.Serialization;
    using Cedar.TypeResolution;
    
    using AppFunc = System.Func<
        System.Collections.Generic.IDictionary<string, object>, 
        System.Threading.Tasks.Task
    >;
    using MidFunc = System.Func<
        System.Func<
            System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >,
        System.Func<
            System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >
    >;

    public static class CommandHandlingMiddleware
    {
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);

        public static MidFunc HandleCommands(HandlerSettings options, string commandPath = "/commands")
        {
            Guard.EnsureNotNull(options, "options");

            var resultReportingHandler = new CommandResultHandlerModule(options.HandlerResolvers);

            options = new HandlerSettings(resultReportingHandler,
                options.RequestTypeResolver,
                options.ExceptionToModelConverter,
                options.Serializer);

            return next => env =>
            {
                var context = new OwinContext(env);

                var path = context.Request.Path;
                if(false == path.StartsWithSegments(new PathString(commandPath), out path))
                {
                    // not routed to us
                    return next(env);
                }

                var commandIdString = path.Value.Substring(1);
                Guid commandId;

                if(false == Guid.TryParse(commandIdString, out commandId))
                {
                    return next(env);
                }

                // GET" /{guid}"
                if(context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    CommandResult result;
                    if(false == resultReportingHandler.Storage.TryGetResult(commandId, out result))
                    {
                        return next(env);
                    }

                    var body = options.Serializer.Serialize(result);

                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";

                    return context.Response.WriteAsync(body);
                }

                // PUT "/{guid}" with a Json body.
                if(context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    return BuildSendCommand(commandId).ExecuteWithExceptionHandling(context, options);
                }

                // Not a command, pass through.
                return next(env);
            };
        }

        private static Func<IOwinContext, HandlerSettings, Task> BuildSendCommand(Guid commandId)
        {
            return (context, options) => HandleCommand(context, commandId, options);
        }

        private static async Task HandleCommand(IOwinContext context, Guid commandId, HandlerSettings options)
        {
            string contentType = context.Request.ContentType;

            Type commandType;
            if(!contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
               || (commandType = options.RequestTypeResolver.ResolveInputType(new CedarRequest(context))) == null)
            {
                // Not a json entity, bad request
                throw new HttpStatusException("The specified media type is not supported.",
                    HttpStatusCode.UnsupportedMediaType,
                    new NotSupportedException());
            }
            object command;
            using(var streamReader = new StreamReader(context.Request.Body))
            {
                command = options.Serializer.Deserialize(streamReader, commandType);
            }
            var user = (context.Request.User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            var dispatchCommand = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());
            await (Task) dispatchCommand.Invoke(null,
                new[]
                {
                    options.HandlerResolvers, commandId, user, command, context.Request.CallCancelled
                });
            context.Response.StatusCode = 202;
            context.Response.ReasonPhrase = "Accepted";
        }

        private class CommandResultHandlerModule : IHandlerResolver
        {
            private readonly IEnumerable<IHandlerResolver> _inner;
            private readonly CommandResultStorage _storage;

            public CommandResultHandlerModule(IEnumerable<IHandlerResolver> inner)
            {
                _inner = inner;
                _storage = new CommandResultStorage(inner);
            }

            public CommandResultStorage Storage
            {
                get { return _storage; }
            }

            public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
            {
                if(typeof(DomainEventMessage).IsAssignableFrom(typeof(TMessage)))
                {
                    return HandleEventAndReportResults<TMessage>();
                }

                if(typeof(CommandMessage).IsAssignableFrom(typeof(TMessage)))
                {
                    return HandleCommand<TMessage>();
                }

                return Enumerable.Empty<Handler<TMessage>>();
            }

            private IEnumerable<Handler<TMessage>> HandleCommand<TMessage>() where TMessage : class
            {
                yield return (message, ct) => _inner.DispatchSingle(message, ct);

            }

            private IEnumerable<Handler<TMessage>> HandleEventAndReportResults<TMessage>() where TMessage : class
            {
                var handlers = from handlerResolver in _inner
                    from handler in handlerResolver.GetHandlersFor<TMessage>()
                    select handler;

                return handlers
                    .Select(next => new Handler<TMessage>(async (message, ct) =>
                    {
                        Exception caughtException = null;

                        var domainEventMessage = message as DomainEventMessage;

                        try
                        {
                            await next(message, ct);
                        }
                        catch(Exception ex)
                        {
                            caughtException = ex;
                        }

                        var commitId = domainEventMessage.GetCommitId();

                        if(commitId.HasValue)
                        {
                            if(caughtException != null)
                            {
                                Storage.NotifyEventHandledSuccessfully(commitId.Value);
                            }
                            else
                            {
                                Storage.NotifyEventHandledUnsuccessfully(commitId.Value);
                            }
                        }

                        if(caughtException != null)
                        {
                            ExceptionDispatchInfo.Capture(caughtException).Throw();
                        }
                    }));
            }
        }
    }
}