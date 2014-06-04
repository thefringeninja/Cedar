namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.CommandHandling.ExceptionHandling;
    using Cedar.CommandHandling.Serialization;
    using Cedar.Hosting;
    using Nancy;
    using Nancy.Security;
    using Newtonsoft.Json;

    public class HeaderBasedCommandModule : NancyModule
    {
        public HeaderBasedCommandModule(
            ICommandTypeResolver commandTypeResolver,
            ICommandDispatcher commandDispatcher,
            IEnumerable<ICommandExceptionHandler> exceptionHandlers,
            IEnumerable<ICommandDeserializer> commandDeserializers,
            string commandNameHeaderKey)
            : base("/commands")
        {
            Put["/{Id}", true] = async (parameters, ct) =>
            {
                try
                {
                    Guid commandId = parameters.Id;
                    string commandName = Request.Headers[commandNameHeaderKey].Single();
                    Type commandType = commandTypeResolver.GetCommandType(commandName);
                    if (commandType == null)
                    {
                        throw new InvalidOperationException("No command handler found for {0}".FormatWith(commandName));
                    }
                    string contentType = Request.Headers["Content-Type"].Single();
                    ICommandDeserializer commandDeserializer = commandDeserializers.Single(s => s.Handles(contentType));
                    commandDeserializer.Deserialize(Context.Request.Body, commandType);

                    //TODO Support custom (de)serializers? 
                    string jsonBody;
                    using (var streamReader = new StreamReader(Context.Request.Body))
                    {
                        jsonBody = await streamReader.ReadToEndAsync();
                    }
                   
                    object command = JsonConvert.DeserializeObject(jsonBody, commandType, DefaultJsonSerializerSettings.Settings);
                    ClaimsPrincipal user = Context.GetAuthenticationManager().User;
                    var commandContext = new CommandContext(commandId, ct, user);
                    await commandDispatcher.Dispatch(commandContext, command);
                }
                catch (Exception ex)
                {
                    ICommandExceptionHandler exceptionHandler = exceptionHandlers.SingleOrDefault(h => h.Handles(ex));
                    if (exceptionHandler != null)
                    {
                        return exceptionHandler.Handle(ex, Negotiate);
                    }
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.InternalServerError)
                        .WithReasonPhrase(ex.Message);
                }
                return HttpStatusCode.Accepted;
            };
        }
    }
}