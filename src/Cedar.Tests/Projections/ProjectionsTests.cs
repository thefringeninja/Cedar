namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NEventStore;
    using NEventStore.Client;
    using TinyIoC;
    using Xunit;

    public class ProjectionsTests
    {
        [Fact]
        public async Task Blah()
        {
            using (var eventStore = Wireup.Init().UsingInMemoryPersistence().Build())
            {
                using (var container = new TinyIoCContainer())
                {
                    var projectedEvents = new List<Tuple<IDomainEventContext, TestEvent>>();
                    container.Register<IProjectDomainEvent<TestEvent>, TestEventProjector>();
                    container.Register<IList<Tuple<IDomainEventContext, TestEvent>>>(projectedEvents);
                    using(var host = new ProjectionHost(
                        new EventStoreClient(new PollingClient(eventStore.Advanced, 10)), 
                        new InMemoryCheckpointRepository(),
                        new ProjectionResolver(container)))
                    {
                        IObservable<ICommit> firstCommit = host.CommitsProjectedStreamSteam.FirstAsync();

                        Guid streamId = Guid.NewGuid();
                        Guid commitId = Guid.NewGuid();
                        using (var stream = eventStore.CreateStream(streamId))
                        {
                            stream.Add(new EventMessage{ Body = new TestEvent() });
                            stream.CommitChanges(commitId);
                        }
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
                _eventsList.Add(new Tuple<IDomainEventContext, TestEvent>(context, domainEvent));
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