namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.Commands.TypeResolution;
    using Cedar.ExceptionModels;
    using Cedar.Serialization;
    using CuttingEdge.Conditions;

    public class CommandHandlingSettings
    {
        private static readonly ISerializer DefaultSerializer = new DefaultJsonSerializer();
        private static readonly IExceptionToModelConverter DefaultExceptionToModelConverter = new ExceptionToModelConverter();

        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ResolveCommandType _resolveCommandType;
        private ResolveSerializer _resolveSerializer;
        private IExceptionToModelConverter _exceptionToModelConverter;
        private ParseMediaType _parseMediaType = MediaTypeParsers.AllCombined;


        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlingSettings"/> class using
        ///     <see cref="CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix"/> as the command type resolver.
        /// </summary>
        /// <param name="handlerResolver">The handler resolver.</param>
        public CommandHandlingSettings([NotNull] CommandHandlerResolver handlerResolver)
            : this(handlerResolver, handlerResolver.KnownCommandTypes)
        { } 

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlingSettings"/> class using
        ///     <see cref="CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix"/> as the command type resolver.
        /// </summary>
        /// <param name="handlerResolver">The handler resolver.</param>
        /// <param name="knownCommandTypes">The known command types.</param>
        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] IEnumerable<Type> knownCommandTypes) 
            : this(handlerResolver, CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix(knownCommandTypes))
        { } 

        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] ResolveCommandType resolveCommandType)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(resolveCommandType, "ResolveCommandType").IsNotNull();

            _handlerResolver = handlerResolver;
            _resolveCommandType = resolveCommandType;
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

        public ResolveSerializer ResolveSerializer
        {
            get { return _resolveSerializer ?? SerializationResolvers.Default(); }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _resolveSerializer = value;
            }
        }

        public ResolveCommandType ResolveCommandType
        {
            get { return _resolveCommandType; }
        }

        public ParseMediaType ParseMediaType
        {
            get { return _parseMediaType; }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _parseMediaType = value;
            }
        }
    }
}