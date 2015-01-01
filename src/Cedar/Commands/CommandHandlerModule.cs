namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using Cedar.Handlers;

    public class CommandHandlerModule
    {
        private readonly HashSet<CommandHandlerRegistration> _handlerRegistrations
            = new HashSet<CommandHandlerRegistration>(CommandHandlerRegistration.MessageTypeComparer);

        internal HashSet<CommandHandlerRegistration> HandlerRegistrations
        {
            get { return _handlerRegistrations; }
        }

        public IHandlerBuilder<CommandMessage<TCommand>> For<TCommand>()
            where TCommand : class
        {
            return new HandlerBuilder<TCommand>(handlerRegistration =>
            {
                if(!_handlerRegistrations.Add(handlerRegistration))
                {
                    throw new InvalidOperationException(
                        "Attempt to register multiple handlers for command type {0}".FormatWith(typeof(TCommand)));
                }
            });
        }

        private class HandlerBuilder<TCommand> : IHandlerBuilder<CommandMessage<TCommand>>
            where TCommand : class
        {
            private readonly Stack<Pipe<CommandMessage<TCommand>>> _pipes = new Stack<Pipe<CommandMessage<TCommand>>>();
            private readonly Action<CommandHandlerRegistration> _registerHandler;

            internal HandlerBuilder(Action<CommandHandlerRegistration> registerHandler)
            {
                _registerHandler = registerHandler;
            }

            public IHandlerBuilder<CommandMessage<TCommand>> Pipe(Pipe<CommandMessage<TCommand>> pipe)
            {
                _pipes.Push(pipe);
                return this;
            }

            public void Handle(Handler<CommandMessage<TCommand>> handler)
            {
                while(_pipes.Count > 0)
                {
                    var pipe = _pipes.Pop();
                    handler = pipe(handler);
                }

                var registrationType = typeof(Handler<CommandMessage<TCommand>>);

                _registerHandler(new CommandHandlerRegistration(
                    typeof(TCommand),
                    registrationType,
                    handler));
            }
        }
    }
}