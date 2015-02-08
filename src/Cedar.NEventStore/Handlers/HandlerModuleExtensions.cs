namespace Cedar.NEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using global::NEventStore;
    using EventMessage = global::NEventStore.EventMessage;

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
                await (Task)genericMethod.Invoke(null, new object[]
                {
                    handlerModules, commit, version++, eventMessage, cancellationToken
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
                await (Task)genericMethod.Invoke(null, new object[]
                {
                    new [] { handlerModule }, commit, version++, eventMessage, cancellationToken
                });
            }
        }

        [UsedImplicitly]
        private static Task DispatchDomainEvent<TDomainEvent>(
            IEnumerable<IHandlerResolver> handlerModules,
            ICommit commit,
            int version,
            EventMessage eventMessage,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = NEventStoreMessage.Create<TDomainEvent>(eventMessage, commit, version);

            return handlerModules.Dispatch(message, cancellationToken);
        }
    }
}