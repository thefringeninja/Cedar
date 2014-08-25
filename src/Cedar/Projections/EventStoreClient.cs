namespace Cedar.Projections
{
    using NEventStore.Client;

    public class EventStoreClient : IEventStoreClient
    {
        private readonly PollingClient _pollingClient;

        public EventStoreClient(PollingClient pollingClient)
        {
            _pollingClient = pollingClient;
        }

        public IObserveCommits ObserveFrom(string checkpoint)
        {
            return _pollingClient.ObserveFrom(checkpoint);
        }
    }
}