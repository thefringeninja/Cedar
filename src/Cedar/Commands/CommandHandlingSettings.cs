namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.ExceptionModels;
    using Cedar.Serialization;
    using Cedar.TypeResolution;
    using CuttingEdge.Conditions;

    public class CommandHandlingSettings
    {
        private static readonly ISerializer DefaultSerializer = new DefaultJsonSerializer();
        private static readonly IExceptionToModelConverter DefaultExceptionToModelConverter = new ExceptionToModelConverter();

        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ResolveCommandType _commandTypeResolver;
        private ISerializer _serializer;
        private IExceptionToModelConverter _exceptionToModelConverter;
        private ParseMediaType _mediaTypeParser = MediaTypeParsers.AllCombined;


        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlingSettings"/> class using
        ///     <see cref="CommandTypeResolvers.FullNameWithVersionSuffix"/> as the command type resolver.
        /// </summary>
        /// <param name="handlerResolver">The handler resolver.</param>
        /// <param name="knownCommandTypes">The known command types.</param>
        public CommandHandlingSettings([NotNull] CommandHandlerResolver handlerResolver)
            : this(handlerResolver, handlerResolver.KnownCommandTypes)
        { } 

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlingSettings"/> class using
        ///     <see cref="CommandTypeResolvers.FullNameWithVersionSuffix"/> as the command type resolver.
        /// </summary>
        /// <param name="handlerResolver">The handler resolver.</param>
        /// <param name="knownCommandTypes">The known command types.</param>
        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] IEnumerable<Type> knownCommandTypes) 
            : this(handlerResolver, CommandTypeResolvers.FullNameWithVersionSuffix(knownCommandTypes))
        { } 

        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] ResolveCommandType commandTypeResolver)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(commandTypeResolver, "commandTypeResolver").IsNotNull();

            _handlerResolver = handlerResolver;
            _commandTypeResolver = commandTypeResolver;
        }

        public IExceptionToModelConverter ExceptionToModelConverter
        {
            get { return _exceptionToModelConverter ?? DefaultExceptionToModelConverter; }
            set { _exceptionToModelConverter = value; }
        }

        public ICommandHandlerResolver HandlerResolver
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

        public ResolveCommandType CommandTypeResolver
        {
            get { return _commandTypeResolver; }
        }

        public ParseMediaType MediaTypeParser
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