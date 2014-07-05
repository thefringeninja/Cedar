namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Cedar.CommandHandling.Dispatching;
    using Nancy;
    using Nancy.Owin;
    using Nancy.Security;

    public class CommandModule : NancyModule
    {
        public CommandModule(
            ICommandTypeFromHttpContentType commandTypeFromHttpContentType,
            ICommandDispatcher commandDispatcher,
            IEnumerable<ICommandDeserializer> commandDeserializers)
        {
            Put["/{Id}", true] = async (parameters, ct) =>
            {
                try
                {
                    Guid commandId = parameters.Id;
                    string contentType = Request.Headers["Content-Type"].Single();
                    Type commandType = commandTypeFromHttpContentType.GetCommandType(contentType);
                    ICommandDeserializer commandDeserializer = commandDeserializers.Single(s => s.Handles(contentType));
                    object command = await commandDeserializer.Deserialize(Context.Request.Body, commandType);
                    ClaimsPrincipal user = Context.GetAuthenticationManager().User;
                    var commandContext = new CommandContext(commandId, ct, user);
                    await commandDispatcher.Dispatch(commandContext, command);
                }
                catch (Exception ex)
                {
                    //TODO do we want to serialize the exeption back to the client and if so, how?
                    // Header or entity? What to include - message? stack trace etc?
                    return HttpStatusCode.InternalServerError;
                }
                return HttpStatusCode.Accepted;
            };
        }
    }
}