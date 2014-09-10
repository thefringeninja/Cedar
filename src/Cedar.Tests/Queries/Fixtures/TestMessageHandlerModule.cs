namespace Cedar.Queries.Fixtures
{
    using System;
    using System.Threading.Tasks;

    public class TestHandlerModule : QueryHandlerModule
    {
        public TestHandlerModule()
        {
            For(new ThrowingTestQueryHandler());
            For(new TestQueryHandler());
        }

        class ThrowingTestQueryHandler : IQueryHandler<TestQueryWhoseHandlerThrows, TestQueryResponse>
        {
            public Task<TestQueryResponse> PerformQuery(TestQueryWhoseHandlerThrows input)
            {
                throw new InvalidOperationException();
            }
        }

        class TestQueryHandler : IQueryHandler<TestQuery, TestQueryResponse>
        {
            public Task<TestQueryResponse> PerformQuery(TestQuery input)
            {
                return Task.FromResult(new TestQueryResponse());
            }
        }
    }
}