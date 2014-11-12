namespace Cedar.Commands
{
    using System;
    using System.Collections.Concurrent;
    using Cedar.Handlers;

    public class CommandResultHandler
    {
        private readonly IHandlerResolver _handlerResolver;
        private readonly ConcurrentDictionary<Guid, CommandResult> _storage;

        public CommandResultHandler(IHandlerResolver handlerResolver, ConcurrentDictionary<Guid, CommandResult> storage)
        {
            _handlerResolver = handlerResolver;
            _storage = storage;
        }

        public void Handle(DomainEventMessage message)
        {
            var commitId = message.GetCommitId();

            if(false == commitId.HasValue)
                return;

            var result = GetOrAddCommandResult(commitId.Value);

            result.NotifyEventWritten(message);
        }

        public void Handle(EventHandled message)
        {
            var commitId = message.CommitId;

            var result = GetOrAddCommandResult(commitId);

            if(message.Successful)
            {
                result.NotifyEventHandledSuccessfully();
            }
            else
            {
                result.NotifyEventHandledUnsuccessfully();
            }
        }



        private CommandResult GetOrAddCommandResult(Guid commitId)
        {
            CommandResult result;
            if (false == _storage.TryGetValue(commitId, out result))
            {
                result = new CommandResult(commitId, _handlerResolver);

                _storage.TryAdd(commitId, result);
            }
            return result;
        }


    }
}