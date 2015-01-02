namespace Cedar.Testing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Xunit;

    public class QueryTests
    {
        [Fact]
        public async Task a_passing_query_test_should()
        {
            var result = await Scenario.ForQuery<QueryResponse>(new SomeModule())
                .Given(new SomethingHappened(), new SomethingHappened())
                .When(() => Task.FromResult(new QueryResponse {Value = 2}))
                .ThenShouldEqual(new QueryResponse {Value = 2});

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_query_test_should()
        {
            var result = await Scenario.ForQuery<QueryResponse>(new SomeModule())
                .Given(new SomethingHappened())
                .When(() => Task.FromResult(new QueryResponse { Value = 1 }))
                .ThenShouldEqual(new QueryResponse { Value = 2 });

            Assert.False(result.Passed);
        }
        
        private class SomeModule : IHandlerResolver
        {
            private readonly IHandlerResolver[] _inner;

            public SomeModule()
            {
                var queries = new ReadModel();
                var projections = new SomeProjection(queries);
                _inner = new IHandlerResolver[]
                {
                    projections
                };
            }
            private class ReadModel
            {
                public int Count;
            }

            private class SomeProjection : HandlerModule
            {
                public SomeProjection(ReadModel readModel)
                {
                    For<DomainEventMessage<SomethingHappened>>()
                        .Handle(_ => readModel.Count++);
                }
            }

            public IEnumerable<Handler<TMessage>> GetHandlersFor<TMessage>() where TMessage : class
            {
                return (from module in _inner
                    from handler in module.GetHandlersFor<TMessage>()
                    select handler);
            }
        }

        private class SomethingHappened
        {}

        private class Query
        {}

        private class QueryResponse
        {
            public int Value { get; set; }
        }
    }
}