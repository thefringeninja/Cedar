namespace Cedar.Projections
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Projections.Storage;
    using NEventStore;
    using TinyIoC;

    public class ProjectionHost : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly TinyIoCContainer _container = new TinyIoCContainer();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private readonly CommitDispatcher _commitDispatcher;

        public ProjectionHost(
            IEventStoreClient eventStoreClient,
            ICheckpointRepository checkpointRepository,
            IProjectionResolver projectionResolver)
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
            _commitDispatcher = new CommitDispatcher(projectionResolver);
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
            var commitStream = _eventStoreClient.ObserveFrom(checkpointToken);
            var subscription = commitStream
                .Subscribe(commit => Task.Run(async () =>
                {
                    //TODO Handle transient errors and consider cancellation
                    try
                    {
                        await _commitDispatcher.DispatchCommit(commit);
                        await _checkpointRepository.Put(commit.CheckpointToken);
                        _commitsProjectedStream.OnNext(commit);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }).Wait());
            commitStream.Start();
            _compositeDisposable.Add(commitStream);
            _compositeDisposable.Add(subscription);
        }

        public IObservable<ICommit> CommitsProjectedStreamSteam
        {
            get { return _commitsProjectedStream; }
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