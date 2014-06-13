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

            Action<TinyIoCContainer> registerDependencies = container =>
            {
                container.Register<ISystemClock, SystemClock>().AsSingleton();
                container.Register<ICommandDispatcher, CommandDispatcher>();
                container.Register<ICommandHandlerResolver, TinyIoCCommandHandlerResolver>();
                container.RegisterMultiple(typeof (ICommandDeserializer), _bootstrapper.CommandDeserializers);

                var commandTypes = new List<Type>();
                MethodInfo registerCommandHandlerMethod = GetType().GetMethod("RegisterCommandHander", BindingFlags.NonPublic | BindingFlags.Static);
                foreach (Type commandHandler in _bootstrapper.CommandHandlerTypes)
                {
                    Type commandType = commandHandler.GetInterfaceGenericTypeArguments(typeof(ICommandHandler<>))[0];
                    registerCommandHandlerMethod
                        .MakeGenericMethod(commandType, commandHandler)
                        .Invoke(this, new object[] { container });
                    commandTypes.Add(commandType);
                }
                container.Register<ICommandTypeResolver>(new CommandTypeResolver(bootstrapper.VendorName, commandTypes));

                bootstrapper.ConfigureApplicationContainer(container);
            };

            _owinEmbeddedHost = OwinEmbeddedHost.Create(app =>
                app.UseNancy(opt => opt.Bootstrapper = new CedarNancyBootstrapper(registerDependencies, _bootstrapper.NancyModulesTypes)));
        }

        public Func<IDictionary<string, object>, Task> AppFunc
        {
            get { return _owinEmbeddedHost.Invoke; }
        } 

        public void Dispose()
        {
            _owinEmbeddedHost.Dispose();
        }

        private class CedarNancyBootstrapper : DefaultNancyBootstrapper
        {
            private readonly Action<TinyIoCContainer> _registerDependencies;
            private readonly IEnumerable<Type> _nancyModulesTypes;

            public CedarNancyBootstrapper(Action<TinyIoCContainer> registerDependencies, IEnumerable<Type> nancyModulesTypes)
            {
                _registerDependencies = registerDependencies;
                _nancyModulesTypes = nancyModulesTypes;
            }

            protected override void ConfigureApplicationContainer(TinyIoCContainer container)
            {
                _registerDependencies(container);
            }

            protected override IEnumerable<ModuleRegistration> Modules
            {
                get
                {
                    IEnumerable<ModuleRegistration> moduleRegistrations = _nancyModulesTypes.Select(t => new ModuleRegistration(t));
                    return new[] {new ModuleRegistration(typeof(CommandModule))}.Concat(moduleRegistrations);
                }
            }
        }
    }
}