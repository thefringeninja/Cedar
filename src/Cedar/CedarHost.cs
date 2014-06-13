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
        private readonly TinyIoCContainer _container;
        private readonly OwinEmbeddedHost _owinEmbeddedHost;

        public CedarHost(CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

            _bootstrapper = bootstrapper;
            _container = new TinyIoCContainer();

            _container.Register<ISystemClock, SystemClock>().AsSingleton();
            _container.Register<ICommandDispatcher, CommandDispatcher>();
            _container.Register<ICommandHandlerResolver, TinyIoCCommandHandlerResolver>();
            _container.RegisterMultiple(typeof(ICommandDeserializer), _bootstrapper.CommandDeserializers);

            var commandTypes = new List<Type>();
            MethodInfo registerCommandHandlerMethod = GetType().GetMethod("RegisterCommandHander", BindingFlags.NonPublic | BindingFlags.Static);
            foreach (Type commandHandler in _bootstrapper.CommandHandlerTypes)
            {
                Type commandType = commandHandler.GetInterfaceGenericTypeArguments(typeof(ICommandHandler<>))[0];
                registerCommandHandlerMethod
                    .MakeGenericMethod(commandType, commandHandler)
                    .Invoke(this, new object[] { _container });
                commandTypes.Add(commandType);
            }
            _container.Register<ICommandTypeResolver>(new CommandTypeResolver(bootstrapper.VendorName, commandTypes));

            _owinEmbeddedHost = OwinEmbeddedHost.Create(app =>
                app.UseNancy(opt => opt.Bootstrapper = new CedarNancyBootstrapper(_container, _bootstrapper.NancyModulesTypes)));
        }

        public Func<IDictionary<string, object>, Task> AppFunc
        {
            get { return _owinEmbeddedHost.Invoke; }
        } 

        public void Dispose()
        {
            _owinEmbeddedHost.Dispose();
            _container.Dispose();
        }

        public static void RegisterCommandHander<TCommand, TCommandHandler>(TinyIoCContainer container)
            where TCommand : class
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            container.Register<ICommandHandler<TCommand>, TCommandHandler>();
            container.Register<TCommandHandler>();  
        }

        private class CedarNancyBootstrapper : DefaultNancyBootstrapper
        {
            private readonly TinyIoCContainer _container;
            private readonly IEnumerable<Type> _nancyModulesTypes;

            public CedarNancyBootstrapper(TinyIoCContainer container, IEnumerable<Type> nancyModulesTypes)
            {
                _container = container;
                _nancyModulesTypes = nancyModulesTypes;
            }

            protected override IEnumerable<ModuleRegistration> Modules
            {
                get
                {
                    IEnumerable<ModuleRegistration> moduleRegistrations = _nancyModulesTypes.Select(t => new ModuleRegistration(t));
                    return new[] {new ModuleRegistration(typeof(CommandModule))}.Concat(moduleRegistrations);
                }
            }

            protected override TinyIoCContainer GetApplicationContainer()
            {
                return _container;
            }
        }
    }
}