namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.MessageHandling;
    using FluentAssertions;
    using NEventStore;
    using NEventStore.Client;
    using TinyIoC;
    using Xunit;

    public class ProjectionsTests
    {
        [Fact]
        public async Task When_new_commit_create_Then_should_project_domain_event()
        {
            using (var eventStore = Wireup.Init().UsingInMemoryPersistence().Build())
            {
                using (var container = new TinyIoCContainer())
                {
                    var projectedEvents = new List<DomainEventMessage<TestEvent>>();
                    container.Register<IMessageHandler<DomainEventMessage<TestEvent>>, TestMessageHandlerProjector>();
                    container.Register<IList<DomainEventMessage<TestEvent>>>(projectedEvents);
                    var messageDispatcher = new MessageDispatcher(new MessageHandlerResolver(container));

                    using(var host = new ProjectionHost(
                        new EventStoreClient(new PollingClient(eventStore.Advanced)), 
                        new InMemoryCheckpointRepository(),
                        messageDispatcher.DispatchMessage))
                    {
                        await host.Start();
                        Guid streamId = Guid.NewGuid();
                        Guid commitId = Guid.NewGuid();
                        Task<ICommit> commitProjected = host
                            .CommitsProjectedStream
                            .Take(1)
                            .ToTask();

                        using (var stream = eventStore.CreateStream(streamId))
                        {
                            stream.Add(new EventMessage{ Body = new TestEvent() });
                            stream.CommitChanges(commitId);
                        }
                        host.PollNow();
                        await commitProjected;

                        projectedEvents.Count.Should().Be(1);
                        projectedEvents[0].Commit.Should().NotBeNull();
                        projectedEvents[0].EventHeaders.Should().NotBeNull();
                        projectedEvents[0].Version.Should().Be(1);
                    }
                }
            }
        }

        public class TestEvent { }

        public class TestMessageHandlerProjector : IMessageHandler<DomainEventMessage<TestEvent>>
        {
            private readonly IList<DomainEventMessage<TestEvent>> _eventsList;

            public TestMessageHandlerProjector(IList<DomainEventMessage<TestEvent>> eventsList)
            {
                _eventsList = eventsList;
            }

            public Task Project(DomainEventMessage<TestEvent> domainEventMessage, CancellationToken cancellationToken)
            {
                _eventsList.Add(domainEventMessage);
                return Task.FromResult(0);
            }
        }

        private class MessageHandlerResolver : IMessageHandlerResolver
        {
            private readonly TinyIoCContainer _container;

            public MessageHandlerResolver(TinyIoCContainer container)
            {
                _container = container;
            }

            public IEnumerable<IMessageHandler<TEvent>> ResolveAll<TEvent>() where TEvent : class
            {
                return _container.ResolveAll<IMessageHandler<TEvent>>();
            }
        }
    }
}