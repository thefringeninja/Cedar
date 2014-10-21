namespace Cedar.ProcessManagers.Persistence
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Domain.Persistence;
    using Cedar.Handlers;
    using NEventStore;
    using NEventStore.Persistence;

    public class NEventStoreProcessManagerRepository
        : IProcessManagerRepository
    {
        private readonly IStoreEvents _eventStore;
        private readonly IProcessManagerFactory _processManagerFactory;
        private readonly ConcurrentDictionary<Tuple<string, string>, int> _streamHeads;

        public NEventStoreProcessManagerRepository(IStoreEvents eventStore,
            IProcessManagerFactory processManagerFactory = null)
        {
            _eventStore = eventStore;
            _processManagerFactory = processManagerFactory ?? new DefaultProcessManagerFactory();
            _streamHeads = new ConcurrentDictionary<Tuple<string, string>, int>();
        }

        public Task<TProcess> GetById<TProcess>(string bucketId, string id, int versionToLoad, CancellationToken token) where TProcess : IProcessManager
        {
            List<ICommit> commits = _eventStore.Advanced.GetFrom(bucketId, id, 0, versionToLoad).ToList();
            IProcessManager processManager = GetProcess<TProcess>(id);
            int streamHead = ApplyEventsToAggregate(commits, processManager);
            _streamHeads.AddOrUpdate(Tuple.Create(bucketId, id), streamHead, (key, _) => streamHead);
            //TODO NES 6 async support
            return Task.FromResult((TProcess) processManager);
        }

        public async Task Save<TProcess>(string bucketId, TProcess process,
            Action<IDictionary<string, object>> updateHeaders, CancellationToken token)
            where TProcess : IProcessManager
        {
            Dictionary<string, object> headers = PrepareHeaders(updateHeaders);
            while (true)
            {
                int streamHead;

                if (false == _streamHeads.TryGetValue(Tuple.Create(bucketId, process.Id), out streamHead))
                {
                    streamHead = 1;
                }

                var commitAttempt = new CommitAttempt(bucketId, process.Id, streamHead, Guid.NewGuid(), process.Version, DateTime.UtcNow, headers,
                    process.GetUncommittedEvents().Select(@event => new EventMessage
                    {
                        Body = @event
                    }));

                try
                {
                    //TODO NES 6 async support
                    //await stream.CommitChanges(commitId).NotOnCapturedContext()
                    _eventStore.Advanced.Commit(commitAttempt);
                    process.ClearUncommittedEvents();
                    process.ClearUndispatchedCommands();
                    return;
                }
                catch (DuplicateCommitException)
                {
                    return;
                }
                catch (ConcurrencyException e)
                {
                    throw new ConflictingCommandException(e.Message, e);
                }
                catch (StorageException e)
                {
                    throw new PersistenceException(e.Message, e);
                }
            }
        }

        private IProcessManager GetProcess<TAggregate>(string streamId)
        {
            return _processManagerFactory.Build(typeof(TAggregate), streamId);
        }

        private static int ApplyEventsToAggregate(IEnumerable<ICommit> commits, IProcessManager process)
        {
            int lastStreamRevision = 1;

            foreach(ICommit commit in commits)
            {
                lastStreamRevision = commit.StreamRevision;

                foreach(var domainEventMessage in commit.Events.Select(
                    eventMessage => CreateDomainEventMessage(eventMessage, commit, commit.StreamRevision)))
                {
                    process.ApplyEvent(domainEventMessage);
                }
            }

            process.ClearUncommittedEvents();

            return lastStreamRevision;
        }

        private static dynamic CreateDomainEventMessage(EventMessage eventMessage, ICommit commit, int version)
        {
            Guard.EnsureNotNull(eventMessage, "eventMessage");
            Guard.EnsureNotNull(eventMessage.Body, "eventMessage.Body");
            Guard.EnsureNotNull(commit, "commit");

            var messageType = typeof(NEventStoreMessage<>).MakeGenericType(eventMessage.Body.GetType());

            return Activator.CreateInstance(messageType, commit, version, eventMessage.Headers, eventMessage.Body);
        }

        private static Dictionary<string, object> PrepareHeaders(
            Action<IDictionary<string, object>> updateHeaders)
        {
            var headers = new Dictionary<string, object>();

            if(updateHeaders != null)
            {
                updateHeaders(headers);
            }

            return headers;
        }
    }
}