namespace Cedar.Queries.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Queries.Client;
    using Cedar.TypeResolution;
    using Microsoft.Owin;

    public class QueryHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;
        private readonly MessageExecutionSettings _messageExecutionSettings;

        public QueryHandlingFixture()
        {
            const string vendor = "vendor";

            var handlerModule = new TestHandlerModule();
           
            var queryTypeFromContentTypeResolver = new DefaultRequestTypeResolver(
                vendor,
                new[]
                {
                    typeof (TestQuery),
                    typeof (TestQueryWithoutHandler),
                    typeof (TestQueryWhoseHandlerThrows),
                    typeof(TestQueryResponse)
                });
            var options = new DefaultHandlerSettings(handlerModule, queryTypeFromContentTypeResolver);
            _midFunc = QueryHandlingMiddleware.HandleQueries(options);
            _messageExecutionSettings = new QueryExecutionSettings(vendor);
        }

        public MessageExecutionSettings MessageExecutionSettings
        {
            get { return _messageExecutionSettings; }
        }

        public HttpClient CreateHttpClient()
        {
            return CreateHttpClient(env =>
            {
                var context = new OwinContext(env);
                context.Response.StatusCode = 404;
                context.Response.ReasonPhrase = "Not Found";
                return Task.FromResult(0);
            });
        }

        public HttpClient CreateHttpClient(Func<IDictionary<string, object>, Task> next)
        {
            var appFunc = _midFunc(next);
            return new HttpClient(new OwinHttpMessageHandler(appFunc))
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}