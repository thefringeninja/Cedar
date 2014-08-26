namespace Cedar.Projections
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using NEventStore;

    public static class DispatcherExtensions
    {
        public static async Task<int> DispatchCommit(
            this IDispatcher dispatcher,
            ICommit commit,
            CancellationToken cancellationToken)
        {
            var methodInfo = typeof(DispatcherExtensions).GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            int handlers = 0;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                handlers += await (Task<int>)genericMethod.Invoke(null, new []
                {
                    dispatcher, commit, version, eventMessage.Headers, eventMessage.Body, cancellationToken
                });
                version++;
            }
            return handlers;
        }

        private static Task<int> DispatchDomainEvent<TDomainEvent>(
            IDispatcher dispatcher,
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            TDomainEvent domainEvent,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = new DomainEventMessage<TDomainEvent>(commit, version, eventHeaders, domainEvent);
            return dispatcher.Dispatch(message, cancellationToken);
        }
    }
}