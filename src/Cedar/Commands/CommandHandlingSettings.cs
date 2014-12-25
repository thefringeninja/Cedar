namespace Cedar.Commands
{
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Handlers;
    using Cedar.Serialization;
    using Cedar.TypeResolution;
    using CuttingEdge.Conditions;

    public class CommandHandlingSettings
    {
        private static readonly ISerializer DefaultSerializer = new DefaultJsonSerializer();
        private static readonly IExceptionToModelConverter DefaultExceptionToModelConverter = new ExceptionToModelConverter();

        private readonly IHandlerResolver _handlerResolver;
        private readonly IRequestTypeResolver _requestTypeResolver;
        private ISerializer _serializer;
        private IExceptionToModelConverter _exceptionToModelConverter;

        public CommandHandlingSettings(
            [NotNull] IHandlerResolver handlerResolver,
            [NotNull] IRequestTypeResolver requestTypeResolver)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(requestTypeResolver, "requestTypeResolver").IsNotNull();

            _handlerResolver = handlerResolver;
            _requestTypeResolver = requestTypeResolver;
        }

        public IExceptionToModelConverter ExceptionToModelConverter
        {
            get { return _exceptionToModelConverter ?? DefaultExceptionToModelConverter; }
            set { _exceptionToModelConverter = value; }
        }

        public IHandlerResolver HandlerResolver
        {
            get { return _handlerResolver; }
        }

        public ISerializer Serializer
        {
            get { return _serializer ?? DefaultSerializer; }
            set { _serializer = value; }
        }

        public IRequestTypeResolver RequestTypeResolver
        {
            get { return _requestTypeResolver; }
        }
    }
}