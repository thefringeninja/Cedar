namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Commands.ExceptionModels;
    using Microsoft.Owin;

    public static class CommandHandlingMiddleware
    {
        public static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> HandleCommands(CommandHandlerSettings options)
        {
            Guard.EnsureNotNull(options, "options");

            var dispatchCommandMethodInfo = typeof(HandlerModuleExtensions)
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
                        context.Response.StatusCode = 400;
                        context.Response.ReasonPhrase = "Bad Request";
                        return;
                    }
                    Type commandType = options.CommandTypeResolver.GetFromContentType(contentType);
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
                    HandleBadRequest(context, ex, options);
                }
                catch (Exception ex)
                {
                    HandleInternalServerError(context, ex, options);
                    return;
                }
                context.Response.StatusCode = 202;
                context.Response.ReasonPhrase = "Accepted";
            };
        }

        private static void HandleBadRequest(IOwinContext context, InvalidOperationException ex, CommandHandlerSettings options)
        {
            context.Response.StatusCode = 400;
            context.Response.ReasonPhrase = "Bad Request";
            context.Response.ContentType = "application/json";
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(ex);
            string exceptionJson = options.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            context.Response.Write(exceptionBytes);
        }

        private static void HandleInternalServerError(IOwinContext context, Exception ex, CommandHandlerSettings options)
        {
            context.Response.StatusCode = 500;
            context.Response.ReasonPhrase = "Internal Server Error";
            context.Response.ContentType = "application/json";
            ExceptionModel exceptionModel = options.ExceptionToModelConverter.Convert(ex);
            string exceptionJson = options.Serialize(exceptionModel);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            context.Response.Write(exceptionBytes);
        }
    }
}