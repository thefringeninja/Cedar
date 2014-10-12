namespace Cedar.ProcessManagers
{
    using System.Collections.Generic;

    public interface IProcessManager
    {
        string Id { get; }
        int Version { get; }

        IEnumerable<object> GetUncommittedEvents();
        void ClearUncommittedEvents();

        IEnumerable<object> GetUndispatchedCommands();
        void ClearUndispatchedCommands();

        void ApplyEvent(object @event);
    }
}