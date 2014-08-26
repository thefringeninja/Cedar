namespace Cedar.Projections
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using NEventStore;
    using NEventStore.Client;
    using TinyIoC;

    public class ProjectionHost : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly IDispatcher _dispatcher;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly TinyIoCContainer _container = new TinyIoCContainer();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private IObserveCommits _commitStream;

        public ProjectionHost(
            IEventStoreClient eventStoreClient,
            ICheckpointRepository checkpointRepository,
            IDispatcher dispatcher)
        {
            if (eventStoreClient == null)
            {
                throw new ArgumentNullException("eventStoreClient");
            }
            if (checkpointRepository == null)
            {
                throw new ArgumentNullException("checkpointRepository");
            }
            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _dispatcher = dispatcher;
            _compositeDisposable.Add(_commitsProjectedStream);
            _compositeDisposable.Add(_container);
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
                .Subscribe(commit => Task.Run(async () =>
                {
                    //TODO Handle transient errors and consider cancellation
                    try
                    {
                        await _dispatcher.DispatchCommit(commit, CancellationToken.None);
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