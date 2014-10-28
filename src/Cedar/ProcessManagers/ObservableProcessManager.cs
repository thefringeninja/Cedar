namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public abstract class ObservableProcessManager : IProcessManager, IDisposable
    {
        private readonly ISubject<object> _inbox;
        private readonly string _id;
        private readonly string _correlationId;
        private bool _subscribed;
        private int _version;
        private readonly ISubject<object> _commands;
        private readonly ISubject<object> _events;
        private readonly IList<IDisposable> _subscriptions; 
        protected ObservableProcessManager(
            string id, string correlationId)
        {
            _id = id;
            _correlationId = correlationId;

            _inbox = new ReplaySubject<object>();
            _commands = new Subject<object>();
            _events = new Subject<object>();
            _subscriptions = new List<IDisposable>();

            When(OnAnyMessage(), _ => _version++);
        }

        public string Id
        {
            get { return _id; }
        }

        public int Version
        {
            get { return _version; }
        }

        public IObserver<object> Inbox
        {
            get { return _inbox; }
        }

        public IObservable<object> Commands
        {
            get { return _commands; }
        }

        public IObservable<object> Events
        {
            get { return _events; }
        }

        protected void When<TEvent>(IObservable<TEvent> @on, Func<TEvent, IEnumerable<object>> @do)
        {
            Send(@on.SelectMany(@do));
        }

        protected void When<TEvent>(IObservable<TEvent> @on, Func<TEvent, object> @do)
        {
            When(@on, e => Enumerable.Repeat(@do(e), 1));
        }

        protected IObservable<TMessage> On<TMessage>()
        {
            return _inbox.OfType<TMessage>();
        }
        protected IObservable<dynamic> OnAnyMessage()
        {
            return _inbox.OfType<object>();
        }

        protected void CompleteWhen<TEvent>(IObservable<TEvent> @on)
        {
            _subscriptions.Add(@on.Select(_ => new ProcessCompleted
            {
                ProcessId = _id,
                CorrelationId = _correlationId
            }).Subscribe(_events));
        }

        private void Send(IObservable<object> commands)
        {
            _subscriptions.Add(commands.Subscribe(_commands));
        }

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
        }
    }
}