namespace Cedar.Handlers
{
    using NEventStore.Client;

    // TO BE REMOVED WITH NES6
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