namespace Cedar.Testing
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Microsoft.Owin;
    using Xunit;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >
    >;

    public class QueryTests
    {
        private readonly SomeModule _handlerResolver;

        public QueryTests()
        {
            _handlerResolver = new SomeModule();
        }

        [Fact]
        public async Task a_passing_query_test_should()
        {
            var result = await Scenario.ForQuery(_handlerResolver, _handlerResolver.HttpApplication)
                .Given(new SomethingHappened(), new SomethingHappened())
                .When(new HttpRequestMessage(HttpMethod.Get, "/query-response"))
                .ThenShould(response => response.Headers.ContentType.MediaType == "application/json");

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_query_test_should()
        {
            var result = await Scenario.ForQuery(_handlerResolver, _handlerResolver.HttpApplication)
                .Given(new SomethingHappened())
                .When(new HttpRequestMessage(HttpMethod.Get, "/query-response"))
                .ThenShould(response => response.Headers.ContentType.MediaType != "application/json");

            Assert.False(result.Passed);
        }
        
        private class SomeModule : HandlerModule
        {
            private readonly ReadModel _readModel;

            public SomeModule()
            {
                _readModel = new ReadModel();

                For<DomainEventMessage<SomethingHappened>>()
                    .Handle(_ => _readModel.Count++);

            }
            private class ReadModel
            {
                public int Count;
            }
            
            public MidFunc HttpApplication
            {
                get
                {
                    return next => async env =>
                    {
                        var context = new OwinContext(env);

                        if(false == context.Request.Path.StartsWithSegments(new PathString("/query-response")))
                        {
                            await next(env);
                            return;
                        }

                        var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Count\": " + _readModel.Count + "}"));
                        context.Response.Body = stream;
                        context.Response.ContentType = "application/json";
                    };
                }
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