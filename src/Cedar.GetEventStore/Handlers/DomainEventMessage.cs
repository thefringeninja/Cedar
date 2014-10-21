namespace Cedar.Handlers
{
    using System.Collections.Generic;

    public class DomainEventMessage<T>
    {
        private readonly T _domainEvent;
        private readonly string _streamId;
        private readonly int _checkpoint;
        private readonly IDictionary<string, object> _headers;

        public DomainEventMessage(T domainEvent, IDictionary<string, object> headers, string streamId, int checkpoint)
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

        public int Checkpoint
        {
            get { return _checkpoint; }
        }

        public IDictionary<string, object> Headers
        {
            get { return _headers; }
        }
    }
}
