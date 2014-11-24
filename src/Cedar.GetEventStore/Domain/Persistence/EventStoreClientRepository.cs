namespace Cedar.GetEventStore.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using Cedar.Domain.Persistence;
    using Cedar.GetEventStore.Serialization;
    using Cedar.Handlers;
    using Cedar.Serialization;
    using EventStore.ClientAPI;

    public class EventStoreClientRepository<T>
        where T : class, IAggregate
    {
        private const int PageSize = 512;
        private readonly IEventStoreConnection _connection;
        private readonly ISerializer _serializer;
        private readonly IAggregateFactory _aggregateFactory;

        public EventStoreClientRepository(IEventStoreConnection connection, ISerializer serializer, IAggregateFactory aggregateFactory = null)
        {
            _connection = connection;
            _serializer = serializer;
            _aggregateFactory = aggregateFactory ?? new DefaultAggregateFactory();
        }

        public async Task<T> GetById(string streamId, int maxVersion = Int32.MaxValue, string bucketId = null)
        {
            var streamName = streamId.FormatStreamNameWithBucket(bucketId);

            var slice = await _connection.ReadStreamEventsForwardAsync(streamName, StreamPosition.Start, PageSize, false);
            
            if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound)
            {
                return null;
            }
            
            var aggregate = _aggregateFactory.Build(typeof(T), streamId);

            ApplySlice(maxVersion, slice, aggregate);

            while (false == slice.IsEndOfStream)
            {
                slice = await _connection.ReadStreamEventsForwardAsync(streamName, slice.NextEventNumber, PageSize, false);

                ApplySlice(maxVersion, slice, aggregate);
            }

            return (T)aggregate;
        }

        private void ApplySlice(int maxVersion, StreamEventsSlice slice, IAggregate aggregate)
        {
            int version = aggregate.Version;
            var eventsToApply = from @event in slice.Events
                where ++version <= maxVersion
                select _serializer.DeserializeEventData(@event);
         
            eventsToApply.ForEach(aggregate.ApplyEvent);

            aggregate.ClearUncommittedEvents();
        }

        public async Task Save(T aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders = null, string bucketId = null)
        {
            var changes = aggregate.GetUncommittedEvents().OfType<object>().ToList();

            var expectedVersion = aggregate.Version - changes.Count;

            if(false == changes.Any())
            {
                throw new ArgumentOutOfRangeException("aggregate.GetUncommittedEvents", "No changes found.");
            }
            if(changes.Count > PageSize)
            {
                throw new ArgumentOutOfRangeException("aggregate.GetUncommittedEvents", "Too many changes found. You are probably doing something wrong.");
            }

            int currentEventVersion = expectedVersion;

            var streamName = aggregate.Id.FormatStreamNameWithBucket(bucketId);

            updateHeaders = updateHeaders ?? (_ => { });

            var eventData = changes.Select(@event => _serializer.SerializeEventData(
                @event, 
                streamName, 
                currentEventVersion++,
                headers =>
                {
                    updateHeaders(headers);

                    headers[DomainEventMessageHeaders.CommitId] = commitId;
                }));

            var result = await _connection.AppendToStreamAsync(streamName, expectedVersion - 1, eventData);

            if(result.LogPosition == Position.End)
            {
                throw new Exception();
            }
        }
    }
}
