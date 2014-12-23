namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.TypeResolution;

    internal class CommandController : ApiController
    {
        internal static readonly MethodInfo DispatchCommandMethodInfo = typeof(CommandController)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly HandlerSettings _handlerSettings;

        public CommandController(HandlerSettings handlerSettings)
        {
            _handlerSettings = handlerSettings;
        }

        [Route("{commandId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCommand(Guid commandId, CancellationToken cancellationToken)
        {
            var commandType = GetCommandType();
            var command = await DeserializeCommand(commandType);
            var user = (User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            var dispatchCommand = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());

            Func<Task> act = async () => await ((Task)dispatchCommand.Invoke(null,
               new[]
                {
                    _handlerSettings.HandlerResolvers, commandId, user, command, cancellationToken
                })).NotOnCapturedContext();

            var response = await act.ExecuteWithExceptionHandling_ThisIsToBeReplaced(_handlerSettings) 
                ?? new HttpResponseMessage(HttpStatusCode.Accepted);

            return response;
        }

        private Type GetCommandType()
        {
            string contentType = Request.Content.Headers.ContentType.MediaType;

            Type commandType;
            if (!contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
               || (commandType = _handlerSettings.RequestTypeResolver.ResolveInputType(new CedarRequest(Request.GetOwinEnvironment()))) == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            return commandType;
        }

        private async Task<object> DeserializeCommand(Type commandType)
        {
            using (var streamReader = new StreamReader(await Request.Content.ReadAsStreamAsync()))
            {
                return _handlerSettings.Serializer.Deserialize(streamReader, commandType);
            }
        }

        [UsedImplicitly]
        private static Task DispatchCommand<TCommand>(
            IEnumerable<IHandlerResolver> handlerResolvers,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            Guard.EnsureNotNull(handlerResolvers, "handlerModules");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(command, "command");

            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return handlerResolvers.DispatchSingle(commandMessage, cancellationToken);
        }
    }
}