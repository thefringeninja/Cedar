namespace Cedar.ProcessManagers
{
    using System;
    using System.Collections.Generic;

    public interface IProcessManager : IDisposable
    {
        string Id { get; }
        string CorrelationId { get; }
        int Version { get; }

        IObserver<object> Inbox { get; }
        IEnumerable<object> Commands { get; }
        IObservable<object> Events { get; }
    }
}