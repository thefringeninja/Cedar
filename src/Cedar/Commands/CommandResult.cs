namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cedar.Handlers;

    internal class CommandResult
    {
        private readonly Guid _commandId;
        private readonly IEnumerable<IHandlerResolver> _handlerResolvers;
        private int _handlerCount;
        private int _successfulHandlerCount;
        private int _unsuccessfulHandlerCount;

        public CommandResult(Guid commandId, IEnumerable<IHandlerResolver> handlerResolvers)
        {
            _commandId = commandId;
            _handlerResolvers = handlerResolvers;
        }

        public Guid CommandId
        {
            get { return _commandId; }
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

        internal void NotifyEventWritten<TEvent>()
            where TEvent : class
        {
            int count = _handlerResolvers.Sum(
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