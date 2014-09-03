namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Cedar.Domain;
    using FluentAssertions;

    public static partial class Scenario
    {
        public static Aggregate.Given<T> ForAggregate<T>(Func<T> factory = null, string aggregateId = null, [CallerMemberName] string scenarioName = null) where T : IAggregate
        {
            aggregateId = aggregateId ?? "testid";
            factory = factory ?? (() => (T) new DefaultAggregateFactory().Build(typeof (T), aggregateId));

            return new Aggregate.ScenarioBuilder<T>(factory, scenarioName);
        }

        public static class Aggregate
        {
            public interface Given<out T> : When<T> where T : IAggregate
            {
                When<T> Given(params object[] events);
            }

            public interface When<out T> : Then where T : IAggregate
            {
                Then When(Action<T> when);
                Then When(Func<T, Task> when);
            }

            public interface Then : IScenario
            {
                Then Then(params object[] expectedEvents);

                Then ThenNothingHappened();

                Then ThenShouldThrow<TException>(Func<TException, bool> isMatch = null) where TException : Exception;
            }

            internal class ScenarioBuilder<T> : Given<T> where T : IAggregate
            {
                private readonly Func<T> _factory;
                private readonly string _name;

                private readonly Action<T> _runGiven;
                private readonly Func<T, Task> _runWhen;
                private Action<T> _runThen;

                private object[] _given;
                private Func<T, Task> _when;
                private object[] _expect;
                
                private Exception _occurredException;

                public ScenarioBuilder(Func<T> factory, string name)
                {
                    _factory = factory;
                    _name = name;
                    _runGiven = aggregate =>
                    {
                        foreach (var @event in _given)
                        {
                            aggregate.ApplyEvent(@event);
                        }
                    };
                    _runWhen = aggregate => _when(aggregate);
                }

                public When<T> Given(params object[] events)
                {
                    _given = events;
                    return this;
                }

                public Then When(Func<T, Task> when)
                {
                    _when = when;
                    return this;
                }

                public Then When(Action<T> when)
                {
                    return When(async aggregate => when(aggregate));
                }

                public Then Then(params object[] expectedEvents)
                {
                    GuardThenNotSet();
                    _expect = expectedEvents;

                    _runThen = aggregate =>
                    {
                        var uncommittedEvents =
                            new List<object>(aggregate.GetUncommittedEvents().Cast<object>());
                        uncommittedEvents.ShouldBeEquivalentTo(expectedEvents);
                    };
                    return this;
                }

                public Then ThenNothingHappened()
                {
                    GuardThenNotSet();
                    _expect = new object[0];

                    _runThen = aggregate =>
                    {
                        var uncommittedEvents =
                            new List<object>(aggregate.GetUncommittedEvents().Cast<object>());
                        uncommittedEvents.Should().BeEmpty();
                    };
                    return this;
                }

                public Then ThenShouldThrow<TException>(Func<TException, bool> isMatch = null) where TException : Exception
                {
                    GuardThenNotSet();
                    isMatch = isMatch ?? (_ => true);
                    _runThen = _ =>
                    {
                        _occurredException.Should().BeOfType<TException>();
                        isMatch((TException) _occurredException).Should().BeTrue();
                    };
                    return this;
                }

                public TaskAwaiter GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                void GuardThenNotSet()
                {
                    if (_runThen !=null) throw new InvalidOperationException("Then already set.");
                }

                string IScenario.Name
                {
                    get { return _name; }
                }

                Task IScenario.Print(TextWriter writer)
                {
                    throw new NotImplementedException();
                }

                async Task IScenario.Run()
                {
                    var aggregate = _factory();
                    _runGiven(aggregate);

                    try
                    {
                        await _runWhen(aggregate);
                    }
                    catch (Exception ex)
                    {
                        _occurredException = ex;
                    }

                    _runThen(aggregate);
                }
            }
        }
    }
}