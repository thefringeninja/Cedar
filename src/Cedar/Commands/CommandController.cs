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

        private readonly CommandHandlingSettings _settings;

        public CommandController(CommandHandlingSettings settings)
        {
            _settings = settings;
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
                    new [] { _settings.HandlerResolver }, commandId, user, command, cancellationToken
                })).NotOnCapturedContext();

            var response = await act.ExecuteWithExceptionHandling_ThisIsToBeReplaced(_settings.ExceptionToModelConverter, _settings.Serializer) 
                ?? new HttpResponseMessage(HttpStatusCode.Accepted);

            return response;
        }

        private Type GetCommandType()
        {
            string mediaType = Request.Content.Headers.ContentType.MediaType;
            IParsedMediaAndSerializationType parsedMediaAndSerializationType = null;
            foreach(var tryParseMediaType in _settings.MediaTypeParsers)
            {
                if(tryParseMediaType(mediaType, out parsedMediaAndSerializationType))
                {
                    break;
                }
            }
            if(parsedMediaAndSerializationType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var commandType = _settings.TypeResolver.Resolve(parsedMediaAndSerializationType);
            if(commandType == null)
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
        private static Task DispatchCommand<TCommand>(
            IEnumerable<IHandlerResolver> handlerResolvers,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return handlerResolvers.DispatchSingle(commandMessage, cancellationToken);
        }
    }
}