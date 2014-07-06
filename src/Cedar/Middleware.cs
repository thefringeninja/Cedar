namespace Cedar
{
    using Microsoft.Owin;
    using Microsoft.Owin.Mapping;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    internal static class Middleware
    {
        internal static MidFunc MapPath(string pathMatch, AppFunc branch)
        {
            return next =>
            {
                var options = new MapOptions
                {
                    Branch = branch,
                    PathMatch = new PathString(pathMatch)
                };
                var middleware = new MapMiddleware(next, options);
                return middleware.Invoke;
            };
        }
    }
}