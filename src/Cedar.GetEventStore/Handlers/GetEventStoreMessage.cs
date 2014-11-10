namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using EventStore.ClientAPI;

    public static class GetEventStoreMessage
    {
        public static DomainEventMessage<T> Create<T>(
            T domainEvent,
            IDictionary<string, object> headers,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll) where T : class
        {
            return new DomainEventMessage<T>(
                resolvedEvent.Event.EventStreamId, 
                domainEvent, 
                resolvedEvent.Event.EventNumber, 
                headers, isSubscribedToAll
                    ? resolvedEvent.OriginalPosition.ToCheckpointToken()
                    : resolvedEvent.OriginalEventNumber.ToString());
        }
    }
}