namespace Cedar.Projections
{
    using System;
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
        private readonly IHandlerResolver _handlerResolver;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private IObserveCommits _commitStream;

        public ProjectionHost(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] IHandlerResolver handlerResolver)
        {
            if (eventStoreClient == null)
            {
                throw new ArgumentNullException("eventStoreClient");
            }
            if (checkpointRepository == null)
            {
                throw new ArgumentNullException("checkpointRepository");
            }
            if (handlerResolver == null)
            {
                throw new ArgumentNullException("handlerResolver");
            }

            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _handlerResolver = handlerResolver;
            _compositeDisposable.Add(_commitsProjectedStream);
        }

        public async Task Start()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1) //Ensures Start is only called once.
            {
                return;
            }
            string checkpointToken = await _checkpointRepository.Get();
            _commitStream = _eventStoreClient.ObserveFrom(checkpointToken);
            var subscription = _commitStream
                .Subscribe(async commit => 
                {
                    //TODO Handle transient errors and consider cancellation
                    try
                    {
                        await _handlerResolver.DispatchCommit(commit, CancellationToken.None);
                        await _checkpointRepository.Put(commit.CheckpointToken);
                        _commitsProjectedStream.OnNext(commit);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
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