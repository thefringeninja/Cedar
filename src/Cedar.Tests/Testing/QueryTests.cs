namespace Cedar.Testing
{
    using System.IO;
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
        private readonly IHandlerResolver _resolver;
        private readonly SomeModule _module;

        public QueryTests()
        {
            _module = new SomeModule();
            _resolver = new HandlerResolver(_module);
        }

        [Fact]
        public async Task a_passing_query_test_should()
        {
            var result = await Scenario.ForQuery(_resolver, _module.Middleware)
                .Given(new SomethingHappened(), new SomethingHappened())
                .When(new HttpRequestMessage(HttpMethod.Get, "/query-response"))
                .ThenShould(response => response.Headers.ContentType.MediaType == "application/json");

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_query_test_should()
        {
            var result = await Scenario.ForQuery(_resolver, _module.Middleware)
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

                For<EventMessage<SomethingHappened>>()
                    .Handle(_ => _readModel.Count++);

            }
            private class ReadModel
            {
                public int Count;
            }
            
            public MidFunc Middleware
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