namespace Cedar.Handlers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Dispatcher : IDispatcher
    {
        private readonly IHandlerResolver _handlerResolver;

        public Dispatcher(IHandlerResolver handlerResolver)
        {
            if (handlerResolver == null)
            {
                throw new ArgumentNullException("handlerResolver");
            }

            _handlerResolver = handlerResolver;
        }

        public async Task<int> Message<TMessage>(TMessage message, CancellationToken cancellationToken)
            where TMessage : class
        {
            IHandler<TMessage>[] handlers = _handlerResolver.ResolveAll<TMessage>().ToArray();
            foreach (var projector in handlers)
            {
                await projector.Handle(message, cancellationToken);
            }
            return handlers.Length;
        }
    }
}