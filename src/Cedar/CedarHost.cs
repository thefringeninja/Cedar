namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.Hosting;
    using Owin;
    using Owin.EmbeddedHost;
    using TinyIoC;

    public class CedarHost : IDisposable
    {
        private readonly OwinEmbeddedHost _owinEmbeddedHost;

        public CedarHost([NotNull] CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

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

            Type[] commandTypes = commandsAndHandlers.Select(c => c.CommandType).ToArray();

            _owinEmbeddedHost = OwinEmbeddedHost.Create(app => 
                app.Map("/commands", commandsApp =>
                    commandsApp.Use(CommandHandlerMiddleware.HandleCommands(
                        new DefaultCommandTypeFromContentTypeResolver(bootstrapper.VendorName, commandTypes),
                        new CommandDispatcher(new TinyIoCCommandHandlerResolver(container)), 
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