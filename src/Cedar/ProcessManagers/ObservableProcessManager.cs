namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Cedar.Handlers;

    public abstract class ObservableProcessManager : IProcessManager
    {
        private readonly IList<object> _outbox;
        private readonly ISubject<object> _inbox;
        private readonly IList<object> _events;
        private readonly string _id;

        private int _version;

        protected ObservableProcessManager(
            string id)
        {
            _id = id;
            
            _inbox = new ReplaySubject<object>();
            _outbox = new List<object>();
            _events = new List<object>();

            Subscribe();
        }

        public string Id
        {
            get { return _id; }
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

        public void ApplyEvent<TEvent>(DomainEventMessage<TEvent> @event)
        {
            _inbox.OnNext(@event);
            _version++;
        }
        protected void When<TEvent>(IObservable<DomainEventMessage<TEvent>> @on, Func<DomainEventMessage<TEvent>, IEnumerable<object>> @do)
        {
            Send(@on.SelectMany(@do));
        }

        protected void When<TEvent>(IObservable<DomainEventMessage<TEvent>> @on, Func<DomainEventMessage<TEvent>, object> @do)
        {
            When(@on, e => Enumerable.Repeat(@do(e), 1));
        }

        protected void When<TEvent>(IObservable<TEvent> @on, Func<TEvent, IEnumerable<object>> @do)
        {
            Send(@on.SelectMany(@do));
        }

        protected void When<TEvent>(IObservable<TEvent> @on, Func<TEvent, object> @do)
        {
            When(@on, e => Enumerable.Repeat(@do(e), 1));
        }

        protected IObservable<DomainEventMessage<TEvent>> OnMessage<TEvent>()
        {
            return _inbox.OfType<DomainEventMessage<TEvent>>();
        }

        protected IObservable<TEvent> On<TEvent>()
        {
            return _inbox.OfType<DomainEventMessage<TEvent>>()
                .Select(message => message.DomainEvent);
        }

        protected void CompleteWhen<TEvent>(IObservable<TEvent> @on)
        {
            @on.Select(_ => new ProcessCompleted
            {
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