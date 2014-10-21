namespace Cedar.Handlers
{
    using System;
    using System.Threading.Tasks;
    using NEventStore;

    public interface IEventStoreClient
    {
        IDisposable Subscribe(string checkpoint, Func<ICommit, Task> onCommit);

        void RetrieveNow();
    }
}