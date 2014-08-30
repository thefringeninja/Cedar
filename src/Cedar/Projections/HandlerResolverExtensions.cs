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

    public static class HandlerResolverExtensions
    {
        public static async Task<int> DispatchCommit(
            [NotNull] this IHandlerResolver handlerResolver,
            [NotNull] ICommit commit,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");
            Guard.EnsureNotNull(commit, "commit");

            var methodInfo = typeof(HandlerResolverExtensions).GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            int handlers = 0;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                handlers += await (Task<int>)genericMethod.Invoke(null, new []
                {
                    handlerResolver, commit, version, eventMessage.Headers, eventMessage.Body, cancellationToken
                });
                version++;
            }
            return handlers;
        }

        private static Task DispatchDomainEvent<TDomainEvent>(
            IHandlerResolver handlerResolver,
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            TDomainEvent domainEvent,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = new DomainEventMessage<TDomainEvent>(commit, version, eventHeaders, domainEvent);
            return handlerResolver.Dispatch(message, cancellationToken);
        }
    }
}