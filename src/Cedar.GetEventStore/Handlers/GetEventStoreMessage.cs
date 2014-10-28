namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using EventStore.ClientAPI;

    public static class GetEventStoreMessage
    {
        public static DomainEventMessage<T> Create<T>(T domainEvent, IDictionary<string, object> headers,
            ResolvedEvent resolvedEvent) where T : class
        {
            return new DomainEventMessage<T>(domainEvent, headers, resolvedEvent.Event.EventStreamId,
                resolvedEvent.Event.EventNumber, resolvedEvent.OriginalPosition.ToCheckpointToken());
        }
    }
}