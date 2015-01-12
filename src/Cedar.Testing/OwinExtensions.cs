namespace Cedar.Testing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cedar.Testing.LibOwin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    internal static class OwinExtensions
    {
        internal static AppFunc Terminate(this MidFunc midFunc)
        {
            return midFunc(Terminate);
        }

        private static Task Terminate(IDictionary<string, object> env)
        {
            // By convention, an owin pipeline is terminated with a Not Found. 
            var context = new OwinContext(env);
            context.Response.StatusCode = 404;
            context.Response.ReasonPhrase = "Not Found";

            return Task.FromResult(0);
        }
    }
}