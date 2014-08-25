namespace Cedar.CommandHandling
{
    using Cedar.Annotations;
    using Cedar.CommandHandling.Client;
    using Newtonsoft.Json;

    public class CommandHandlerSettings
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ICommandTypeResolver _commandTypeResolver;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        public CommandHandlerSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
        {
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");
            Guard.EnsureNotNull(commandTypeResolver, "commandTypeFromHttpContentType");

            _handlerResolver = handlerResolver;
            _commandTypeResolver = commandTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializerSettings = serializerSettings ?? DefaultJsonSerializerSettings.Settings;
        }

        public ICommandHandlerResolver HandlerResolver
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