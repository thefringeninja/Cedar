
namespace Cedar.Testing
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    internal static class OwinExtensions
    {
        public static AppFunc Terminate(this MidFunc midFunc)
        {
            return midFunc(env =>
            {
                // By convention, an owin pipeline is terminated with a Not Found. 
                var context = new OwinContext(env);
                context.Response.StatusCode = 404;
                context.Response.ReasonPhrase = "Not Found";
                return Task.FromResult(0);
            });
        }


        public static HttpClient CreateClient(this AppFunc appFunc)
        {
            var handler = new OwinHttpMessageHandler(appFunc)
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true, // need this else the auth cookie won't be carried to subsequent requests,
                AllowAutoRedirect = true
            };
            return new HttpClient(handler, true)
            {
                BaseAddress = new Uri("https://localhost")
            };
        }

    }
}
