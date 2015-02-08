namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task Handler<TMessage>(TMessage message, CancellationToken ct)
        where TMessage: class;

    public delegate void HandlerSync<TMessage>(TMessage message) 
        where TMessage : class;

    public delegate Handler<TMessage> Pipe<TMessage>(Handler<TMessage> next) 
        where TMessage : class;

    /// <summary>
    /// Represents a collection of handlers pipelines.
    /// </summary>
    public class HandlerModule : ICreateHandlerBuilder
    {
        private readonly List<HandlerRegistration> _handlerRegistrations = new List<HandlerRegistration>();

        internal IEnumerable<HandlerRegistration> HandlerRegistrations
        {
            get { return _handlerRegistrations; }
        }

        /// <summary>
        /// Starts to build a handler pipeline for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message the pipeline will handle.</typeparam>
        /// <returns>A a handler builder to continue defining the pipeline.</returns>
        public IHandlerBuilder<TMessage> For<TMessage>() where TMessage : class
        {
            return new HandlerBuilder<TMessage>(_handlerRegistrations.Add);
        }

        private class HandlerBuilder<TMessage> : IHandlerBuilder<TMessage> where TMessage : class
        {
            private readonly Action<HandlerRegistration> _registerHandler;
            private readonly Stack<Pipe<TMessage>> _pipes = new Stack<Pipe<TMessage>>();

            public HandlerBuilder(Action<HandlerRegistration> registerHandler)
            {
                _registerHandler = registerHandler;
            }

            public IHandlerBuilder<TMessage> Pipe(Pipe<TMessage> pipe)
            {
                _pipes.Push(pipe);
                return this;
            }

            public Handler<TMessage> Handle(Handler<TMessage> handler)
            {
                while (_pipes.Count > 0)
                {
                    var pipe = _pipes.Pop();
                    handler = pipe(handler);
                }

                _registerHandler(new HandlerRegistration(typeof(Handler<TMessage>), handler));

                return handler;
            }
        }
    }
}