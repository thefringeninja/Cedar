namespace Cedar.Projections
{
    using NEventStore.Client;

    public interface IEventStoreClient
    {
        IObserveCommits ObserveFrom(string checkpoint);
    }
}