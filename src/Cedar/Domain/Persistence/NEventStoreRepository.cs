namespace Cedar.Domain.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NEventStore;
    using NEventStore.Persistence;

    public class NEventStoreRepository : IRepository
    {
        private const string AggregateTypeHeader = "AggregateType";
        private readonly IConflictDetector _conflictDetector;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly IStoreEvents _eventStore;

        public NEventStoreRepository(IStoreEvents eventStore)
            : this(eventStore, new DefaultAggregateFactory(), new DefaultConflictDetector())
        {}

        public NEventStoreRepository(IStoreEvents eventStore, IAggregateFactory aggregateFactory)
            : this(eventStore, aggregateFactory, new DefaultConflictDetector())
        {}

        public NEventStoreRepository(IStoreEvents eventStore, IConflictDetector conflictDetector)
            : this(eventStore, new DefaultAggregateFactory(), conflictDetector)
        {}

        public NEventStoreRepository(IStoreEvents eventStore, IAggregateFactory aggregateFactory, IConflictDetector conflictDetector)
        {
            _eventStore = eventStore;
            _aggregateFactory = aggregateFactory;
            _conflictDetector = conflictDetector;
        }

        public virtual TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(Bucket.Default, id);
        }

        public virtual TAggregate GetById<TAggregate>(Guid id, int versionToLoad) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(Bucket.Default, id, versionToLoad);
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(bucketId, id, int.MaxValue);
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id, int versionToLoad)
            where TAggregate : class, IAggregate
        {
            IEventStream stream = _eventStore.OpenStream(bucketId, id, 0, versionToLoad);
            IAggregate aggregate = GetAggregate<TAggregate>(stream);
            ApplyEventsToAggregate(versionToLoad, stream, aggregate);
            return aggregate as TAggregate;
        }

        public virtual Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            return Save(Bucket.Default, aggregate, commitId, updateHeaders);
        }

        public async Task Save(string bucketId, IAggregate aggregate, Guid commitId,
            Action<IDictionary<string, object>> updateHeaders)
        {
            Dictionary<string, object> headers = PrepareHeaders(aggregate, updateHeaders);
            while (true)
            {
                IEventStream stream = PrepareStream(bucketId, aggregate, headers);
                int commitEventCount = stream.CommittedEvents.Count;

                try
                {
                    //TODO NES 6 async support
                    //await stream.CommitChanges(commitId).NotOnCapturedContext()
                    stream.CommitChanges(commitId);
                    aggregate.ClearUncommittedEvents();
                    return;
                }
                catch (DuplicateCommitException)
                {
                    stream.ClearChanges();
                    return;
                }
                catch (ConcurrencyException e)
                {
                    bool conflict = ThrowOnConflict(stream, commitEventCount);
                    stream.ClearChanges();

                    if (conflict)
                    {
                        throw new ConflictingCommandException(e.Message, e);
                    }
                }
                catch (StorageException e)
                {
                    throw new PersistenceException(e.Message, e);
                }
            }
        }

        private IAggregate GetAggregate<TAggregate>(IEventStream stream)
        {
            return _aggregateFactory.Build(typeof(TAggregate), stream.StreamId);
        }

        private static void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
        {
            if (versionToLoad != 0 && aggregate.Version >= versionToLoad)
            {
                return;
            }
            foreach (object @event in stream.CommittedEvents.Select(x => x.Body))
            {
                aggregate.ApplyEvent(@event);
            }
        }

        private IEventStream PrepareStream(string bucketId, IAggregate aggregate, Dictionary<string, object> headers)
        {
            IEventStream stream = _eventStore.CreateStream(bucketId, aggregate.Id);
            foreach (var item in headers)
            {
                stream.UncommittedHeaders[item.Key] = item.Value;
            }
            aggregate.GetUncommittedEvents()
                .Cast<object>()
                .Select(x => new EventMessage {Body = x})
                .ToList()
                .ForEach(stream.Add);
            return stream;
        }

        private static Dictionary<string, object> PrepareHeaders(
            IAggregate aggregate,
            Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>();

            headers[AggregateTypeHeader] = aggregate.GetType().FullName;
            if (updateHeaders != null)
            {
                updateHeaders(headers);
            }

            return headers;
        }

        private bool ThrowOnConflict(IEventStream stream, int skip)
        {
            IEnumerable<object> committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
            IEnumerable<object> uncommitted = stream.UncommittedEvents.Select(x => x.Body);
            return _conflictDetector.ConflictsWith(uncommitted, committed);
        }
    }
}