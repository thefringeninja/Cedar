namespace Cedar.Testing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.Queries;
    using Xunit;

    public class QueryTests
    {
        [Fact]
        public async Task a_passing_query_test_should()
        {
            var result = await Scenario.ForQuery<Query, QueryResponse>(new SomeModule())
                .Given(new SomethingHappened(), new SomethingHappened())
                .When(new Query())
                .ThenShouldEqual(new QueryResponse {Value = 2});

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_query_test_should()
        {
            var result = await Scenario.ForQuery<Query, QueryResponse>(new SomeModule())
                .Given(new SomethingHappened())
                .When(new Query())
                .ThenShouldEqual(new QueryResponse { Value = 2 });

            Assert.False(result.Passed);
        }
        
        class SomeModule : IHandlerResolver
        {
            private readonly IHandlerResolver[] _inner;

            public SomeModule()
            {
                var queries = new SomeQueryHandler();
                var projections = new SomeProjection(queries);
                _inner = new IHandlerResolver[]
                {
                    queries, projections
                };
            }
            class SomeQueryHandler : QueryHandlerModule
            {
                public int count;

                public SomeQueryHandler()
                {
                    For<Query, QueryResponse>()
                        .HandleQuery((query, token) => Task.FromResult(new QueryResponse {Value = count}));
                }
            }

            class SomeProjection : HandlerModule
            {
                public SomeProjection(SomeQueryHandler readModel)
                {
                    For<DomainEventMessage<SomethingHappened>>()
                        .Handle(_ => readModel.count++);
                }
            }

            public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
            {
                return (from module in _inner
                    from handler in module.GetHandlersFor<TMessage>()
                    select handler);
            }
        }

        class SomethingHappened { }

        class Query
        {
        }

        class QueryResponse
        {
            public int Value { get; set; }
        }
    }
}