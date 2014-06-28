namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.CommandHandling.Modules;
    using Cedar.CommandHandling.Serialization;
    using Cedar.Hosting;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;
    using Owin;
    using Owin.EmbeddedHost;

    public class CedarHost : IDisposable
    {
        private readonly CedarBootstrapper _bootstrapper;
        private readonly OwinEmbeddedHost _owinEmbeddedHost;

        public CedarHost(CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

            _bootstrapper = bootstrapper;

            MethodInfo registerCommandHandlerMethod = typeof(TinyIoCExtensions).GetMethod("RegisterCommandHandler", BindingFlags.Public | BindingFlags.Static);
            
            Action<TinyIoCContainer> registerDependencies = container =>
            {
                container.Register<ISystemClock, SystemClock>().AsSingleton();
                container.Register<ICommandDispatcher, CommandDispatcher>();
                container.Register<ICommandHandlerResolver, TinyIoCCommandHandlerResolver>();
                container.RegisterMultiple(typeof (ICommandDeserializer), _bootstrapper.CommandDeserializers);

                var commands = _bootstrapper.CommandHandlerTypes.Select(commandHandlerType => new
                {
                    CommandHandlerType = commandHandlerType,
                    CommandType = commandHandlerType.GetInterfaceGenericTypeArguments(typeof (ICommandHandler<>))[0]
                }).ToArray();
                foreach (var command in commands)
                {
                    registerCommandHandlerMethod
                        .MakeGenericMethod(command.CommandType, command.CommandHandlerType)
                        .Invoke(this, new object[] { container });
                }
                container.Register<ICommandTypeFromHttpContentType>(
                    new CommandTypeFromContentTypeResolver(bootstrapper.VendorName, commands.Select(c => c.CommandType)));

                bootstrapper.ConfigureApplicationContainer(container);
            };

            _owinEmbeddedHost = OwinEmbeddedHost.Create(app =>
                app.Map("/commands", commandsApp => 
                    commandsApp.UseNancy(opt => opt.Bootstrapper = new CommandHandlingNancyBootstrapper(registerDependencies))));
        }

        public Func<IDictionary<string, object>, Task> AppFunc
        {
            get { return _owinEmbeddedHost.Invoke; }
        } 

        public void Dispose()
        {
            _owinEmbeddedHost.Dispose();
        }

        private class CommandHandlingNancyBootstrapper : DefaultNancyBootstrapper
        {
            private readonly Action<TinyIoCContainer> _registerDependencies;

            public CommandHandlingNancyBootstrapper(Action<TinyIoCContainer> registerDependencies)
            {
                _registerDependencies = registerDependencies;
            }

            protected override void ConfigureApplicationContainer(TinyIoCContainer container)
            {
                _registerDependencies(container);
            }

            protected override IEnumerable<ModuleRegistration> Modules
            {
                get
                {
                    return new[] {new ModuleRegistration(typeof(CommandModule))};
                }
            }
        }
    }
}