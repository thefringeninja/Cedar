namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.CommandHandling.ExceptionHandling;
    using Cedar.CommandHandling.Serialization;
    using Nancy;
    using Nancy.Security;

    public class CommandModule : NancyModule
    {
        public CommandModule(
            ICommandTypeResolver commandTypeResolver,
            ICommandDispatcher commandDispatcher,
            IEnumerable<ICommandExceptionHandler> exceptionHandlers,
            IEnumerable<ICommandDeserializer> commandDeserializers,
            string modulePath = "/commands")
            : base(modulePath)
        {
            Put["/{Id}", true] = async (parameters, ct) =>
            {
                try
                {
                    Guid commandId = parameters.Id;
                    string contentType = Request.Headers["Content-Type"].Single();
                    Type commandType = commandTypeResolver.GetCommandType(contentType);
                    ICommandDeserializer commandDeserializer = commandDeserializers.Single(s => s.Handles(contentType));
                    object command = await commandDeserializer.Deserialize(Context.Request.Body, commandType);
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