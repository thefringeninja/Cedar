namespace Cedar.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;

    public static class HandlerResolverExtensions
    {
        public static async Task Dispatch<TMessage>(
            [NotNull] this IHandlerResolver handlerResolver,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage: class
        {
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");
            Guard.EnsureNotNull(message, "message");

            var handlers = handlerResolver.ResolveAll<TMessage>();
            foreach (var handler in handlers)
            {
                await handler.Handle(message, cancellationToken);
            }
        }

        public static async Task DispatchSingle<TMessage>(
            [NotNull] this IHandlerResolver handlerResolver,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");
            Guard.EnsureNotNull(message, "message");

            var handler = handlerResolver.ResolveSingle<TMessage>();
            await handler.Handle(message, cancellationToken);
        }
    }
}