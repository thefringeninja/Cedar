namespace Cedar.Commands
{
    using Cedar.Annotations;
    using Cedar.Commands.Client;
    using Cedar.Handlers;
    using Newtonsoft.Json;

    public class CommandHandlerSettings
    {
        private readonly IDispatcher _dispatcher;
        private readonly ICommandTypeResolver _commandTypeResolver;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        public CommandHandlerSettings(
            [NotNull] IDispatcher dispatchDispatcher,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter = null,
            JsonSerializerSettings serializerSettings = null)
        {
            Guard.EnsureNotNull(dispatchDispatcher, "dispatchMessage");
            Guard.EnsureNotNull(commandTypeResolver, "commandTypeFromHttpContentType");

            _dispatcher = dispatchDispatcher;
            _commandTypeResolver = commandTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
            _serializerSettings = serializerSettings ?? DefaultJsonSerializerSettings.Settings;
        }

        public IDispatcher Dispatcher
        {
            get { return _dispatcher; }
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