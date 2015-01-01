namespace Cedar.Commands
{
    using System;
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
    using Cedar.TypeResolution;

    internal class CommandController : ApiController
    {
        internal static readonly MethodInfo DispatchCommandMethodInfo = typeof(CommandController)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly CommandHandlingSettings _settings;

        public CommandController(CommandHandlingSettings settings)
        {
            _settings = settings;
        }

        [Route("{commandId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCommand(Guid commandId, CancellationToken cancellationToken)
        {
            Type commandType = GetCommandType();
            object command = await DeserializeCommand(commandType);
            var user = (User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
            MethodInfo dispatchCommandMethod = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());

            Func<Task> func = async () => await ((Task)dispatchCommandMethod.Invoke(null,
               new[]
                {
                    _settings.HandlerResolver, commandId, user, command, cancellationToken
                })).NotOnCapturedContext();

            HttpResponseMessage response = await func
                .ExecuteWithExceptionHandling_ThisIsToBeReplaced(_settings.ExceptionToModelConverter, _settings.Serializer) 
                ?? new HttpResponseMessage(HttpStatusCode.Accepted);

            return response;
        }

        private Type GetCommandType()
        {
            string mediaType = Request.Content.Headers.ContentType.MediaType;
            IParsedMediaType parsedMediaType = _settings.MediaTypeParser(mediaType);
            if (parsedMediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            Type commandType = _settings.CommandTypeResolver(parsedMediaType.TypeName, parsedMediaType.Version);
            if (commandType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            return commandType;
        }

        private async Task<object> DeserializeCommand(Type commandType)
        {
            using (var streamReader = new StreamReader(await Request.Content.ReadAsStreamAsync()))
            {
                return _settings.Serializer.Deserialize(streamReader, commandType);
            }
        }

        [UsedImplicitly]
        private static async Task DispatchCommand<TCommand>(
            ICommandHandlerResolver handlerResolver,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            await handlerResolver.Resolve<TCommand>()(commandMessage, cancellationToken);
        }
    }
}