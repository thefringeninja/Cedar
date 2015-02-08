namespace Cedar.NEventStore.Handlers
{
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
           [NotNull] this IHandlerResolver handlerResolver,
           [NotNull] ICommit commit,
           CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerResolver, "handlerModules");
            Guard.EnsureNotNull(commit, "commit");

            var methodInfo = typeof(HandlerModuleExtensions)
                .GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var genericMethod = methodInfo.MakeGenericMethod(eventMessage.Body.GetType());
                await (Task)genericMethod.Invoke(null, new object[]
                {
                    handlerResolver, commit, version++, eventMessage, cancellationToken
                });
            }
        }

        [UsedImplicitly]
        private static Task DispatchDomainEvent<TDomainEvent>(
            IHandlerResolver handlerModule,
            ICommit commit,
            int version,
            EventMessage eventMessage,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = NEventStoreMessage.Create<TDomainEvent>(eventMessage, commit, version);

            return handlerModule.Dispatch(message, cancellationToken);
        }
    }
}