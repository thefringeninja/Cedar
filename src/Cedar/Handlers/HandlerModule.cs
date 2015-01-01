namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public class HandlerModule : ICreateHandlerBuilder, IHandlerResolver
    {
        private delegate Task NonGenericHandler(object message, CancellationToken ct);

        private readonly Dictionary<Type, List<NonGenericHandler>> _handlersByMessageType =
            new Dictionary<Type, List<NonGenericHandler>>();

        /// <summary>
        /// Starts to build a handler pipeline for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message the pipeline will handle.</typeparam>
        /// <returns>A a handler builder to continue defining the pipeline.</returns>
        public IHandlerBuilder<TMessage> For<TMessage>() where TMessage : class
        {
            var key = typeof(TMessage);
            List<NonGenericHandler> handlers = _handlersByMessageType.ContainsKey(key) 
                ? _handlersByMessageType[key]
                : new List<NonGenericHandler>();

            var handlerBuilder = new HandlerBuilder<TMessage>();
            handlers.Add((message, ct) => handlerBuilder.Invoke((TMessage)message, ct));
            _handlersByMessageType[key] = handlers;

            return handlerBuilder;
        }

        /// <summary>
        /// Gets the handlers for the specified message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns>The collection of handlers. Null if none exist.</returns>
        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
        {
            if (!_handlersByMessageType.ContainsKey(typeof(TMessage)))
            {
                return Enumerable.Empty<Handler<TMessage>>();
            }
            return _handlersByMessageType[typeof(TMessage)]
                .Select(handler => new Handler<TMessage>((message, ct) => handler(message, ct)));
        } 

        private class HandlerBuilder<TMessage> : IHandlerBuilder<TMessage> where TMessage : class
        {
            private readonly Stack<Pipe<TMessage>> _middlewares = new Stack<Pipe<TMessage>>();
            private Handler<TMessage> _handler;

            internal Task Invoke(TMessage message, CancellationToken ct)
            {
                return _handler(message, ct);
            }

            public IHandlerBuilder<TMessage> Pipe(Pipe<TMessage> pipe)
            {
                _middlewares.Push(pipe);
                return this;
            }

            public void Handle(Handler<TMessage> handler)
            {
                _handler = handler;

                while (_middlewares.Count > 0)
                {
                    var handlerMiddleware = _middlewares.Pop();
                    _handler = handlerMiddleware(_handler);
                }
            }
        }
    }
}