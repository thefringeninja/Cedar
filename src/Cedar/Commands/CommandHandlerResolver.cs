namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using Cedar.Handlers;
    using Cedar.TinyIoC;

    public class CommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly TinyIoCContainer _container = new TinyIoCContainer();
        private readonly HashSet<Type> _knownCommandTypes = new HashSet<Type>();

        public CommandHandlerResolver(params CommandHandlerModule[] commandHandlerModules)
        {
            foreach(var module in commandHandlerModules)
            {
                foreach(var handlerRegistration in module.HandlerRegistrations)
                {
                    if (!_knownCommandTypes.Add(handlerRegistration.MessageType))
                    {
                        throw new InvalidOperationException(
                            "Attempt to register multiple handlers for command type {0}"
                                .FormatWith(handlerRegistration.MessageType));
                    }

                    _container.Register(
                        handlerRegistration.RegistrationType,
                        handlerRegistration.HandlerInstance);
                }
            }
        }

        public IEnumerable<Type> KnownCommandTypes
        {
            get { return _knownCommandTypes; }
        }

        public Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class
        {
            return _container.Resolve<Handler<CommandMessage<TCommand>>>();
        }
    }
}