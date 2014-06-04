namespace Cedar.Hosting
{
    using System;
    using System.IO;
    using System.Security.Claims;
    using System.Threading;
    using Cedar.Domain;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class CommandModule : NancyModule
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public CommandModule(
            ICommandTypeResolver commandTypeResolver,
            ICommandDispatcher commandDispatcher,
            string commandNameHeaderKey)
            : base("/commands")
        {
            Put["/{Id}", true] = async (parameters, ct) =>
            {
                Guid commandId = parameters.Id;
                string jsonBody;
                using (var streamReader = new StreamReader(Context.Request.Body))
                {
                    jsonBody = await streamReader.ReadToEndAsync();
                }
                string commandName = Request.Headers[commandNameHeaderKey].Single();
                string contentType = Request.Headers["Content-Type"].Single();

                Type commandType = commandTypeResolver.GetCommandType(commandName, contentType);
                if (commandType == null)
                {
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.InternalServerError)
                        .WithReasonPhrase("No command handler found for {0}".FormatWith(commandName));
                }
                object command = JsonConvert.DeserializeObject(jsonBody, commandType, JsonSerializerSettings);
                
                try
                {
                    ClaimsPrincipal user = Context.GetAuthenticationManager().User;
                    var commandContext = new CommandContext(commandId, ct, user);
                    await commandDispatcher.Dispatch(commandContext, command);
                }
                catch (CommandValidationException ex)
                {
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.BadRequest)
                        .WithReasonPhrase("Command validation failed")
                        .WithModel(ex.ToExceptionResponse());
                }
                catch (Exception ex)
                {
                    return Negotiate
                        .WithStatusCode(HttpStatusCode.InternalServerError)
                        .WithReasonPhrase(ex.Message);
                }
                return HttpStatusCode.Accepted;
            };
        }

        private class CommandContext : ICommandContext
        {
            private readonly CancellationToken _cancellationToken;
            private readonly ClaimsPrincipal _user;
            private readonly Guid _commandId;

            public CommandContext(Guid commandId, CancellationToken cancellationToken, ClaimsPrincipal user)
            {
                _commandId = commandId;
                _cancellationToken = cancellationToken;
                _user = user;
            }

            public Guid CommandId
            {
                get { return _commandId; }
            }

            public CancellationToken CancellationToken
            {
                get { return _cancellationToken; }
            }

            public ClaimsPrincipal User
            {
                get { return _user; }
            }
        }
    }
}