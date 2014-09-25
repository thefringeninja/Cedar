namespace Cedar.Queries.Fixtures
{
    using System;
    using System.Threading.Tasks;

    public class TestHandlerModule : QueryHandlerModule
    {
        public TestHandlerModule()
        {
            For<TestQueryWhoseHandlerThrows, TestQueryResponse>()
                .Handle((_, __) =>
                {
                    throw new InvalidOperationException();
                });
            For<TestQuery, TestQueryResponse>()
                .Handle((_, __) => Task.FromResult(new TestQueryResponse()));
        }
    }
}