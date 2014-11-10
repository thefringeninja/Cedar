namespace Cedar.Queries.Fixtures
{
    using System;
    using Cedar.Handlers;

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
                .Handle(message => message.Source.SetResult(new TestQueryResponse()));

            For<TestQueryWhichReturnsNull, TestQueryWhichReturnsNullResponse>()
                .Handle(message => message.Source.SetResult(null));
        
            For<TestQueryWithNoReturnType, object>()
                .Handle(message => message.Source.SetResult(null));
        }
    }
}