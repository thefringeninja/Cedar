namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using NEventStore;

    public static class NEventStoreMessage
    {
        public static DomainEventMessage<T> Create<T>(EventMessage eventMessage, ICommit commit, int version) where T : class
        {
            var @event = (T)eventMessage.Body;

            var headers = new Dictionary<string, object>(commit.Headers).Merge(@eventMessage.Headers, new Dictionary<string, object>
            {
                {DomainEventMessageHeaders.StreamId, commit.StreamId},
                {DomainEventMessageHeaders.Type, typeof(T)},
                {NEventStoreMessageHeaders.Commit, commit}
            });

            return new DomainEventMessage<T>(commit.StreamId.FormatStreamNameWithoutBucket(), @event, version, headers, commit.CheckpointToken);
        }

        public static ICommit Commit<T>(this DomainEventMessage<T> message) where T : class
        {
            object commit;

            message.Headers.TryGetValue(NEventStoreMessageHeaders.Commit, out commit);

            return commit as ICommit;
        }
    }
}