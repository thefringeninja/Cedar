namespace Cedar.NEventStore.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;
    using Cedar.Internal;
    using Cedar.Logging;
    using global::NEventStore;

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
        private readonly Subject<ICommit> _projectedCommits = new Subject<ICommit>();
        private readonly InterlockedBoolean _isStarted = new InterlockedBoolean();
        private int _isDisposed;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();
        private IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="DurableCommitDispatcher"/> class.
        /// </summary>
        /// <param name="eventStoreClient">An event store client.</param>
        /// <param name="checkpointRepository">A checkpoint repository. Each instane of a <see cref="DurableCommitDispatcher"/>
        /// should have their own instance of a <see cref="ICheckpointRepository"/>.</param>
        /// <param name="handlerModule">A handler module to dispatch the commit to.</param>
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
            [NotNull] IHandlerResolver handlerModule) :
            this(eventStoreClient, checkpointRepository, new[] { handlerModule })
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
            [NotNull] IEnumerable<IHandlerResolver> handlerModules):
            this(eventStoreClient, checkpointRepository, handlerModules.DispatchCommit)
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
            [NotNull] Func<ICommit, CancellationToken, Task> dispatchCommit)
        {
            Guard.EnsureNotNull(eventStoreClient, "eventStoreClient");
            Guard.EnsureNotNull(checkpointRepository, "checkpointRepository");
            Guard.EnsureNotNull(dispatchCommit, "dispatchCommit");

            _eventStoreClient = eventStoreClient;
            _checkpointRepository = checkpointRepository;
            _dispatchCommit = dispatchCommit;
        }

        /// <summary>
        /// Starts observing commits and dispatching them..
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (_isStarted.EnsureCalledOnce())
            {
                return;
            }

            string checkpointToken = await _checkpointRepository.Get();

            _subscription = _eventStoreClient.Subscribe(checkpointToken, async commit =>
            {
                try
                {
                    await _dispatchCommit(commit, _disposed.Token);
                    await _checkpointRepository.Put(commit.CheckpointToken);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(
                        ExtensionMethods.FormatWith(Messages.ExceptionHasOccuredWhenDispatchingACommit, new[] { commit.ToString() }),
                        ex);
                    _projectedCommits.OnError(ex);
                    throw;
                }
                _projectedCommits.OnNext(commit);
            });
        }

        /// <summary>
        /// Gets an observable of <see cref="ICommit"/> as they have been projectd. When subscribers
        /// observe an exception it indicates that  the <see cref="TransientExceptionRetryPolicy"/>
        /// has failed. This would probably indicate a serious issue where you may wish to consider
        /// terminiating your application.
        /// </summary>
        public IObservable<ICommit> ProjectedCommits
        {
            get { return _projectedCommits; }
        }

        /// <summary>
        /// Polls the EventStore for new commits. Invoking this from a NEventStore pipeline will help to reduce latency when
        /// dispatching commits to handlers.
        /// </summary>
        public void PollNow()
        {
            if (_eventStoreClient != null)
            {
                _eventStoreClient.RetrieveNow();
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
            _projectedCommits.Dispose();
            _subscription.Dispose();
        }
    }
}
