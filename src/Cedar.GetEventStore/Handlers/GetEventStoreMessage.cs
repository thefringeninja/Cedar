namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using EventStore.ClientAPI;

    public class GetEventStoreMessage<T>
    {
        private readonly T _domainEvent;
        private readonly string _streamId;
        private readonly Position? _checkpoint;
        private readonly IDictionary<string, object> _headers;

        public GetEventStoreMessage(T domainEvent, IDictionary<string, object> headers, string streamId, Position? checkpoint)
        {
            _domainEvent = domainEvent;
            _streamId = streamId;
            _checkpoint = checkpoint;
            _headers = headers;
        }

        public T DomainEvent
        {
            get { return _domainEvent; }
        }

        public string StreamId
        {
            get { return _streamId; }
        }

        public Position? Checkpoint
        {
            get { return _checkpoint; }
        }

        public IDictionary<string, object> Headers
        {
            get { return _headers; }
        }
    }
}
