using Cedar.CommandHandling;
using Nancy.TinyIoc;

namespace Cedar.Hosting
{
    public class TinyIoCCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly TinyIoCContainer _container;

        public TinyIoCCommandHandlerResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public ICommandHandler<T> Resolve<T>() where T : class
        {
            return _container.Resolve<ICommandHandler<T>>();
        }
    }
}