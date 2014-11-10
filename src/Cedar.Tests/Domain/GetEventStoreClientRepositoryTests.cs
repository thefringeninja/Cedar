namespace Cedar.Domain
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Cedar.Domain.Persistence;
    using Cedar.GetEventStore.Domain.Persistence;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;
    using Xunit;

    public class GetEventStoreClientRepositoryTests : IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private readonly ClusterVNode _node;
        private readonly EventStoreClientRepository<Aggregate> _repository;
        private readonly Task _eventStoreInitialized;
        private readonly Guid _id;
        private readonly string _streamId;

        public GetEventStoreClientRepositoryTests()
        {
            var source = new TaskCompletionSource<bool>();
            _eventStoreInitialized = source.Task;

            var notListening = new IPEndPoint(IPAddress.None, 0);

            _node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .WithExternalTcpOn(notListening)
                .WithInternalTcpOn(notListening)
                .WithExternalHttpOn(notListening)
                .WithInternalHttpOn(notListening)
                .RunProjections(ProjectionsMode.All);

            _node.NodeStatusChanged += (_, e) =>
            {
                if (e.NewVNodeState != VNodeState.Master)
                {
                    return;
                }
                source.SetResult(true);
            };

            _connection = EmbeddedEventStoreConnection.Create(_node);

            _repository = new EventStoreClientRepository<Aggregate>(_connection, new DefaultGetEventStoreJsonSerializer());

            _node.Start();

            _id = Guid.NewGuid();

            _streamId = "aggregate-" + _id.ToString("n");
        }

        [Fact]
        public async Task persisting_events()
        {
            await _eventStoreInitialized;

            var aggregate = new Aggregate(_id);

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId);

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId);

            Assert.Equal(2, aggregate.Version);
        }

        [Fact]
        public async Task persisting_multiple_events()
        {
            await _eventStoreInitialized;

            var aggregate = new Aggregate(_id);

            aggregate.DoSomething();

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId);

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId);

            Assert.Equal(3, aggregate.Version);
        }


        [Fact]
        public async Task loading_an_empty_aggregate()
        {
            await _eventStoreInitialized;

            var aggregate = await _repository.GetById(_streamId);

            Assert.Null(aggregate);
        }

        [Fact]
        public async Task only_load_the_requested_version()
        {
            await _eventStoreInitialized;

            var aggregate = new Aggregate(_id);

            aggregate.DoSomething();

            aggregate.DoSomething();

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId, 2);

            Assert.Equal(2, aggregate.Version);
        }

        [Fact]
        public async Task throw_an_exception_on_duplicate_write()
        {
            await _eventStoreInitialized;

            var aggregate = new Aggregate(_id);

            aggregate.DoSomething();

            aggregate.DoSomething();

            aggregate.DoSomething();

            await _repository.Save(aggregate);

            aggregate = await _repository.GetById(_streamId, 2);

            aggregate.DoSomething();

            Exception caughtException = null;

            try
            {
                await _repository.Save(aggregate);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            Assert.NotNull(caughtException);
            Assert.IsType<Exception>(caughtException);
        }


        public void Dispose()
        {
            _node.Stop();
            _connection.Dispose();
        }

        private class Aggregate : AggregateBase
        {
            public Aggregate(Guid id)
                : this("aggregate-" + id.ToString("n"))
            {

            }
            protected Aggregate(string id)
                : base(id)
            { }

            public void DoSomething()
            {
                RaiseEvent(new Somethinged());
            }

            void Apply(Somethinged e) { }
        }

        private class Somethinged
        { }
    }
}