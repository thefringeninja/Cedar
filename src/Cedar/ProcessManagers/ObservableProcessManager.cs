namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public abstract class ObservableProcessManager : IProcessManager
    {
        private readonly IList<object> _outbox;
        private readonly ISubject<object> _inbox;
        private readonly IList<object> _events;
        private readonly string _id;
        private readonly Guid _correlationId;

        private int _version;

        protected ObservableProcessManager(
            string id,
            Guid correlationId)
        {
            _id = id;
            _correlationId = correlationId;
            
            _inbox = new ReplaySubject<object>();
            _outbox = new List<object>();
            _events = new List<object>();

            Subscribe();
        }

        public string Id
        {
            get { return _id; }
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public int Version
        {
            get { return _version; }
        }

        public IEnumerable<object> GetUncommittedEvents()
        {
            return _events.AsEnumerable();
        }

        public void ClearUncommittedEvents()
        {
            _events.Clear();
        }

        public IEnumerable<object> GetUndispatchedCommands()
        {
            return _outbox.AsEnumerable();
        }

        public void ClearUndispatchedCommands()
        {
            _outbox.Clear();
        }

        public void ApplyEvent(object @event)
        {
            _inbox.OnNext(@event);
            _version++;
        }

        protected void When<T>(IObservable<T> @on, Func<T, IEnumerable<object>> @do)
        {
            Send(@on.SelectMany(@do));
        }

        protected void When<T>(IObservable<T> @on, Func<T, object> @do)
        {
            When(@on, e => Enumerable.Repeat(@do(e), 1));
        }

        protected IObservable<T> On<T>()
        {
            return _inbox.OfType<T>();
        }

        protected void CompleteWhen<T>(IObservable<T> @on)
        {
            @on.Select(_ => new ProcessCompleted
            {
                CorrelationId = _correlationId,
                ProcessId = _id
            }).Subscribe(_inbox);
        }

        protected abstract void Subscribe();

        private void Send(IObservable<object> messages)
        {
            messages.Subscribe(_outbox.Add);
        }
    }
}