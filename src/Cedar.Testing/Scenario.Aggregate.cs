namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Domain;
    using FluentAssertions;

    public static partial class Scenario
    {
        public static IAggregateGiven<T> ForAggregate<T>() where T : IAggregate
        {
            return ForAggregate<T>("testid");
        }

        public static IAggregateGiven<T> ForAggregate<T>(string aggregateId) where T : IAggregate
        {
            var factory = new DefaultAggregateFactory();
            var aggregate = (T) factory.Build(typeof (T), aggregateId);
            return new AggregateScenarioBuilder<T>(aggregate);
        }

        public static IAggregateGiven<T> ForAggregate<T>(T aggregate) where T : IAggregate
        {
            return new AggregateScenarioBuilder<T>(aggregate);
        }

        private class AggregateScenarioBuilder<T> : IAggregateGiven<T> where T : IAggregate
        {
            private readonly T _aggregate;
            private Action _given = () => { };
            private Action _when = () => { };

            public AggregateScenarioBuilder(T aggregate)
            {
                _aggregate = aggregate;
            }

            public IAggregateWhen<T> Given(params object[] events)
            {
                _given = () =>
                {
                    foreach (object @event in events)
                    {
                        _aggregate.ApplyEvent(@event);
                    }
                };
                return this;
            }

            public IAggregateThen When(Action<T> when)
            {
                _when = () => when(_aggregate);
                return this;
            }

            public void Then(params object[] expectedEvents)
            {
                _given();
                _when();

                var uncommittedEvents =
                    new List<object>(_aggregate.GetUncommittedEvents().Cast<object>());
                uncommittedEvents.ShouldBeEquivalentTo(expectedEvents);
            }

            public void ThenNothingHappened()
            {
                _given();
                _when();

                var uncommittedEvents =
                    new List<object>(_aggregate.GetUncommittedEvents().Cast<object>());
                uncommittedEvents.Should().BeEmpty();
            }

            public void ThenShouldThrow<TException>() where TException : Exception
            {
                Action then = () => _when();

                _given();
                then.ShouldThrow<TException>();
            }
        }

        public interface IAggregateGiven<out T> : IAggregateWhen<T> where T : IAggregate
        {
            IAggregateWhen<T> Given(params object[] events);
        }

        public interface IAggregateThen
        {
            void Then(params object[] expectedEvents);

            void ThenNothingHappened();

            void ThenShouldThrow<TException>() where TException : Exception;
        }

        public interface IAggregateWhen<out T> : IAggregateThen where T : IAggregate
        {
            IAggregateThen When(Action<T> when);
        }
    }
}