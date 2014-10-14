namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using NEventStore;

    public static class HandlerModuleExtensions
    {
        public static Task Dispatch<TMessage>(
            [NotNull] this IHandlerResolver handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            return new[] {handlerModules}.Dispatch(message, cancellationToken);
        }

        public static async Task Dispatch<TMessage>(
            [NotNull] this IEnumerable<IHandlerResolver> handlerModules,
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
            [NotNull] this IHandlerResolver handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            return new[] { handlerModules }.DispatchSingle(message, cancellationToken);
        }
        
        public static async Task DispatchSingle<TMessage>(
            [NotNull] this IEnumerable<IHandlerResolver> handlerModules,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(message, "message");

            var handler = handlerModules.SelectMany(m => m.GetHandlersFor<TMessage>()).Single();
            await handler(message, cancellationToken);
        }

        public static async Task DispatchCommit(
           [NotNull] this IEnumerable<IHandlerResolver> handlerModules,
           [NotNull] ICommit commit,
           CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(commit, "commit");

            var methodInfo = typeof(HandlerModuleExtensions)
                .GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                await (Task)genericMethod.Invoke(null, new[]
                {
                    handlerModules, commit, version, eventMessage.Headers, eventMessage.Body, cancellationToken
                });
            }
        }

        public static async Task DispatchCommit(
           [NotNull] this IHandlerResolver handlerModule,
           [NotNull] ICommit commit,
           CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModule, "handlerModule");
            Guard.EnsureNotNull(commit, "commit");

            var methodInfo = typeof(HandlerModuleExtensions)
                .GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                await (Task)genericMethod.Invoke(null, new[]
                {
                    new[] { handlerModule }, commit, version, eventMessage.Headers, eventMessage.Body, cancellationToken
                });
            }
        }

        [UsedImplicitly]
        private static Task DispatchDomainEvent<TDomainEvent>(
            IEnumerable<IHandlerResolver> handlerModules,
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            TDomainEvent domainEvent,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = new DomainEventMessage<TDomainEvent>(commit, version, eventHeaders, domainEvent);
            return handlerModules.Dispatch(message, cancellationToken);
        }
    }
}