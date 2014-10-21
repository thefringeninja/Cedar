namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;

    public static class HandlerModuleExtensions
    {
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