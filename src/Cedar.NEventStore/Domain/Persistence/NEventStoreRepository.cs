namespace Cedar.NEventStore.Domain.Persistence
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using Cedar.Domain.Persistence;
    using global::NEventStore;
    using global::NEventStore.Persistence;

    public class NEventStoreRepository : IRepository
    {
        private const string AggregateTypeHeader = "AggregateType";
        private readonly IAggregateFactory _aggregateFactory;
        private readonly IStoreEvents _eventStore;
        private readonly ConcurrentDictionary<Tuple<string, string>, int> _streamHeads;

        public NEventStoreRepository(IStoreEvents eventStore)
            : this(eventStore, (IAggregateFactory) new DefaultAggregateFactory())
        {}

        public NEventStoreRepository(IStoreEvents eventStore, IAggregateFactory aggregateFactory)
        {
            _eventStore = eventStore;
            _aggregateFactory = aggregateFactory;
            _streamHeads = new ConcurrentDictionary<Tuple<string, string>, int>();
        }

        public Task<TAggregate> GetById<TAggregate>(string bucketId, string id, int versionToLoad, CancellationToken cancellationToken)
            where TAggregate : class, IAggregate
        {
            var commits = _eventStore.Advanced.GetFrom(bucketId, id, 0, versionToLoad).ToList();
            IAggregate aggregate = GetAggregate<TAggregate>(id);
            var streamHead = ApplyEventsToAggregate(commits, aggregate);
            _streamHeads.AddOrUpdate(Tuple.Create(bucketId, id), streamHead, (key, _) => streamHead);
            //TODO NES 6 async support
            return Task.FromResult(aggregate as TAggregate);
        }

        public async Task Save(
            string bucketId,
            IAggregate aggregate,
            Guid commitId,
            Action<IDictionary<string, object>> updateHeaders,
            CancellationToken cancellationToken)
        {
            Dictionary<string, object> headers = PrepareHeaders(aggregate, updateHeaders);
            while (true)
            {
                int streamHead;
                
                if(false == _streamHeads.TryGetValue(Tuple.Create(bucketId, aggregate.Id), out streamHead))
                {
                    streamHead = 1;
                }

                var commitAttempt = new CommitAttempt(bucketId, aggregate.Id, streamHead, commitId, aggregate.Version, DateTime.UtcNow, headers,
                    aggregate.GetUncommittedEvents().OfType<object>().Select(@event => new EventMessage
                    {
                        Body = @event
                    }));

                try
                {
                    //TODO NES 6 async support
                    //await stream.CommitChanges(commitId).NotOnCapturedContext()
                    _eventStore.Advanced.Commit(commitAttempt);
                    aggregate.ClearUncommittedEvents();
                    return;
                }
                catch(DuplicateCommitException)
                {
                    return;
                }
                catch(ConcurrencyException e)
                {
                    throw new ConflictingCommandException(e.Message, e);
                }
                catch(StorageException e)
                {
                    throw new PersistenceException(e.Message, e);
                }
            }
        }

        private IAggregate GetAggregate<TAggregate>(string streamId)
        {
            return _aggregateFactory.Build(typeof(TAggregate), streamId);
        }

        private static int ApplyEventsToAggregate(IEnumerable<ICommit> commits, IAggregate aggregate)
        {
            int lastStreamRevision = 1;

            foreach (var commit in commits)
            {
                lastStreamRevision = commit.StreamRevision;
                foreach(var eventMessage in commit.Events)
                {
                    aggregate.ApplyEvent(eventMessage.Body);
                }
            }

            aggregate.ClearUncommittedEvents();

            return lastStreamRevision;
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
    }
}