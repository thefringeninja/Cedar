namespace Cedar.Handlers.TempImportFromNES
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.PlatformServices;
    using System.Threading.Tasks;
    using Cedar.Internal;
    using NEventStore;
    using NEventStore.Persistence;

    // Imported from NES 6, will be removed when NES6 is out

    public class EventStoreClient : IEventStoreClient, IDisposable
    {
        public const int DefaultPollingInterval = 5000;
        private readonly IPersistStreams _persistStreams;
        private readonly int _pageSize;

        private readonly ConcurrentDictionary<Guid, Subscriber> _subscribers =
            new ConcurrentDictionary<Guid, Subscriber>();

        private readonly InterlockedBoolean _isRetrieving = new InterlockedBoolean();
        private readonly IDisposable _retrieveTimer;
        private readonly LruCache<string, ICommit[]> _commitsCache = new LruCache<string, ICommit[]>(100);

        static EventStoreClient()
        {
            PlatformEnlightenmentProvider.Current = new CurrentPlatformEnlightenmentProvider();
        }

        public EventStoreClient(
            IPersistStreams persistStreams,
            int pollingIntervalMilliseconds = DefaultPollingInterval,
            int pageSize = 256)
        {
            _persistStreams = persistStreams;
            _pageSize = pageSize;
            _retrieveTimer = Observable
                .Interval(TimeSpan.FromMilliseconds(pollingIntervalMilliseconds))
                .Subscribe(_ => RetrieveNow());
        }

        public void Dispose()
        {
            _retrieveTimer.Dispose();
        }

        public IDisposable Subscribe(string fromCheckpoint, Func<ICommit, Task> onCommit)
        {
            var subscriberId = Guid.NewGuid();
            var subscriber = new Subscriber(
                fromCheckpoint,
                onCommit,
                _pageSize,
                RetrieveNow,
                () =>
                {
                    Subscriber _;
                    _subscribers.TryRemove(subscriberId, out _);
                });
            _subscribers.TryAdd(subscriberId, subscriber);
            RetrieveNow();
            return subscriber;
        }

        public void RetrieveNow()
        {
            if (_isRetrieving.CompareExchange(true, false))
            {
                return;
            }

            Task.Run(() =>
            {
                foreach (var subscriber in _subscribers.Values.ToArray())
                {
                    if (subscriber.QueueLength >= _pageSize)
                    {
                        continue;
                    }

                    string key = subscriber.Checkpoint ?? "<null>";
                    ICommit[] commits;
                    if (!_commitsCache.TryGet(key, out commits))
                    {
                        commits = _persistStreams //Will be async
                            .GetFrom(subscriber.Checkpoint)
                            .Take(_pageSize)
                            .ToArray();
                        if (commits.Length == _pageSize)
                        {
                            // Only store full page prevents
                            _commitsCache.Set(key, commits);
                        }
                    }

                    foreach (var commit in commits)
                    {
                        subscriber.Enqueue(commit);
                    }
                }
                _isRetrieving.Set(false);
            });
        }

        private class Subscriber : IDisposable
        {
            private string _checkpoint;
            private readonly Func<ICommit, Task> _onCommit;
            private readonly int _threshold;
            private readonly Action _onThreashold;
            private readonly Action _onDispose;
            private readonly ConcurrentQueue<ICommit> _commits = new ConcurrentQueue<ICommit>();
            private readonly InterlockedBoolean _isPushing = new InterlockedBoolean();

            public Subscriber(
                string checkpoint,
                Func<ICommit, Task> onCommit,
                int threshold,
                Action onThreashold,
                Action onDispose)
            {
                _checkpoint = checkpoint;
                _onCommit = onCommit;
                _threshold = threshold;
                _onThreashold = onThreashold;
                _onDispose = onDispose;
            }

            public string Checkpoint
            {
                get { return _checkpoint; }
            }

            public void Enqueue(ICommit commit)
            {
                _commits.Enqueue(commit);
                _checkpoint = commit.CheckpointToken;
                Push();
            }

            public int QueueLength
            {
                get { return _commits.Count; }
            }

            public void Dispose()
            {
                _onDispose();
            }

            private void Push()
            {
                if (_isPushing.CompareExchange(true, false))
                {
                    return;
                }
                Task.Run(async () =>
                {
                    ICommit commit;
                    while (_commits.TryDequeue(out commit))
                    {
                        try
                        {
                            await _onCommit(commit);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        if (_commits.Count < _threshold)
                        {
                            _onThreashold();
                        }
                    }
                    _isPushing.Set(false);
                });
            }
        }
    }
}