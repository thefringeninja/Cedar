namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Cedar.Client;
    using Cedar.CommandHandling.Dispatching;
    using Nancy;
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
                    var exceptionResponse = new ExceptionResponse
                    {
                        ExeptionType = ex.GetType().Name,
                        Message = ex.Message
                    };
                    return Negotiate
                        .WithModel(exceptionResponse)
                        .WithStatusCode(500);
                }
                return HttpStatusCode.Accepted;
            };
        }
    }
}