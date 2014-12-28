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
        private readonly TryResolveType _typeResolver;
        private ISerializer _serializer;
        private IExceptionToModelConverter _exceptionToModelConverter;
        private TryParseMediaType _mediaTypeParser = MediaTypeParsers.AllCombined;

        public CommandHandlingSettings(
            [NotNull] IHandlerResolver handlerResolver,
            [NotNull] TryResolveType typeResolver)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(typeResolver, "typeResolver").IsNotNull();

            _handlerResolver = handlerResolver;
            _typeResolver = typeResolver;
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

        public ISerializer Serializer //TODO Needs to be a collection serializers for content negotiation.
        {
            get { return _serializer ?? DefaultSerializer; }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _serializer = value;
            }
        }

        public TryResolveType TypeResolver
        {
            get { return _typeResolver; }
        }

        public TryParseMediaType MediaTypeParser
        {
            get { return _mediaTypeParser; }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _mediaTypeParser = value;
            }
        }
    }
}