namespace Cedar.Commands
{
    using Cedar.Annotations;
    using Cedar.Commands.Client;
    using Cedar.Handlers;
    using Newtonsoft.Json;

    public class CommandHandlerSettings
    {
        private readonly IHandlerResolver _handlerResolver;
        private readonly ICommandTypeResolver _commandTypeResolver;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        public CommandHandlerSettings(
            [NotNull] IHandlerResolver handlerResolver,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
        {
            Guard.EnsureNotNull(handlerResolver, "dispatcher");
            Guard.EnsureNotNull(commandTypeResolver, "commandTypeResolver");

            _handlerResolver = handlerResolver;
            _commandTypeResolver = commandTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializerSettings = serializerSettings ?? DefaultJsonSerializerSettings.Settings;
        }

        public IHandlerResolver HandlerResolver
        {
            get { return _handlerResolver; }
        }

        public ICommandTypeResolver CommandTypeResolver
        {
            get { return _commandTypeResolver; }
        }

        public IExceptionToModelConverter ExceptionToModelConverter
        {
            get { return _exceptionToModelConverter; }
        }
        public JsonSerializerSettings SerializerSettings
        {
            get { return _serializerSettings; }
        }
    }
}