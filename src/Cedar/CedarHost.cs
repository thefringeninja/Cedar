namespace Cedar
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Dispatching;
    using Cedar.Hosting;
    using TinyIoC;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public class CedarHost : IDisposable
    {
        private readonly AppFunc _owinAppFunc;

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

            MidFunc commandHandlerMidFunc = CommandHandlerMiddleware.HandleCommands(
                new DefaultCommandTypeFromContentTypeResolver(bootstrapper.VendorName, commandTypes),
                new CommandDispatcher(new TinyIoCCommandHandlerResolver(container)),
                bootstrapper.ExceptionToModelConverter);

            _owinAppFunc = Middleware.MapPath("/commands", commandHandlerMidFunc(_ => Task.FromResult(0)))(_ => Task.FromResult(0));
        }

        /// <summary>
        /// Gets the owin application function.
        /// </summary>
        /// <value>
        /// The owin application function.
        /// </value>
        public AppFunc OwinAppFunc
        {
            get { return _owinAppFunc; }
        }

        public void Dispose()
        {}
    }
}