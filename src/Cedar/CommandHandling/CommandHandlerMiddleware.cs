namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using Cedar.Hosting;
    using Nancy;
    using Nancy.Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public static class CommandHandlerMiddleware
    {
        public static MidFunc HandleCommands(
            [NotNull] string vendorName,
            [NotNull] IEnumerable<Type> commandTypes,
            ICommandHandlerResolver handlerResolver)
        {
            Guard.EnsureNullOrWhiteSpace(vendorName, "vendorName");
            Guard.EnsureNotNull(commandTypes, "commandTypes");
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");

            return next => env =>
            {
                var nancyMiddleware = new NancyOwinHost(next, new NancyOptions
                {
                    Bootstrapper = new CommandHandlingNancyBootstrapper(vendorName, commandTypes, handlerResolver),
                    PerformPassThrough = ctx => ctx.Response.StatusCode == HttpStatusCode.NotFound
                });
                return nancyMiddleware.Invoke(env);
            };
        }
    }
}