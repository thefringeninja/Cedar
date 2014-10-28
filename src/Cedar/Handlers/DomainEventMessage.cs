namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class DomainEventMessage<T>
        where T: class 
    {
        public readonly T DomainEvent;
        public readonly IDictionary<string, object> Headers;
        public readonly int Version;
        public readonly string CheckpointToken;
        public readonly string StreamId;

        public DomainEventMessage(T domainEvent, IDictionary<string, object> headers, string streamId, int version, string checkpointToken)
        {
            StreamId = streamId;
            DomainEvent = domainEvent;
            Headers = new ReadOnlyDictionary<string, object>(headers);
            Version = version;
            CheckpointToken = checkpointToken;
        }
    }
}