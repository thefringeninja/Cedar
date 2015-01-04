namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using Cedar.Annotations;
    using Cedar.Commands.TypeResolution;
    using CuttingEdge.Conditions;

    public class CommandHandlingSettings
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ResolveCommandType _resolveCommandType;
        private ParseMediaType _parseMediaType = MediaTypeParsers.AllCombined;
        private CreateProblemDetails _createProblemDetails;

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

        public CreateProblemDetails CreateProblemDetails
        {
            get
            {
                if(_createProblemDetails == null)
                {
                    return _ => null;
                }
                return _createProblemDetails;
            }
            set { _createProblemDetails = value; }
        }

        public ICommandHandlerResolver HandlerResolver
        {
            get { return _handlerResolver; }
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