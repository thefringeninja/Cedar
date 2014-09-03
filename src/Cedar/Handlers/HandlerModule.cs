﻿namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task Handler<TMessage>(TMessage message, CancellationToken ct);
    public delegate Handler<TMessage> Pipe<TMessage>(Handler<TMessage> next);

    public class HandlerModule
    {
        private delegate Task NonGenericHandler(object message, CancellationToken ct);

        private readonly Dictionary<Type, List<NonGenericHandler>> _handlersByMessageType =
            new Dictionary<Type, List<NonGenericHandler>>(); 

        public IHandlerBuilder<TMessage> For<TMessage>()
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

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
        {
            if (!_handlersByMessageType.ContainsKey(typeof(TMessage)))
            {
                return null;
            }
            return _handlersByMessageType[typeof(TMessage)]
                .Select(handler => new Handler<TMessage>((message, ct) => handler(message, ct)));
        } 

        private class HandlerBuilder<TMessage> : IHandlerBuilder<TMessage>
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