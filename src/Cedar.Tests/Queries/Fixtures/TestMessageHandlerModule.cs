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
                .Handle(async (message, __) => message.Source.SetResult(new TestQueryResponse()));

            For<TestQueryWhichReturnsNull, TestQueryWhichReturnsNullResponse>()
                .Handle(async (message, __) => message.Source.SetResult(null));
        
            For<TestQueryWithNoReturnType, object>()
                .Handle(async (message, __) => message.Source.SetResult(null));

        }
    }
}