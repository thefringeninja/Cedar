namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;
    using Cedar.Handlers;

    public interface IProcessManager : IDisposable
    {
        string Id { get; }
        string CorrelationId { get; }
        int Version { get; }

        IObserver<EventMessage> Inbox { get; }
        IEnumerable<object> Commands { get; }
        IObservable<object> Events { get; }
    }
}