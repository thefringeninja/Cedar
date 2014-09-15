namespace Cedar.Queries
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        public void For<TInput, TOutput>(IQueryHandler<TInput, TOutput> handler, params Pipe<QueryMessage<TInput, TOutput>>[] pipeline)
        {
            pipeline.Aggregate(_inner.For<QueryMessage<TInput, TOutput>>(), (builder, pipe) => builder.Pipe(pipe),
                BuildHandler(handler));

            _registeredTypes.Add(typeof(TInput));
            _registeredTypes.Add(typeof(TOutput));
        }

        private static Func<IHandlerBuilder<QueryMessage<TInput, TOutput>>, ICreateHandlerBuilder> BuildHandler<TInput, TOutput>(IQueryHandler<TInput, TOutput> handler)
        {
            return builder => builder.Handle((message, ct) => HandleQuery(message, ct, handler));
            
        }
        private static async Task HandleQuery<TInput, TOutput>(QueryMessage<TInput, TOutput> message,
            CancellationToken ct, IQueryHandler<TInput, TOutput> handler)
        {
            var output = await handler.PerformQuery(message.Input);
            message.Source.TrySetResult(output);
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