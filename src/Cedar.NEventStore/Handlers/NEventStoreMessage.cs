namespace Cedar.NEventStore.Handlers
{
    using System.Collections.Generic;
    using Cedar.Handlers;
    using global::NEventStore;
    using EventMessage = global::NEventStore.EventMessage;

    public static class NEventStoreMessage
    {
        public static EventMessage<T> Create<T>(EventMessage eventMessage, ICommit commit, int version) where T : class
        {
            var @event = (T)eventMessage.Body;

            var headers = new Dictionary<string, object>(commit.Headers).Merge(@eventMessage.Headers, new Dictionary<string, object>
            {
                {EventMessageHeaders.StreamId, commit.StreamId},
                {EventMessageHeaders.Type, typeof(T)},
                {NEventStoreMessageHeaders.Commit, commit}
            });

            return new EventMessage<T>(commit.StreamId, @event, version, headers, commit.CheckpointToken);
        }

        public static ICommit Commit<T>(this EventMessage<T> message) where T : class
        {
            object commit;

            message.Headers.TryGetValue(NEventStoreMessageHeaders.Commit, out commit);

            return commit as ICommit;
        }
    }
}