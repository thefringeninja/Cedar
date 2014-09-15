namespace Cedar.Queries.Fixtures
{
    using System;
    using System.Threading.Tasks;

    public class TestHandlerModule : QueryHandlerModule
    {
        public TestHandlerModule()
        {
            For<TestQueryWhoseHandlerThrows, TestQueryResponse>()
                .HandleQuery(_ =>
                {
                    throw new InvalidOperationException();
                });
            For<TestQuery, TestQueryResponse>()
                .HandleQuery(_ => Task.FromResult(new TestQueryResponse()));
        }
    }
}