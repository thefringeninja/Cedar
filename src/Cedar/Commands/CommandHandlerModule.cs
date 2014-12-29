namespace Cedar.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Handlers;

    public class CommandHandlerModule : ICommandHandlerResolver, IHandlerResolver, IEnumerable<Type>
    {
        private readonly ICollection<Type> _registeredTypes;
        private readonly HandlerModule _inner;

        public CommandHandlerModule()
        {
            _registeredTypes = new HashSet<Type>();
            _inner = new HandlerModule();
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
        {
            return _inner.GetHandlersFor<TMessage>();
        }

        public IHandlerBuilder<CommandMessage<TMessage>> For<TMessage>()
        {
            _registeredTypes.Add(typeof(TMessage));
            return _inner.For<CommandMessage<TMessage>>();
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _registeredTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Handler<TCommand> Resolve<TCommand>() where TCommand : class
        {
            var x = GetHandlersFor<TCommand>().SingleOrDefault();
            return x;
        }
    }
}