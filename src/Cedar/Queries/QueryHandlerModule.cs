namespace Cedar.Queries
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Cedar.Handlers;

    public class QueryHandlerModule : IHandlerResolver, IEnumerable<Type>
    {
        private readonly HandlerModule _inner;
        private readonly ICollection<Type> _registeredTypes;
        public QueryHandlerModule()
        {
            _inner = new HandlerModule();
            _registeredTypes = new Collection<Type>();
        }

        public IHandlerBuilder<QueryMessage<TInput, TOutput>> For<TInput, TOutput>()
        {
            _registeredTypes.Add(typeof(TInput));
            _registeredTypes.Add(typeof(TOutput));
            return _inner.For<QueryMessage<TInput, TOutput>>();
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
        {
            return _inner.GetHandlersFor<TMessage>();
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _registeredTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}