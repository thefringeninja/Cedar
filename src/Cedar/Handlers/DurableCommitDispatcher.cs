namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Logging;
    using NEventStore;
    using NEventStore.Client;

    /// <summary>
    /// Subscribes to a stream of Commits from NEventStore and dispatches to a handler. It tracks the commit stream checkpoint
    /// such that on restart it will continue where it left off. If handlers throw a <see cref="TransientException"/>, the dispatcher
    /// will be retried according to the supplied <see cref="TransientExceptionRetryPolicy"/>.
    /// </summary>
    public sealed class DurableCommitDispatcher : IDisposable
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IEventStoreClient _eventStoreClient;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly Func<ICommit, CancellationToken, Task> _dispatchCommit;
        private readonly Subject<ICommit> _commitsProjectedStream = new Subject<ICommit>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _isStarted;
        private int _isDisposed;
        private IObserveCommits _commitStream;
        private readonly TransientExceptionRetryPolicy _retryPolicy;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="DurableCommitDispatcher"/> class.
        /// </summary>
        /// <param name="eventStoreClient">An event store client.</param>
        /// <param name="checkpointRepository">A checkpoint repository. Each instane of a <see cref="DurableCommitDispatcher"/>
        /// should have their own instance of a <see cref="ICheckpointRepository"/>.</param>
        /// <param name="handlerModule">A handler module to dispatch the commit to.</param>
        /// <param name="retryPolicy">A retry policy when a <see cref="TransientException"/> occurs.
        /// If none specified TransientException.None is used.</param>
        /// <exception cref="System.ArgumentNullException">
        /// eventStoreClient
        /// or
        /// checkpointRepository
        /// or
        /// dispatchCommit
        /// </exception>
        public DurableCommitDispatcher(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] IHandlerResolver handlerModule,
            TransientExceptionRetryPolicy retryPolicy = null) :
            this(eventStoreClient, checkpointRepository, new[] { handlerModule }, retryPolicy)
        {
            Guard.EnsureNotNull(handlerModule, "handlerModule");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DurableCommitDispatcher"/> class.
        /// </summary>
        /// <param name="eventStoreClient">An event store client.</param>
        /// <param name="checkpointRepository">A checkpoint repository. Each instane of a <see cref="DurableCommitDispatcher"/>
        /// should have their own instance of a <see cref="ICheckpointRepository"/>.</param>
        /// <param name="handlerModules">A collection of handler modules to dispatch the commit to.</param>
        /// <param name="retryPolicy">A retry policy when a <see cref="TransientException"/> occurs.
        /// If none specified TransientException.None is used.</param>
        /// <exception cref="System.ArgumentNullException">
        /// eventStoreClient
        /// or
        /// checkpointRepository
        /// or
        /// dispatchCommit
        /// </exception>
        public DurableCommitDispatcher(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            TransientExceptionRetryPolicy retryPolicy = null):
            this(eventStoreClient, checkpointRepository, handlerModules.DispatchCommit, retryPolicy )
        {
            Guard.EnsureNotNull(handlerModules, "handlerModule");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DurableCommitDispatcher"/> class.
        /// </summary>
        /// <param name="eventStoreClient">An event store client.</param>
        /// <param name="checkpointRepository">A checkpoint repository. Each instane of a <see cref="DurableCommitDispatcher"/>
        /// should have their own instance of a <see cref="ICheckpointRepository"/>.</param>
        /// <param name="dispatchCommit">A handler to dispatch the commit to.</param>
        /// <param name="retryPolicy">A retry policy when a <see cref="TransientException"/> occurs.
        /// If none specified TransientException.None is used.</param>
        /// <exception cref="System.ArgumentNullException">
        /// eventStoreClient
        /// or
        /// checkpointRepository
        /// or
        /// dispatchCommit
        /// </exception>
        public DurableCommitDispatcher(
            [NotNull] IEventStoreClient eventStoreClient,
            [NotNull] ICheckpointRepository checkpointRepository,
            [NotNull] Func<ICommit, CancellationToken, Task> dispatchCommit,
            TransientExceptionRetryPolicy retryPolicy = null)
        {
            Guard.EnsureNotNull(eventStoreClient, "eventStoreClient");
            Guard.EnsureNotNull(checkpointRepository, "checkpointRepository");
            Guard.EnsureNotNull(dispatchCommit, "dispatchCommit");

            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _dispatchCommit = dispatchCommit;
            _retryPolicy = retryPolicy ?? TransientExceptionRetryPolicy.None();
            _compositeDisposable.Add(_commitsProjectedStream);
        }

        /// <summary>
        /// Starts observing commits and dispatching them..
        /// </summary>
        /// <returns></returns>
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
                    try
                    {
                        await _retryPolicy.Retry(async () =>
                        {
                            await _dispatchCommit(commit, CancellationToken.None);
                        }, _disposed.Token);
                        await _retryPolicy.Retry(async () =>
                        {
                            await _checkpointRepository.Put(commit.CheckpointToken);
                        }, _disposed.Token);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException(
                            Messages.ExceptionHasOccuredWhenDispatchingACommit.FormatWith(commit.ToString()),
                            ex);
                        _commitsProjectedStream.OnError(ex);
                        throw;
                    }
                    _commitsProjectedStream.OnNext(commit);
                }).Wait());
            _commitStream.Start();
            _compositeDisposable.Add(_commitStream);
            _compositeDisposable.Add(subscription);
        }

        /// <summary>
        /// Gets an observable of <see cref="ICommit"/>. When subscribers observe an exception it indicates that
        /// the <see cref="TransientExceptionRetryPolicy"/> has failed. This would probably
        /// indicate a serious issue where you may wish to consider terminiating your application.
        /// </summary>
        public IObservable<ICommit> CommitsProjectedStream
        {
            get { return _commitsProjectedStream; }
        }

        /// <summary>
        /// Polls the EventStore for new commits. Invoking this from a NEventStore pipeline will help to reduce latency when
        /// dispatching commits to handlers.
        /// </summary>
        public void PollNow()
        {
            if (_commitStream != null)
            {
                _commitStream.PollNow();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
            {
                return;
            }
            _disposed.Cancel();
            _compositeDisposable.Dispose();
        }
    }
}