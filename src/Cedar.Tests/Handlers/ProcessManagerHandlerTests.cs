namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.GetEventStore;
    using Cedar.GetEventStore.Handlers;
    using Cedar.GetEventStore.ProcessManagers;
    using Cedar.GetEventStore.Serialization;
    using Cedar.ProcessManagers;
    using Cedar.ProcessManagers.Messages;
    using Cedar.Serialization;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;
    using FluentAssertions;
    using Xunit;
    using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;

    public class ProcessManagerHandlerTests : IDisposable
    {
        private readonly ClusterVNode _node;
        private readonly Task _nodeStarted;
        private readonly IEventStoreConnection _connection;
        private readonly IList<object> _commands;
        private readonly ISerializer _serializer;
        private readonly Guid _orderId;
        private readonly string _streamName;
        private readonly string _correlationId;
        private readonly ResolvedEventDispatcher _dispatcher;

        public ProcessManagerHandlerTests()
        {
            var source = new TaskCompletionSource<bool>();
            _nodeStarted = source.Task;

            var notListening = new IPEndPoint(IPAddress.None, 0);

            _node = EmbeddedVNodeBuilder.AsSingleNode()
                .WithInternalTcpOn(notListening)
                .WithExternalTcpOn(notListening)
                .WithInternalHttpOn(notListening)
                .WithExternalHttpOn(notListening);

            _node.NodeStatusChanged += (_, e) =>
            {
                if(e.NewVNodeState != VNodeState.Master)
                {
                    return;
                }

                source.SetResult(true);
            };

            _node.Start();

            _connection = EmbeddedEventStoreConnection.Create(_node);

            _commands = new List<object>();

            _serializer = new DefaultGetEventStoreJsonSerializer();

            var commandHandler = new CommandHandlerModule();
            commandHandler.For<ShipOrder>()
                .Handle(message => _commands.Add(message.Command));
            commandHandler.For<BillCustomer>()
                .Handle(message => _commands.Add(message.Command));

            ProcessHandler<OrderFulfillment, CompareablePosition> processHandler = ProcessHandler.For<OrderFulfillment, CompareablePosition>(
                commandHandler,
                new ClaimsPrincipal(),
                new EventStoreClientProcessManagerCheckpointRepository(_connection, _serializer))
                .CorrelateBy<OrderPlaced>(e => e.DomainEvent.OrderId.ToString())
                .CorrelateBy<OrderShipped>(e => e.DomainEvent.OrderId.ToString())
                .CorrelateBy<BillingSucceeded>(e => e.DomainEvent.OrderId.ToString())
                .CorrelateBy<BillingFailed>(e => e.DomainEvent.OrderId.ToString());


            _dispatcher = new ResolvedEventDispatcher(_connection,
                new DefaultGetEventStoreJsonSerializer(),
                new InMemoryCheckpointRepository(),
                processHandler.BuildHandlerResolver(),
                () => { });

            _orderId = Guid.NewGuid();
            _streamName = ("orders-" + _orderId.ToString("n")).FormatStreamNameWithBucket();
            _correlationId = _orderId.ToString();
        }

        protected async Task StartDispatcher()
        {
            await _nodeStarted;

            await _dispatcher.Start();
        }

        [Fact]
        public async Task should_send_commands_and_notify_checkpoint_reached()
        {
            await StartDispatcher();

            await PlaceOrder();

            var checkpointReached = await _dispatcher.ProjectedEvents.Take(2).ToTask().WithTimeout(TimeSpan.FromSeconds(5));

            _commands.Count.Should().Be(1);
            _commands.Single().Should().BeOfType<BillCustomer>();

            checkpointReached.Event.EventType.Should().Be("CheckpointReached");
        }

        [Fact]
        public async Task should_discard_commands_after_checkpoint_reached()
        {
            var result = await PlaceOrder();

            await WriteCheckpoint(result.LogPosition);

            await StartDispatcher();

            await SucceedBilling();

            await _dispatcher.ProjectedEvents.Take(4).ToTask().WithTimeout(TimeSpan.FromSeconds(5));

            _commands.Count.Should().Be(2);
            _commands.Last().Should().BeOfType<ShipOrder>();
        }

        public void Dispose()
        {
            _dispatcher.Dispose();
            _node.Stop();
        }

        private async Task<WriteResult> PlaceOrder()
        {
            await _nodeStarted;

            return await AppendToStream(_streamName,
                ExpectedVersion.NoStream,
                new OrderPlaced
                {
                    OrderId = _orderId
                });
        }

        private async Task<WriteResult> SucceedBilling()
        {
            await _nodeStarted;

            return await AppendToStream(_streamName,
                0,
                new BillingSucceeded
                {
                    OrderId = _orderId
                });
        }

        private async Task WriteCheckpoint(Position? checkpoint)
        {
            await AppendToStream("checkpoints",
                ExpectedVersion.Any,
                new CheckpointReached
                {
                    CheckpointToken = checkpoint.ToCheckpointToken(),
                    CorrelationId = _correlationId,
                    ProcessId = ProcessHandler<OrderFulfillment, CompareablePosition>.DefaultBuildProcessManagerId(_correlationId)
                });
        }

        private Task<WriteResult> AppendToStream(string streamName, int expectedVersion, object e)
        {
            return _connection.AppendToStreamAsync(streamName.FormatStreamNameWithBucket(),
                expectedVersion,
                _serializer.SerializeEventData(e, streamName, 0));
        }

        private class OrderFulfillment : ObservableProcessManager
        {
            protected OrderFulfillment(string id, string correlationId) : base(id, correlationId)
            {
                var orderPlaced = OnEvent<OrderPlaced>();
                var billingSucceeded = OnEvent<BillingSucceeded>();
                var billingFailed = OnEvent<BillingFailed>();
                var orderShipped = OnEvent<OrderShipped>();

                var attemptBilling = Observable.When(orderPlaced.And(billingFailed).Then((placed, failed) => new
                {
                    placed.OrderId,
                    placed.CustomerId,
                    placed.Total
                })).Merge(orderPlaced.Select(placed => new
                {
                    placed.OrderId,
                    placed.CustomerId,
                    placed.Total
                }));

                When(attemptBilling,
                    e => new BillCustomer
                    {
                        BillingId = Guid.NewGuid(),
                        OrderId = e.OrderId,
                        CustomerId = e.CustomerId,
                        Total = e.Total
                    });

                When(billingSucceeded,
                    e => new ShipOrder
                    {
                        CustomerId = e.CustomerId,
                        OrderId = e.OrderId,
                        ShipmentId = Guid.NewGuid()
                    });

                CompleteWhen(orderShipped);
            }
        }

        private class OrderPlaced
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        private class BillCustomer
        {
            public Guid BillingId { get; set; }
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        private class BillingSucceeded
        {
            public Guid BillingId { get; set; }
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        private class BillingFailed
        {
            public Guid BillingId { get; set; }
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
        }

        private class ShipOrder
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public Guid ShipmentId { get; set; }
        }

        private class OrderShipped
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public Guid ShipmentId { get; set; }
        }
    }
}