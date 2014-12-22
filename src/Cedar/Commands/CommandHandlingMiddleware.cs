namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Dispatcher;
    using Cedar.Handlers;
    using Microsoft.Owin.Builder;
    using Owin;
    using TinyIoC;
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

        public static MidFunc HandleCommands(HandlerSettings settings, string commandPath = "/commands")
        {
            Guard.EnsureNotNull(settings, "options");
            Guard.EnsureNotNullOrWhiteSpace(commandPath, "commandPath");

            var resultReportingHandler = new CommandResultHandlerModule(settings.HandlerResolvers);

            settings = new HandlerSettings(resultReportingHandler,
                settings.RequestTypeResolver,
                settings.ExceptionToModelConverter,
                settings.Serializer);

            return next =>
            {
                var webApiConfig = ConfigureWebApi(settings);
                var appBuilder = new AppBuilder();
                appBuilder
                    .Map(commandPath,
                        commandsApp =>
                        {
                            commandsApp.UseWebApi(webApiConfig);
                        })
                    .Run(ctx => next(ctx.Environment));
                return appBuilder.Build();
            };
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
                            await next(message, ct).NotOnCapturedContext();
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

        private static HttpConfiguration ConfigureWebApi(HandlerSettings settings)
        {
            var container = new TinyIoCContainer();
            container.Register(settings);

            var config = new HttpConfiguration
            {
                DependencyResolver = new TinyIoCDependencyResolver(container)
            };
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new CommandHandlingHttpControllerTypeResolver());
            config.MapHttpAttributeRoutes();

            return config;
        }

        private class TinyIoCDependencyResolver : IDependencyResolver
        {
            private readonly TinyIoCContainer _container;

            public TinyIoCDependencyResolver(TinyIoCContainer container)
            {
                _container = container;
            }

            public void Dispose()
            { }

            public object GetService(Type serviceType)
            {
                try
                {
                    return _container.Resolve(serviceType);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                try
                {
                    return _container.ResolveAll(serviceType, true);
                }
                catch
                {
                    return Enumerable.Empty<object>();
                }
            }

            public IDependencyScope BeginScope()
            {
                return this;
            }
        }

        private class CommandHandlingHttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            // We want to be very explicit which controllers we want to use.
            // Also we want our controllers internal.

            public ICollection<Type> GetControllerTypes(IAssembliesResolver _)
            {
                return new[] { typeof(CommandController) };
            }
        }
    }
}