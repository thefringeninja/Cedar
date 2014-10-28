namespace Cedar.ProcessManagers
{
    using System;

    public interface IProcessManager : IDisposable
    {
        string Id { get; }
        int Version { get; }

        IObserver<object> Inbox { get; }
        IObservable<object> Commands { get; }
        IObservable<object> Events { get; }
    }
}