namespace Cedar
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.CommandHandling;
    using Cedar.Hosting;
    using TinyIoC;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    public class CedarHost : IDisposable
    {
        private readonly AppFunc _owinAppFunc;
        private readonly TinyIoCContainer _container;

        public CedarHost([NotNull] CedarBootstrapper bootstrapper)
        {
            Guard.EnsureNotNull(bootstrapper, "bootstrapper");

            _container = new TinyIoCContainer();
            bootstrapper.ConfigureApplicationContainer(_container);

            MidFunc commandHandlerMidFunc = CommandHandlerMiddleware.HandleCommands(
                new DefaultCommandTypeFromContentTypeResolver(bootstrapper.VendorName, bootstrapper.GetCommandTypes()),
                new TinyIoCCommandHandlerResolver(_container),
                _container.Resolve<IExceptionToModelConverter>());

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
        {
            _container.Dispose();
        }
    }
}