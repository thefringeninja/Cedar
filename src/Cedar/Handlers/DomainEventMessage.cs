namespace Cedar.Handlers
{
    using System.Collections.Generic;

    public abstract class DomainEventMessage
    {
        public readonly dynamic DomainEvent;
        public readonly IDictionary<string, object> Headers;
        public readonly int Version;
        public readonly string CheckpointToken;
        public readonly string StreamId;

        protected DomainEventMessage(
            string streamId,
            object domainEvent,
            int version,
            IDictionary<string, object> headers,
            string checkpointToken)
        {
            DomainEvent = domainEvent;
            Headers = headers;
            Version = version;
            CheckpointToken = checkpointToken;
            StreamId = streamId;
        }

        public override string ToString()
        {
            return DomainEvent.ToString();
        }
    }

    public class DomainEventMessage<T> : DomainEventMessage
        where T : class
    {
        public new readonly T DomainEvent;

        public DomainEventMessage(
            string streamId,
            T domainEvent,
            int version,
            IDictionary<string, object> headers,
            string checkpointToken) : base(streamId, domainEvent, version, headers, checkpointToken)
        {
            DomainEvent = domainEvent;
        }
    }
}