namespace Cedar.Commands
{
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly ITypeResolver _typeResolver;
        private ISerializer _serializer;
        private IExceptionToModelConverter _exceptionToModelConverter;
        private ICollection<TryParseMediaType> _mediaTypeParsers = TypeResolution.MediaTypeParsers.DefaultParsers.ToList();

        public CommandHandlingSettings(
            [NotNull] IHandlerResolver handlerResolver,
            [NotNull] ITypeResolver typeResolver)
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

        public ISerializer Serializer
        {
            get { return _serializer ?? DefaultSerializer; }
            set { _serializer = value; }
        }

        public ITypeResolver TypeResolver
        {
            get { return _typeResolver; }
        }

        public ICollection<TryParseMediaType> MediaTypeParsers
        {
            get { return _mediaTypeParsers; }
            set
            {
                if(value == null)
                {
                    _mediaTypeParsers.Clear();
                }
                _mediaTypeParsers = value;
            }
        }
    }
}