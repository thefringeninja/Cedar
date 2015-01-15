namespace Cedar.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.NEventStore.Domain.Persistence;
    using FluentAssertions;
    using global::NEventStore;
    using Xunit;

    public class NEventStoreRepositoryTests
    {
        class SomethingHappened
        {
            public override string ToString()
            {
                return "The following happened: \r\n\tSomething Interesting.";
            }
        }
        class SomeAggregate : AggregateBase
        {
            public SomeAggregate(Guid id)
                : this("someaggregate-" + id)
            {
                
            }
            protected SomeAggregate(string id) : base(id)
            {}

            public void DoSomething()
            {
                RaiseEvent(new SomethingHappened());
            }

            void Apply(SomethingHappened e)
            {
                
            }
        }
        [Fact]
        public async Task persisting_commits()
        {
            var repository = new NEventStoreRepository(Wireup.Init().UsingInMemoryPersistence().Build());

            var id = Guid.NewGuid();

            var aggregate = new SomeAggregate(id);
            
            aggregate.DoSomething();
            
            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.DoSomething();

            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.Version.Should().Be(2);
        }


        [Fact]
        public async Task commits_with_multiple_events()
        {
            var repository = new NEventStoreRepository(Wireup.Init().UsingInMemoryPersistence().Build());

            var id = Guid.NewGuid();

            var aggregate = new SomeAggregate(id);

            aggregate.DoSomething();

            aggregate.DoSomething();

            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.DoSomething();

            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.Version.Should().Be(3);
        }


        [Fact]
        public async Task loading_an_empty_aggregate()
        {
            var repository = new NEventStoreRepository(Wireup.Init().UsingInMemoryPersistence().Build());

            var id = Guid.NewGuid();

            var aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.DoSomething();

            aggregate.DoSomething();

            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.DoSomething();

            await repository.Save(aggregate, Guid.NewGuid(), new CancellationToken());

            aggregate = await repository.GetById<SomeAggregate>("someaggregate-" + id, Int32.MaxValue, new CancellationToken());

            aggregate.Version.Should().Be(3);
        }
    }
}