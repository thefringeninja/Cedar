namespace Cedar.ProcessManagers
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.ProcessManagers.Messages;
    using Cedar.Serialization;
    using Cedar.Serialization.Client;
    using EventStore.ClientAPI;

    public class EventStoreClientProcessManagerCheckpointRepository : IProcessManagerCheckpointRepository<CompareablePosition>
    {
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;

        public EventStoreClientProcessManagerCheckpointRepository(IEventStoreConnection connection, ISerializer serializer)
        {
            Guard.EnsureNotNull(connection, "connection");
            _connection = connection;
            _serializer = serializer;
        }

        public Task SaveCheckpointToken(IProcessManager processManager, string checkpointToken)
        {
            return AppendToStream(processManager.Id, new CheckpointReached
            {
                CheckpointToken = checkpointToken,
                CorrelationId = processManager.CorrelationId,
                ProcessId = processManager.Id
            });
        }

        public Task MarkProcessCompleted(ProcessCompleted message)
        {
            return AppendToStream(message.ProcessId, message);
        }

        public async Task<CompareablePosition> GetCheckpoint(string processId)
        {
            int streamPosition = StreamPosition.End;
            var streamName = GetStreamName(processId);
            do
            {
                var slice = await _connection.ReadStreamEventsBackwardAsync(streamName, streamPosition, 1, false);

                if(slice.Status != SliceReadStatus.Success || slice.Events.Length == 0)
                {
                    return new CompareablePosition();
                }

                var resolvedEvent = slice.Events[0];

                var checkpointReached = _serializer.DeserializeEventData(resolvedEvent) as CheckpointReached;

                if(checkpointReached != null)
                {
                    return new CompareablePosition(checkpointReached.CheckpointToken.ParsePosition());
                }

                streamPosition = resolvedEvent.Event.EventNumber - 1;
            } while(streamPosition >= 0);

            return new CompareablePosition();
        }

        private Task AppendToStream(string processId, object @event)
        {
            return _connection.AppendToStreamAsync(GetStreamName(processId),
                ExpectedVersion.Any,
                _serializer.SerializeEventData(@event, processId, 0));
        }

        private static string GetStreamName(string processId)
        {
            return "checkpoints-" + processId;
        }
    }
}