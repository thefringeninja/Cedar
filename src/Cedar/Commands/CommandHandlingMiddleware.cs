namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Dispatcher;
    using Cedar.Serialization;
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

    public static class CommandHandlingMiddleware
    {
        public static MidFunc HandleCommands(CommandHandlingSettings settings)
        {
            Condition.Requires(settings, "settings").IsNotNull();

            return next =>
            {
                var webApiConfig = ConfigureWebApi(settings);
                var appBuilder = new AppBuilder();
                appBuilder
                    .UseWebApi(webApiConfig)
                    .Run(ctx => next(ctx.Environment));
                return appBuilder.Build();
            };
        }

        private static HttpConfiguration ConfigureWebApi(CommandHandlingSettings settings)
        {
            var container = new TinyIoCContainer();
            container.Register(settings);

            var config = new HttpConfiguration
            {
                DependencyResolver = new TinyIoCDependencyResolver(container)
            };
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new CommandHandlingHttpControllerTypeResolver());
            config.Filters.Add(new HttpProblemDetailsExceptionFilterAttribute(settings.CreateProblemDetails));
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/problem+json"));
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = DefaultJsonSerializer.Settings.ContractResolver;

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
                return _container.CanResolve(serviceType) 
                    ? _container.Resolve(serviceType) 
                    : null;
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return _container.CanResolve(serviceType) 
                    ? _container.ResolveAll(serviceType, true) 
                    : Enumerable.Empty<object>();
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