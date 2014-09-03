namespace Cedar.Handlers
{
    using NEventStore.Client;

    public interface IEventStoreClient
    {
        IObserveCommits ObserveFrom(string checkpoint);
    }
}