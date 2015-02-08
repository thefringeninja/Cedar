namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Cedar.GetEventStore;
    using Cedar.GetEventStore.Handlers;
    using Cedar.GetEventStore.Serialization;
    using Cedar.Internal;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using EventStore.Core.Data;
    using FluentAssertions;
    using Xunit;
    using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;
    using ResolvedEvent = EventStore.ClientAPI.ResolvedEvent;

    public class ResolvedEventtDispatcherTests : IDisposable
    {
        private readonly ClusterVNode _node;
        private readonly Task _nodeStarted;
        private readonly IEventStoreConnection _connection;

        public ResolvedEventtDispatcherTests()
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
                if(e.NewVNodeState != VNodeState.Master) return;

                source.SetResult(true);
            };

            _node.Start();

            _connection = EmbeddedEventStoreConnection.Create(_node);
        }

        [Fact]
        public async Task When_new_resolved_event_then_should_dispatch()
        {
            await _nodeStarted;

            var dispatchedEvents = new List<EventMessage<TestEvent>>();
            var handlerModule = new TestHandlerModule(dispatchedEvents);

            var serializer = new DefaultGetEventStoreJsonSerializer();

            using(var host = new ResolvedEventDispatcher(
                _connection, serializer,
                new InMemoryCheckpointRepository(),
                new HandlerResolver(handlerModule)))
            {
                var projectedEvents = host
                    .ProjectedEvents.Replay();

                using(projectedEvents.Connect())
                {
                    await host.Start();

                    Task<ResolvedEvent> commitProjected = projectedEvents
                        .Take(1)
                        .ToTask()
                        .WithTimeout(TimeSpan.FromSeconds(5));

                    await
                        _connection.AppendToStreamAsync("events".FormatStreamNameWithBucket(),
                            ExpectedVersion.Any,
                            serializer.SerializeEventData(new TestEvent(), "events", 1));

                    await commitProjected;

                    dispatchedEvents.Count.Should().Be(1);
                    dispatchedEvents[0].Headers.Should().NotBeNull();
                    dispatchedEvents[0].Version.Should().Be(0);
                    dispatchedEvents[0].DomainEvent.Should().BeOfType<TestEvent>();
                }
            }
        }

        [Fact]
        public async Task When_handler_throws_Then_invoke_exception_callback()
        {
            await _nodeStarted;

            var serializer = new DefaultGetEventStoreJsonSerializer();
            var handlerModule = new TestHandlerModule(new List<EventMessage<TestEvent>>());

            using(var host = new ResolvedEventDispatcher(
                _connection, serializer,
                new InMemoryCheckpointRepository(),
                new HandlerResolver(handlerModule)))
            {
                var projectedEvents = host
                    .ProjectedEvents.Replay();

                using(projectedEvents.Connect())
                {
                    await host.Start();

                    Task<ResolvedEvent> commitProjected = projectedEvents
                        .Take(1)
                        .ToTask()
                        .WithTimeout(TimeSpan.FromSeconds(5));

                    await
                        _connection.AppendToStreamAsync("events",
                            ExpectedVersion.Any,
                            serializer.SerializeEventData(new TestEventThatThrows(), "events", 1));

                    Func<Task> act = async () => await commitProjected;

                    act.ShouldThrow<Exception>();
                }
            }
        }

        public class TestEvent
        {}

        public class TestEventThatThrows
        { }

        private class TestHandlerModule : HandlerModule
        {
            private readonly List<EventMessage<TestEvent>> _eventsList;

            public TestHandlerModule(List<EventMessage<TestEvent>> eventsList)
            {
                _eventsList = eventsList;

                For<EventMessage<TestEvent>>()
                    .Handle((message, _) =>
                    {
                        _eventsList.Add(message);
                        return Task.FromResult(0);
                    });

                For<EventMessage<TestEventThatThrows>>()
                    .Handle((message, _) =>
                    {
                       throw new Exception();
                    });
            }
        }

        public void Dispose()
        {
            _node.Stop();
        }
    }
}