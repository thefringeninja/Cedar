namespace Cedar.GetEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.GetEventStore.Serialization;
    using Cedar.Handlers;
    using Cedar.Serialization;
    using EventStore.ClientAPI;

    public static class HandlerModuleExtensions
    {
        private static readonly MethodInfo DispatchDomainEventMethod;

        static HandlerModuleExtensions()
        {
            DispatchDomainEventMethod = typeof(HandlerModuleExtensions)
                .GetMethod("DispatchDomainEvent", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static Task DispatchResolvedEvent(
            [NotNull] this IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] ISerializer serializer,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(resolvedEvent, "resolvedEvent");
            Guard.EnsureNotNull(serializer, "serializer");

            IDictionary<string, object> headers;
            var @event = serializer.DeserializeEventData(resolvedEvent, out headers);

            return (Task) DispatchDomainEventMethod.MakeGenericMethod(@event.GetType()).Invoke(null, new[]
            {
                handlerModules, serializer, @event, headers, resolvedEvent, isSubscribedToAll, cancellationToken
            });
        }

        public static Task DispatchResolvedEvent(
            [NotNull] this IHandlerResolver handlerModule,
            [NotNull] ISerializer serializer,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll,
            CancellationToken cancellationToken)
        {
            Guard.EnsureNotNull(handlerModule, "handlerModule");
            Guard.EnsureNotNull(resolvedEvent, "resolvedEvent");
            Guard.EnsureNotNull(serializer, "serializer");

            IDictionary<string, object> headers;
            var @event = serializer.DeserializeEventData(resolvedEvent, out headers);

            return (Task) DispatchDomainEventMethod.MakeGenericMethod(@event.GetType()).Invoke(null, new[]
            {
                new[] {handlerModule}, serializer, @event, headers, resolvedEvent, isSubscribedToAll, cancellationToken
            });
        }

        [UsedImplicitly]
        private static Task DispatchDomainEvent<TDomainEvent>(
            IEnumerable<IHandlerResolver> handlerModules,
            [NotNull] ISerializer serializer,
            TDomainEvent domainEvent,
            IDictionary<string, object> headers,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll,
            CancellationToken cancellationToken)
            where TDomainEvent : class
        {
            var message = GetEventStoreMessage.Create(domainEvent, headers, resolvedEvent, isSubscribedToAll);

            return handlerModules.Dispatch(message, cancellationToken);
        }
    }
}
