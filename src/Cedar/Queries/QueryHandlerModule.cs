namespace Cedar.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;

    public class QueryHandlerModule : IHandlerResolver
    {
        private readonly HandlerModule _inner;

        public QueryHandlerModule()
        {
            _inner = new HandlerModule();
        }

        public void For<TInput, TOutput>(Func<TInput, Task<TOutput>> query, params Pipe<QueryMessage<TInput, TOutput>>[] pipeline)
        {
            var handler = CreateHandler(query);

            pipeline.Aggregate(_inner.For<QueryMessage<TInput, TOutput>>(), (builder, pipe) => builder.Pipe(pipe), BuildHandler(handler));
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


        private static IQueryHandler<TInput, TOutput> CreateHandler<TInput, TOutput>(Func<TInput, Task<TOutput>> query)
        {
            return new DelegateQueryHandler<TInput, TOutput>(query);
        }

        internal class DelegateQueryHandler<TInput, TOutput> : IQueryHandler<TInput, TOutput>
        {
            private readonly Func<TInput, Task<TOutput>> _query;

            public DelegateQueryHandler(Func<TInput, Task<TOutput>> query)
            {
                _query = query;
            }

            public Task<TOutput> PerformQuery(TInput input)
            {
                return _query(input);
            }
        }

        public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>()
        {
            return _inner.GetHandlersFor<TMessage>();
        }
    }
}