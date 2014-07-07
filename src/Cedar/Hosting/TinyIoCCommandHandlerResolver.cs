namespace Cedar.Hosting
{
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.CommandHandling;
    using TinyIoC;

    public class TinyIoCCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly TinyIoCContainer _container;

        public TinyIoCCommandHandlerResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public ICommandHandler<T> Resolve<T>() where T : class
        {
            List<ICommandHandler<T>> handlers = _container.ResolveAll<ICommandHandler<T>>().ToList();

            Guard.Ensure(handlers.Count() == 1, "");

            return handlers.Single();
        }
    }
}