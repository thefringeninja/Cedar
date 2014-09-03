namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.Handlers;

    public abstract class CommandHandlerSettings
    {
        private readonly IEnumerable<HandlerModule> _handlerModules;
        private readonly ICommandTypeResolver _commandTypeResolver;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        protected CommandHandlerSettings(
            [NotNull] IEnumerable<HandlerModule> handlerModules,
            [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter)
        {
            Guard.EnsureNotNull(handlerModules, "handlerResolver");
            Guard.EnsureNotNull(commandTypeResolver, "commandTypeResolver");

            _handlerModules = handlerModules;
            _commandTypeResolver = commandTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
        }

        public IEnumerable<HandlerModule> HandlerModules
        {
            get { return _handlerModules; }
        }

        public ICommandTypeResolver CommandTypeResolver
        {
            get { return _commandTypeResolver; }
        }

        public IExceptionToModelConverter ExceptionToModelConverter
        {
            get { return _exceptionToModelConverter; }
        }

        public abstract object Deserialize(TextReader reader, Type type);

        public abstract void Serialize(TextWriter writer, object target);
    }
}