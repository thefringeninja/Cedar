namespace Cedar.Testing
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing.PlainText;
    using Microsoft.Owin;
    using Xunit;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
            >
        >;

    public class MiddlewareTests
    {
        [Fact]
        public async Task a_passing_middleware_test_should()
        {
            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Created)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location))
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);
            
            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_middleware_test_should()
        {
            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Accepted)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location))
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);

            Assert.False(result.Passed);
        }

        private static MidFunc Middleware
        {
            get
            {
                var resources = new ConcurrentDictionary<string, byte[]>();
                
                return next => async env =>
                {
                    var context = new OwinContext(env);

                    var id = context.Request.Path.Value;

                    byte[] resource;
                    
                    if(context.Request.Method == "PUT")
                    {
                        context.Response.StatusCode = 201;
                        context.Response.ReasonPhrase = "Created";
                        context.Response.Headers["Location"] = id;

                        using(var stream = new MemoryStream())
                        {
                            await context.Request.Body.CopyToAsync(stream);
                            resource = stream.ToArray();
                            resources.AddOrUpdate(id, resource, (_, previous) => resource);
                        }
                        return;
                    }

                    if(false == resources.TryGetValue(id, out resource))
                    {
                        return;
                    }

                    if(context.Request.Method == "GET")
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ReasonPhrase = "OK";

                        await context.Response.WriteAsync(resource);
                        return;
                    }

                    context.Response.StatusCode = 405;
                    context.Response.ReasonPhrase = "Method Not Allowed";
                };
            }
        }
    }
}