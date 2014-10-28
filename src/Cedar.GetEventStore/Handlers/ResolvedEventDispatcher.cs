namespace Cedar.Handlers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.PlatformServices;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Internal;
    using Cedar.Serialization.Client;
    using EventStore.ClientAPI;


    public class ResolvedEventDispatcher : IDisposable
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly ISerializer _serializer;
        private readonly ICheckpointRepository _checkpoints;
        private readonly Func<ResolvedEvent, ISerializer, CancellationToken, Task> _dispatchResolvedEvent;
        private readonly Action _onCaughtUp;
        private readonly TransientExceptionRetryPolicy _retryPolicy;
        private EventStoreAllCatchUpSubscription _subscription;
        private readonly Subject<ResolvedEvent> _projectedEvents;
        private readonly InterlockedBoolean _isStarted;
        private readonly InterlockedBoolean _isDisposed;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();
        private readonly SimpleQueue _queue;

        static ResolvedEventDispatcher()
        {
            PlatformEnlightenmentProvider.Current = new CurrentPlatformEnlightenmentProvider();
        }

        public ResolvedEventDispatcher(
            IEventStoreConnection eventStore,
            ISerializer serializer,
            ICheckpointRepository checkpoints,
            Func<ResolvedEvent, ISerializer, CancellationToken, Task> dispatchResolvedEvent,
            Action onCaughtUp,
            TransientExceptionRetryPolicy retryPolicy = null)
        {
            _isStarted = new InterlockedBoolean();
            _isDisposed = new InterlockedBoolean();

            _eventStore = eventStore;
            _serializer = serializer;
            _checkpoints = checkpoints;
            _dispatchResolvedEvent = dispatchResolvedEvent;
            _onCaughtUp = onCaughtUp;
            _projectedEvents = new Subject<ResolvedEvent>();
            _retryPolicy = retryPolicy ?? TransientExceptionRetryPolicy.None();

            _queue = new SimpleQueue(async (resolvedEvent, token) =>
            {
                try
                {
                    await _retryPolicy.Retry(() => _dispatchResolvedEvent(resolvedEvent, _serializer, _disposed.Token), token);
                }
                catch(Exception ex)
                {
                    _projectedEvents.OnError(ex);
                    throw;
                }

                _projectedEvents.OnNext(resolvedEvent);

            }, _disposed.Token);
        }

        public ResolvedEventDispatcher(
            IEventStoreConnection eventStore,
            ISerializer serializer,
            ICheckpointRepository checkpoints,
            [NotNull] IEnumerable<IHandlerResolver> handlerModules,
            Action onCaughtUp,
            TransientExceptionRetryPolicy retryPolicy = null)
            : this(eventStore, serializer, checkpoints, handlerModules.DispatchResolvedEvent, onCaughtUp, retryPolicy)
        {}

        public ResolvedEventDispatcher(
            IEventStoreConnection eventStore,
            ISerializer serializer,
            ICheckpointRepository checkpoints,
            [NotNull] IHandlerResolver handlerModule,
            Action onCaughtUp,
            TransientExceptionRetryPolicy retryPolicy = null)
            : this(eventStore, serializer, checkpoints, new[] {handlerModule}, onCaughtUp, retryPolicy)
        {}

        public IObservable<ResolvedEvent> ProjectedEvents
        {
            get { return _projectedEvents; }
        }

        public async Task Start()
        {
            if (_isStarted.EnsureCalledOnce())
            {
                return;
            }

            await RecoverSubscription();
        }

        public void Dispose()
        {
            if (_isDisposed.EnsureCalledOnce())
            {
                return;
            }

            _disposed.Cancel();
            _projectedEvents.Dispose();
            _subscription.Stop();
        }

        private async Task RecoverSubscription()
        {
            var checkpoint = await _checkpoints.Get();

            _subscription = _eventStore.SubscribeToAllFrom(checkpoint.ParsePosition(),
                false,
                EventAppeared,
                _ => _onCaughtUp(),
                SubscriptionDropped);
        }

        private void SubscriptionDropped(EventStoreCatchUpSubscription _, SubscriptionDropReason reason, Exception ex)
        {
            if(reason == SubscriptionDropReason.UserInitiated)
            {
                return;
            }

            RecoverSubscription().Wait(TimeSpan.FromSeconds(2));
        }

        private void EventAppeared(EventStoreCatchUpSubscription _, ResolvedEvent resolvedEvent)
        {
            if(resolvedEvent.OriginalEvent.EventType.StartsWith("$")
               || resolvedEvent.OriginalStreamId.StartsWith("$"))
            {
                return;
            }

            _queue.Enqueue(resolvedEvent);
        }

        class SimpleQueue
        {
            private readonly Func<ResolvedEvent, CancellationToken, Task> _onResolvedEvent;
            private readonly CancellationToken _token;
            private readonly ConcurrentQueue<ResolvedEvent> _events;
            private readonly InterlockedBoolean _isPushing;

            public SimpleQueue(Func<ResolvedEvent, CancellationToken, Task> onResolvedEvent, CancellationToken token)
            {
                _onResolvedEvent = onResolvedEvent;
                _token = token;
                _events = new ConcurrentQueue<ResolvedEvent>();
                _isPushing = new InterlockedBoolean();
            }

            public void Enqueue(ResolvedEvent resolvedEvent)
            {
                _events.Enqueue(resolvedEvent);
                Push();
            }

            private void Push()
            {
                if(_isPushing.CompareExchange(true, false))
                {
                    return;
                }
                Task.Run(async () =>
                {
                    ResolvedEvent resolvedEvent;
                    while(_events.TryDequeue(out resolvedEvent))
                    {
                        try
                        {
                            await _onResolvedEvent(resolvedEvent, _token);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    _isPushing.Set(false);
                }, _token);
            }
        }
    }
}
