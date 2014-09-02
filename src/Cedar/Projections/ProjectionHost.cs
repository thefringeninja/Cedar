namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using NEventStore;
    using NEventStore.Client;

    public class ProjectionHost : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IEnumerable<HandlerModule> _handlerModules;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private IObserveCommits _commitStream;

        public ProjectionHost(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] HandlerModule handlerModule)
            : this(eventStoreClient, checkpointRepository, new[] { handlerModule })
        {}

        public ProjectionHost(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] IEnumerable<HandlerModule> handlerModules)
        {
            if (eventStoreClient == null)
            {
                throw new ArgumentNullException("eventStoreClient");
            }
            if (checkpointRepository == null)
            {
                throw new ArgumentNullException("checkpointRepository");
            }
            if (handlerModules == null)
            {
                throw new ArgumentNullException("handlerModules");
            }

            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _handlerModules = handlerModules;
            _compositeDisposable.Add(_commitsProjectedStream);
        }

        public async Task Start()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1) //Ensures Start is only called once.
            {
                return;
            }
            string checkpointToken = await _checkpointRepository.Get();
            _commitStream = _eventStoreClient.ObserveFrom(checkpointToken); //TODO replace with EventStoreClient in NES v6
            var subscription = _commitStream
                .Subscribe(commit => Task.Run(async () =>
                {
                    //TODO Handle transient errors and consider cancellation
                    try
                    {
                        await _handlerModules.DispatchCommit(commit, CancellationToken.None);
                        await _checkpointRepository.Put(commit.CheckpointToken);
                        _commitsProjectedStream.OnNext(commit);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }).Wait());
            _commitStream.Start();
            _compositeDisposable.Add(_commitStream);
            _compositeDisposable.Add(subscription);
        }

        public IObservable<ICommit> CommitsProjectedStream
        {
            get { return _commitsProjectedStream; }
        }

        public void PollNow()
        {
            if (_commitStream != null)
            {
                _commitStream.PollNow();
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            {
                return;
            }
            _compositeDisposable.Dispose();
        }
    }
}