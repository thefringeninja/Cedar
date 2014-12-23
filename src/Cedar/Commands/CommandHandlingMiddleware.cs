namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Dispatcher;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using CuttingEdge.Conditions;
    using Microsoft.Owin.Builder;
    using Owin;
    using TinyIoC;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, 
        System.Threading.Tasks.Task
    >;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >
    >;

    public class CommandHandlingSettings
    {
        public CommandHandlingSettings([NotNull] IHandlerResolver handlerResolver)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
        }
    }

    public static class CommandHandlingMiddleware
    {
        public static MidFunc HandleCommands(HandlerSettings settings, string commandPath = "/commands")
        {
            Condition.Requires(settings, "settings").IsNotNull();
            Condition.Requires(commandPath, "commandPath").IsNotNullOrWhiteSpace();

            return next =>
            {
                var webApiConfig = ConfigureWebApi(settings);
                var appBuilder = new AppBuilder();
                appBuilder
                    .Map(commandPath,
                        commandsApp =>
                        {
                            commandsApp.UseWebApi(webApiConfig);
                        })
                    .Run(ctx => next(ctx.Environment));
                return appBuilder.Build();
            };
        }

        private static HttpConfiguration ConfigureWebApi(HandlerSettings settings)
        {
            var container = new TinyIoCContainer();
            container.Register(settings);

            var config = new HttpConfiguration
            {
                DependencyResolver = new TinyIoCDependencyResolver(container)
            };
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new CommandHandlingHttpControllerTypeResolver());
            config.MapHttpAttributeRoutes();

            return config;
        }

        private class TinyIoCDependencyResolver : IDependencyResolver
        {
            private readonly TinyIoCContainer _container;

            public TinyIoCDependencyResolver(TinyIoCContainer container)
            {
                _container = container;
            }

            public void Dispose()
            { }

            public object GetService(Type serviceType)
            {
                try
                {
                    return _container.Resolve(serviceType);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                try
                {
                    return _container.ResolveAll(serviceType, true);
                }
                catch
                {
                    return Enumerable.Empty<object>();
                }
            }

            public IDependencyScope BeginScope()
            {
                return this;
            }
        }

        private class CommandHandlingHttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            // We want to be very explicit which controllers we want to use.
            // Also we want our controllers internal.

            public ICollection<Type> GetControllerTypes(IAssembliesResolver _)
            {
                return new[] { typeof(CommandController) };
            }
        }
    }
}