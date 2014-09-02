namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using FluentAssertions;
    using NEventStore;
    using NEventStore.Client;
    using Xunit;

    public class ProjectionsTests
    {
        [Fact]
        public async Task When_new_commit_create_Then_should_project_domain_event()
        {
            using (IStoreEvents eventStore = Wireup.Init().UsingInMemoryPersistence().Build())
            {
                var projectedEvents = new List<DomainEventMessage<TestEvent>>();
                var handlerModule = new TestHandlerModule(projectedEvents);

                using (var host = new ProjectionHost(
                    new EventStoreClient(new PollingClient(eventStore.Advanced)),
                    new InMemoryCheckpointRepository(),
                    handlerModule))
                {
                    await host.Start();
                    Guid streamId = Guid.NewGuid();
                    Guid commitId = Guid.NewGuid();
                    Task<ICommit> commitProjected = host
                        .CommitsProjectedStream
                        .Take(1)
                        .ToTask();

                    using (IEventStream stream = eventStore.CreateStream(streamId))
                    {
                        stream.Add(new EventMessage {Body = new TestEvent()});
                        stream.CommitChanges(commitId);
                    }
                    host.PollNow();
                    await commitProjected;

                    projectedEvents.Count.Should().Be(1);
                    projectedEvents[0].Commit.Should().NotBeNull();
                    projectedEvents[0].EventHeaders.Should().NotBeNull();
                    projectedEvents[0].Version.Should().Be(1);
                    projectedEvents[0].DomainEvent.Should().BeOfType<TestEvent>();
                }
            }
        }

        public class TestEvent
        {}

        private class TestHandlerModule : HandlerModule
        {
            private readonly IList<DomainEventMessage<TestEvent>> _eventsList;

            public TestHandlerModule(IList<DomainEventMessage<TestEvent>> eventsList)
            {
                _eventsList = eventsList;

                For<DomainEventMessage<TestEvent>>()
                    .Handle((message, _) =>
                    {
                        _eventsList.Add(message);
                        return Task.FromResult(0);
                    });
            }
        }
    }
}