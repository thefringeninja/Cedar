namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;

    public interface IProcessManager
    {
        string Id { get; }
        Guid CorrelationId { get; }
        int Version { get; }

        IEnumerable<object> GetUncommittedEvents();
        void ClearUncommittedEvents();

        IEnumerable<object> GetUndispatchedCommands();
        void ClearUndispatchedCommands();

        void ApplyEvent(object @event);
    }
}