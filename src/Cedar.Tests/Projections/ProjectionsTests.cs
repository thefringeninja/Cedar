namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
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
                    var projectedEvents = new List<Tuple<IDomainEventContext, TestEvent>>();
                    container.Register<IProjectDomainEvent<TestEvent>, TestEventProjector>();
                    container.Register<IList<Tuple<IDomainEventContext, TestEvent>>>(projectedEvents);

                    using(var host = new ProjectionHost(
                        new EventStoreClient(new PollingClient(eventStore.Advanced)), 
                        new InMemoryCheckpointRepository(),
                        new ProjectionResolver(container)))
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
                        projectedEvents[0].Item1.Commit.Should().NotBeNull();
                        projectedEvents[0].Item1.EventHeaders.Should().NotBeNull();
                        projectedEvents[0].Item1.Version.Should().Be(1);
                        projectedEvents[0].Item2.Should().BeOfType<TestEvent>();
                    }
                }
            }
        }

        public class TestEvent { }

        public class TestEventProjector : IProjectDomainEvent<TestEvent>
        {
            private readonly IList<Tuple<IDomainEventContext, TestEvent>> _eventsList;

            public TestEventProjector(IList<Tuple<IDomainEventContext, TestEvent>> eventsList)
            {
                _eventsList = eventsList;
            }

            public Task Project(IDomainEventContext context, TestEvent domainEvent, CancellationToken cancellationToken)
            {
                _eventsList.Add(Tuple.Create(context, domainEvent));
                return Task.FromResult(0);
            }
        }

        private class ProjectionResolver : IProjectionResolver
        {
            private readonly TinyIoCContainer _container;

            public ProjectionResolver(TinyIoCContainer container)
            {
                _container = container;
            }

            public IEnumerable<IProjectDomainEvent<TEvent>> ResolveAll<TEvent>() where TEvent : class
            {
                return _container.ResolveAll<IProjectDomainEvent<TEvent>>();
            }
        }
    }
}