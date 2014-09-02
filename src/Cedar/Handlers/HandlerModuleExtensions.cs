namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;

    public static class HandlerModuleExtensions
    {
        public static Task Dispatch<TMessage>(
            [NotNull] this HandlerModule handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            return new[] {handlerModules}.Dispatch(message, cancellationToken);
        }

        public static async Task Dispatch<TMessage>(
            [NotNull] this IEnumerable<HandlerModule> handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage: class
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(message, "message");

            var handlers = handlerModules.SelectMany(m => m.GetHandlersFor<TMessage>());
            foreach (var handler in handlers)
            {
                await handler(message, cancellationToken);
            }
        }

        public static Task DispatchSingle<TMessage>(
            [NotNull] this HandlerModule handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            return new[] { handlerModules }.DispatchSingle(message, cancellationToken);
        }
        
        public static async Task DispatchSingle<TMessage>(
            [NotNull] this IEnumerable<HandlerModule> handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(message, "message");

            var handler = handlerModules.SelectMany(m => m.GetHandlersFor<TMessage>()).SingleOrDefault();
            if (handler == null)
            {
                return;
            }
            await handler(message, cancellationToken);
        }
    }
}