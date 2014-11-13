namespace Cedar.Commands
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Cedar.Handlers;

    public class CommandResultStorage
    {
        private readonly IEnumerable<IHandlerResolver> _handlerResolvers;
        private readonly ConcurrentDictionary<Guid, CommandResult> _storage;

        public CommandResultStorage(IEnumerable<IHandlerResolver> handlerResolvers)
        {
            _handlerResolvers = handlerResolvers;
            _storage = new ConcurrentDictionary<Guid, CommandResult>();
        }

        public bool TryGetResult(Guid commitId, out CommandResult result)
        {
            return _storage.TryGetValue(commitId, out result);
        }

        public void NotifyEventWritten<TEvent>(Guid commitId) where TEvent : class
        {
            var result = GetOrAddCommandResult(commitId);

            result.NotifyEventWritten<TEvent>();
        }

        public void NotifyEventHandledSuccessfully(Guid commitId)
        {
            var result = GetOrAddCommandResult(commitId);

            result.NotifyEventHandledSuccessfully();
        }

        public void NotifyEventHandledUnsuccessfully(Guid commitId)
        {
            var result = GetOrAddCommandResult(commitId);

            result.NotifyEventHandledUnsuccessfully();
        }

        private CommandResult GetOrAddCommandResult(Guid commitId)
        {
            CommandResult result;
            if (false == _storage.TryGetValue(commitId, out result))
            {
                result = new CommandResult(commitId, _handlerResolvers);

                _storage.TryAdd(commitId, result);
            }
            return result;
        }
    }
}