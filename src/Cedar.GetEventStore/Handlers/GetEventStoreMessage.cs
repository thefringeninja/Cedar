namespace Cedar.GetEventStore.Handlers
{
    using System.Collections.Generic;
    using Cedar.Handlers;
    using EventStore.ClientAPI;

    public static class GetEventStoreMessage
    {
        public static EventMessage<T> Create<T>(
            T domainEvent,
            IDictionary<string, object> headers,
            ResolvedEvent resolvedEvent,
            bool isSubscribedToAll) where T : class
        {
            return new EventMessage<T>(
                resolvedEvent.Event.EventStreamId, 
                domainEvent, 
                resolvedEvent.Event.EventNumber, 
                headers, isSubscribedToAll
                    ? resolvedEvent.OriginalPosition.ToCheckpointToken()
                    : resolvedEvent.OriginalEventNumber.ToString());
        }
    }
}