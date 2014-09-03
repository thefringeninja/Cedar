namespace Cedar.Handlers
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;
    using NEventStore.Client;

    public class DurableCommitDispatcher : IDisposable
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly Func<ICommit, CancellationToken, Task> _dispatchCommit;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private IObserveCommits _commitStream;

        public DurableCommitDispatcher(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] Func<ICommit, CancellationToken, Task> dispatchCommit)
        {
            if (eventStoreClient == null)
            {
                throw new ArgumentNullException("eventStoreClient");
            }
            if (checkpointRepository == null)
            {
                throw new ArgumentNullException("checkpointRepository");
            }
            if (dispatchCommit == null)
            {
                throw new ArgumentNullException("dispatchCommit");
            }

            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _dispatchCommit = dispatchCommit;
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
                        await _dispatchCommit(commit, CancellationToken.None);
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