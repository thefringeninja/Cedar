namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Cedar.ProcessManagers.Messages;

    public abstract class ObservableProcessManager : IProcessManager
    {
        private readonly ISubject<object> _inbox;
        private readonly string _id;
        private readonly string _correlationId;
        private int _version;
        private readonly List<object> _commands;
        private readonly ISubject<object> _events;
        private readonly IList<IDisposable> _subscriptions; 
        protected ObservableProcessManager(
            string id, string correlationId)
        {
            _id = id;
            _correlationId = correlationId;

            _inbox = new ReplaySubject<object>();
            _commands = new List<object>();
            _events = new Subject<object>();
            _subscriptions = new List<IDisposable>();

            Subscribe(OnAnyMessage(), _ => _version++);
        }

        public string Id
        {
            get { return _id; }
        }

        public string CorrelationId
        {
            get { return _correlationId; }
        }
        public int Version
        {
            get { return _version; }
        }

        public IObserver<object> Inbox
        {
            get { return _inbox; }
        }

        public IEnumerable<object> Commands
        {
            get { return _commands; }
        }

        public IObservable<object> Events
        {
            get { return _events; }
        }

        protected void When<TEvent>(IObservable<TEvent> @on, Func<TEvent, IEnumerable<object>> @do)
        {
            Send(@on.Select(@do));
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
            Subscribe(@on.Select(_ => new ProcessCompleted
            {
                ProcessId = _id,
                CorrelationId = _correlationId
            }), _events);
        }

        private void Send(IObservable<IEnumerable<object>> batches)
        {
            Subscribe(batches, batch => _commands.AddRange(batch));
        }

        protected void Subscribe<T>(IObservable<T> observable, IObserver<T> observer)
        {
            _subscriptions.Add(observable.Subscribe(observer));
        }

        protected void Subscribe<T>(IObservable<T> observable, Action<T> onNext)
        {
            _subscriptions.Add(observable.Subscribe(onNext));
        }

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
        }
    }
}