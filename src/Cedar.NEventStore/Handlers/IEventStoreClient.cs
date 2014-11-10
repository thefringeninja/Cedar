namespace Cedar.NEventStore.Handlers
{
    using System;
    using System.Threading.Tasks;
    using global::NEventStore;

    public interface IEventStoreClient
    {
        IDisposable Subscribe(string checkpoint, Func<ICommit, Task> onCommit);

        void RetrieveNow();
    }
}