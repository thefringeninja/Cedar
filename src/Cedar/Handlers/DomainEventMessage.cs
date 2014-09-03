namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using NEventStore;

    public class DomainEventMessage<T>
    {
        private readonly ICommit _commit;
        private readonly int _version;
        private readonly IReadOnlyDictionary<string, object> _eventHeaders;
        private readonly T _domainEvent;

        public DomainEventMessage(
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            T domainEvent)
        {
            _commit = commit;
            _version = version;
            _eventHeaders = eventHeaders;
            _domainEvent = domainEvent;
        }

        public ICommit Commit
        {
            get { return _commit; }
        }

        public int Version
        {
            get { return _version; }
        }

        public IReadOnlyDictionary<string, object> EventHeaders
        {
            get { return _eventHeaders; }
        }

        public T DomainEvent
        {
            get { return _domainEvent; }
        }
    }
}