namespace Cedar.Commands
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.ContentNegotiation;
    using Microsoft.Owin;

    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public static class CommandHandlingMiddleware
    {
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);

        public static MidFunc HandleCommands(HandlerSettings options)
        {
            Guard.EnsureNotNull(options, "options");

            return next => env =>
            {
                // PUT "/{guid}" with a Json body.
                var context = new OwinContext(env);
                if (!context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a PUT, pass through.
                    return next(env);
                }
                var commandIdString = context.Request.Path.Value.Substring(1);
                Guid commandId;
                if (!Guid.TryParse(commandIdString, out commandId))
                {
                    // Resource is not a GUID, pass through
                    return next(env);
                }
                try
                {
                    return HandleCommand(commandId, options, context);
                }
                catch (InvalidOperationException ex)
                {
                    return context.HandleBadRequest(ex, options);
                }
                catch (Exception ex)
                {
                    return context.HandleInternalServerError(ex, options);
                }
            };
        }

        private static async Task HandleCommand(Guid commandId, HandlerSettings options, OwinContext context)
        {
            string contentType = context.Request.ContentType;
            if (!contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
            {
                // Not a json entity, bad request
                await context.HandleUnsupportedMediaType(new NotSupportedException(), options);
                return;
            }
            Type commandType = options.ContentTypeMapper.GetFromContentType(contentType);
            object command;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                command = options.Deserialize(streamReader, commandType);
            }
            var user = (context.Request.User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            var dispatchCommand = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());
            await (Task) dispatchCommand.Invoke(null, new[]
            {
                options.HandlerModules, commandId, user, command, context.Request.CallCancelled
            });
            context.Response.StatusCode = 202;
            context.Response.ReasonPhrase = "Accepted";
        }
    }
}