namespace Cedar.Commands
{
    using System;
    using System.IO;
    using Cedar.Annotations;
    using Cedar.Handlers;

    public abstract class CommandHandlerSettings
    {
        private readonly IHandlerResolver _handlerResolver;
        private readonly ICommandTypeResolver _commandTypeResolver;
        private readonly IExceptionToModelConverter _exceptionToModelConverter;

        protected CommandHandlerSettings([NotNull] IHandlerResolver handlerResolver, [NotNull] ICommandTypeResolver commandTypeResolver,
            IExceptionToModelConverter exceptionToModelConverter)
        {
            Guard.EnsureNotNull(handlerResolver, "dispatcher");
            Guard.EnsureNotNull(commandTypeResolver, "commandTypeResolver");

            _handlerResolver = handlerResolver;
            _commandTypeResolver = commandTypeResolver;
            _exceptionToModelConverter = exceptionToModelConverter ?? new ExceptionToModelConverter();
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

        public abstract object Deserialize(TextReader reader, Type type);
    }
}