namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cedar.ExceptionModels.Client;
    using Cedar.Handlers;

    public class CommandResult
    {
        private readonly Guid _commitId;
        private readonly IEnumerable<IHandlerResolver> _handlerResolvers;

        public Guid CommitId
        {
            get { return _commitId; }
        }

        public int HandlerCount
        {
            get { return _handlerCount; }
        }

        public int SuccessfulHandlerCount
        {
            get { return _successfulHandlerCount; }
        }

        public int UnsuccessfulHandlerCount
        {
            get { return _unsuccessfulHandlerCount; }
        }

        public bool HandlersCompleted
        {
            get { return HandlerCount - (SuccessfulHandlerCount + UnsuccessfulHandlerCount) == 0; }
        }

        private int _handlerCount;
        private int _successfulHandlerCount;
        private int _unsuccessfulHandlerCount;

        public CommandResult(Guid commitId, IEnumerable<IHandlerResolver> handlerResolvers)
        {
            _commitId = commitId;
            _handlerResolvers = handlerResolvers;
        }

        public void NotifyEventWritten<TEvent>()
            where TEvent : class
        {
            var count = _handlerResolvers.Sum(
                handlerResolver => handlerResolver
                    .GetHandlersFor<DomainEventMessage<TEvent>>()
                    .Count());
            Interlocked.Add(ref _handlerCount, count);
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