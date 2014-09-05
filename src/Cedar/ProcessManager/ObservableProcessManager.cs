namespace Cedar.ProcessManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using NEventStore;

    public abstract class ObservableProcessManager<TInitiatedBy> : IProcessManager
    {
        private readonly IList<IDisposable> _subscriptions;
        private readonly ICheckpoint _lastCheckpoint;
        private ICheckpoint _currentCheckpoint;

        protected ObservableProcessManager(
            Guid correlationId,
            Func<object, Task> dispatch,
            IObservable<ICommit> commits,
            Func<string, ICheckpoint> convertToCheckpoint,
            ICheckpointRepository checkpoints,
            ICheckpoint lastCheckpoint)
        {
            _subscriptions = new List<IDisposable>();

            _convertToCheckpoint = convertToCheckpoint;
            _checkpoints = checkpoints;

            var commitsByCorrelationId =
                from commit in commits
                where commit.Headers.ContainsKey("CorrelationId")
                      && commit.Headers["CorrelationId"].Equals(correlationId)
                select commit;

            _inbox =
                from commit in
                    commitsByCorrelationId.Do(commit => _currentCheckpoint = convertToCheckpoint(commit.CheckpointToken))
                from eventMessage in commit.Events
                select eventMessage.Body;

            var sender = new Subject<object>();
            
            _outbox = sender;
            
            _subscriptions.Add(sender.Subscribe(async outgoing =>
            {
                if (_currentCheckpoint.CompareTo(_lastCheckpoint) <= 0)
                {
                    return;
                }

                await dispatch(outgoing);
                await checkpoints.Put(_currentCheckpoint.ToString());
            }));

            _lastCheckpoint = lastCheckpoint;
        }

        private readonly IObserver<object> _outbox;
        
        private readonly Func<string, ICheckpoint> _convertToCheckpoint;
        private readonly ICheckpointRepository _checkpoints;
        private readonly IObservable<object> _inbox;
        private bool _disposed;

        protected abstract Task Run();

        public async Task RunProcess()
        {
            _currentCheckpoint = _convertToCheckpoint(await _checkpoints.Get());

            await Run();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            _subscriptions.ForEach(subscription => subscription.Dispose());
            _subscriptions.Clear();
        }

        private void Send(IObservable<object> outgoing)
        {
            _subscriptions.Add(outgoing.Subscribe(_outbox));
        }

        protected void Do<T>(IObservable<T> when, Func<T, IEnumerable<object>> selector)
        {
            Send(when.Select(selector));
        }

        protected void Do<T>(IObservable<T> when, Func<T, object> selector)
        {
            Do(when, e => Enumerable.Repeat(selector(e), 1));
        }

        protected IObservable<T> When<T>()
        {
            return _inbox.OfType<T>();
        }

        protected Task CompleteWhen<T>(IObservable<T> when)
        {
            return when.FirstAsync().ToTask();
        }
    }
}