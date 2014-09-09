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
        public static MidFunc HandleCommands(HandlerSettings options)
        {
            Guard.EnsureNotNull(options, "options");

            var dispatchCommandMethodInfo = typeof(HandlerModulesDispatchCommand)
                .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.Public);

            return next => async env =>
            {
                // PUT "/{guid}" with a Json body.
                var context = new OwinContext(env);
                if (!context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a PUT, pass through.
                    await next(env);
                    return;
                }
                var commandIdString = context.Request.Path.Value.Substring(1);
                Guid commandId;
                if (!Guid.TryParse(commandIdString, out commandId))
                {
                    // Resource is not a GUID, pass through
                    await next(env);
                    return;
                }
                try
                {
                    string contentType = context.Request.ContentType;
                    if (!contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Not a json entity, bad request
                        context.Response.StatusCode = 415;
                        context.Response.ReasonPhrase = "Unsupported Media Type";
                        return;
                    }
                    Type commandType = options.ContentTypeMapper.GetFromContentType(contentType);
                    object command;
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        command = options.Deserialize(streamReader, commandType);
                    }
                    var user = (context.Request.User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
                    var dispatchCommand = dispatchCommandMethodInfo.MakeGenericMethod(command.GetType());
                    await (Task)dispatchCommand.Invoke(null, new[]
                    {
                        options.HandlerModules, commandId, user, command, context.Request.CallCancelled
                    });
                }
                catch (InvalidOperationException ex)
                {
                    context.HandleBadRequest(ex, options);
                    return;
                }
                catch (Exception ex)
                {
                    context.HandleInternalServerError(ex, options);
                    return;
                }
                context.Response.StatusCode = 202;
                context.Response.ReasonPhrase = "Accepted";
            };
        }
    }
}