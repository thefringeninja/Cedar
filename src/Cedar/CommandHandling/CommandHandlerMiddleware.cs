namespace Cedar.CommandHandling
{
    using System;
    using System.IO;
    using System.Security.Claims;
    using System.Text;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.Exceptions;
    using Cedar.Hosting;
    using Microsoft.Owin;
    using Newtonsoft.Json;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public static class CommandHandlerMiddleware
    {
        public static MidFunc HandleCommands(
            ICommandTypeFromHttpContentType commandTypeFromHttpContentType,
            ICommandDispatcher commandDispatcher,
            IExceptionToModelConverter exceptionToModelConverter)
        {
            Guard.EnsureNotNull(commandTypeFromHttpContentType, "commandTypeFromHttpContentType");
            Guard.EnsureNotNull(commandDispatcher, "commandDispatcher");
            Guard.EnsureNotNull(exceptionToModelConverter, "exceptionToModelConverter");

            JsonSerializer jsonSerializer = JsonSerializer.Create(DefaultJsonSerializerSettings.Settings);

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
                    Type commandType = commandTypeFromHttpContentType.GetCommandType(contentType);
                    object command;
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        command = jsonSerializer.Deserialize(streamReader, commandType);
                    }
                    var user = context.Request.User as ClaimsPrincipal;
                    var commandContext = new CommandContext(commandId, context.Request.CallCancelled, user);
                    await commandDispatcher.Dispatch(commandContext, command);
                }
                catch (Exception ex)
                {
                    HandleInternalServerError(context, ex, exceptionToModelConverter);
                    return;
                }
                context.Response.StatusCode = 202;
                context.Response.ReasonPhrase = "Accepted";
            };
        }

        private static void HandleInternalServerError(IOwinContext context, Exception ex, IExceptionToModelConverter exceptionToModelConverter)
        {
            context.Response.StatusCode = 500;
            context.Response.ReasonPhrase = "Internal Server Error";
            context.Response.ContentType = "application/json";
            ExceptionModel exceptionModel = exceptionToModelConverter.Convert(ex);
            string exceptionJson = JsonConvert.SerializeObject(exceptionModel, DefaultJsonSerializerSettings.Settings);
            byte[] exceptionBytes = Encoding.UTF8.GetBytes(exceptionJson);
            context.Response.ContentLength = exceptionBytes.Length;
            context.Response.Write(exceptionBytes);
        }
    }
}