namespace Cedar.Projections
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;
    using NEventStore;

    public static class HandlerModuleExtensions
    {
        public static async Task DispatchCommit(
            [NotNull] this IEnumerable<HandlerModule> handlerModules,
            [NotNull] ICommit commit,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(commit, "commit");

            var methodInfo = typeof(HandlerModuleExtensions).GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                await (Task)genericMethod.Invoke(null, new []
                {
                    handlerModules, commit, version, eventMessage.Headers, eventMessage.Body, cancellationToken
                });
            }
        }

        private static Task DispatchDomainEvent<TDomainEvent>(
            IEnumerable<HandlerModule> handlerModules,
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