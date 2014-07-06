namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.CommandHandling;
    using Cedar.Hosting;
    using Nancy.TinyIoc;
    using Owin;
    using Owin.EmbeddedHost;

    public class CedarHost : IDisposable
    {
        private readonly CedarBootstrapper _bootstrapper;
        private readonly OwinEmbeddedHost _owinEmbeddedHost;

        public CedarHost([NotNull] CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

            _bootstrapper = bootstrapper;

            var commandsAndHandlers = bootstrapper.CommandHandlerTypes.Select(commandHandlerType => new
                {
                    CommandHandlerType = commandHandlerType,
                    CommandType = commandHandlerType.GetInterfaceGenericTypeArguments(typeof(ICommandHandler<>))[0]
                }).ToArray();

            var container = new TinyIoCContainer();
            container.Register<ISystemClock, SystemClock>().AsSingleton();
            bootstrapper.ConfigureApplicationContainer(container);
            
            MethodInfo registerCommandHandlerMethod = typeof(TinyIoCExtensions)
                .GetMethod("RegisterCommandHandler", BindingFlags.Public | BindingFlags.Static);
            foreach (var c in commandsAndHandlers)
            {
                registerCommandHandlerMethod
                    .MakeGenericMethod(c.CommandType, c.CommandHandlerType)
                    .Invoke(this, new object[] { container });
            }

            _owinEmbeddedHost = OwinEmbeddedHost.Create(app => 
                app.Map("/commands", commandsApp =>
                    commandsApp.Use(CommandHandlerMiddleware.HandleCommands(
                        bootstrapper.VendorName,
                        commandsAndHandlers.Select(c => c.CommandType),
                        new TinyIoCCommandHandlerResolver(container),
                        bootstrapper.ExceptionToModelConverter))));
        }

        /// <summary>
        /// Gets the owin application function.
        /// </summary>
        /// <value>
        /// The owin application function.
        /// </value>
        public Func<IDictionary<string, object>, Task> OwinAppFunc
        {
            get { return _owinEmbeddedHost.Invoke; }
        }

        public void Dispose()
        {
            _owinEmbeddedHost.Dispose();
        }
    }
}