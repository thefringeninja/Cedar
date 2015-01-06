namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Cedar.GetEventStore;
    using Cedar.NEventStore.Handlers;
    using Cedar.NEventStore.Handlers.TempImportFromNES;
    using FluentAssertions;
    using global::NEventStore;
    using Xunit;

    public class DurableCommitDispatcherTests
    {
        [Fact]
        public async Task When_new_commit_then_should_dispatch()
        {
            using (IStoreEvents eventStore = Wireup.Init().UsingInMemoryPersistence().Build())
            {
                var dispatchedEvents = new List<DomainEventMessage<TestEvent>>();
                var handlerModule = new TestHandlerModule(dispatchedEvents);

                using(var host = new DurableCommitDispatcher(
                    new EventStoreClient(eventStore.Advanced),
                    new InMemoryCheckpointRepository(),
                    handlerModule))
                {
                    var projectedCommits = host
                        .ProjectedCommits
                        .Replay();

                    using(projectedCommits.Connect())
                    {
                        await host.Start();

                        var streamId = Guid.NewGuid().ToString().FormatStreamNameWithBucket();

                        Guid commitId = Guid.NewGuid();

                        Task<ICommit> commitProjected = projectedCommits
                            .Take(1)
                            .ToTask();

                        using(IEventStream stream = eventStore.CreateStream(streamId))
                        {
                            stream.Add(new EventMessage { Body = new TestEvent() });
                            stream.CommitChanges(commitId);
                        }
                        host.PollNow();
                        await commitProjected;

                        dispatchedEvents.Count.Should().Be(1);
                        dispatchedEvents[0].Commit().Should().NotBeNull();
                        dispatchedEvents[0].Headers.Should().NotBeNull();
                        dispatchedEvents[0].Version.Should().Be(1);
                        dispatchedEvents[0].DomainEvent.Should().BeOfType<TestEvent>();
                    }
                }
            }
        }

        [Fact]
        public async Task When_handler_throws_Then_invoke_exception_callback()
        {
            using (IStoreEvents eventStore = Wireup.Init().UsingInMemoryPersistence().Build())
            {
                var projectedEvents = new List<DomainEventMessage<TestEvent>>();
                var handlerModule = new TestHandlerModule(projectedEvents);

                using (var host = new DurableCommitDispatcher(
                    new EventStoreClient(eventStore.Advanced),
                    new InMemoryCheckpointRepository(),
                    handlerModule.DispatchCommit))
                {
                    var projectedCommits = host
                        .ProjectedCommits
                        .Replay();

                    using(projectedCommits.Connect())
                    {
                        await host.Start();

                        var streamId = Guid.NewGuid().ToString().FormatStreamNameWithBucket();

                        Guid commitId = Guid.NewGuid();

                        Task<ICommit> commitProjected = projectedCommits
                            .Take(1)
                            .ToTask();

                        using(IEventStream stream = eventStore.CreateStream(streamId))
                        {
                            stream.Add(new EventMessage { Body = new TestEventThatThrows() });
                            stream.CommitChanges(commitId);
                        }
                        host.PollNow();

                        Func<Task> act = async () => await commitProjected;

                        act.ShouldThrow<Exception>();
                    }
                }
            }
        }

        public class TestEvent
        {}

        public class TestEventThatThrows
        { }

        private class TestHandlerModule : HandlerModule
        {
            private readonly List<DomainEventMessage<TestEvent>> _eventsList;

            public TestHandlerModule(List<DomainEventMessage<TestEvent>> eventsList)
            {
                _eventsList = eventsList;

                For<DomainEventMessage<TestEvent>>()
                    .Handle((message, _) =>
                    {
                        _eventsList.Add(message);
                        return Task.FromResult(0);
                    });

                For<DomainEventMessage<TestEventThatThrows>>()
                    .Handle((message, _) =>
                    {
                       throw new Exception();
                    });
            }
        }
    }
}