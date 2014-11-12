namespace Cedar.Commands
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Cedar.Annotations;
    using Cedar.ExceptionModels.Client;
    using Cedar.Handlers;

    public class CommandResult
    {
        private readonly Guid _commitId;
        private readonly IHandlerResolver _handlerResolver;

        public Guid CommitId
        {
            get { return _commitId; }
        }

        public ExceptionModel Exception { get; private set; }

        private int _handlerCount;
        private int _successfulHandlerCount;
        private int _unsuccessfulHandlerCount;
        private static readonly MethodInfo NotifyEventWrittenInternalMethod;

        public CommandResult(Guid commitId, IHandlerResolver handlerResolver)
        {
            _commitId = commitId;
            _handlerResolver = handlerResolver;
        }

        static CommandResult()
        {
            NotifyEventWrittenInternalMethod = typeof(CommandResult).GetMethod("NotifyEventWrittenInternal",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void NotifyEventWritten(DomainEventMessage @event)
        {
            NotifyEventWrittenInternalMethod
                .MakeGenericMethod(@event.DomainEvent.GetType())
                .Invoke(this, new[] {@event});
        }

        [UsedImplicitly]
        private void NotifyEventWrittenInternal<TEvent>(DomainEventMessage<TEvent> @event)
            where TEvent : class
        {
            Interlocked.Add(ref _handlerCount, _handlerResolver.GetHandlersFor<DomainEventMessage<TEvent>>().Count());
        }

        public void NotifyEventHandledSuccessfully()
        {
            Interlocked.Increment(ref _successfulHandlerCount);
        }


        public void NotifyEventHandledUnsuccessfully()
        {
            Interlocked.Increment(ref _unsuccessfulHandlerCount);
        }
    }
}