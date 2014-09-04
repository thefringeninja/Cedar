namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Cedar.Domain;

    public static partial class Scenario
    {
        public static Aggregate.Given<T> ForAggregate<T>(Func<string, T> factory = null, string aggregateId = null, [CallerMemberName] string scenarioName = null) where T : IAggregate
        {
            aggregateId = aggregateId ?? "testid";
            factory = factory ?? (id => (T) new DefaultAggregateFactory().Build(typeof (T), id));

            return new Aggregate.ScenarioBuilder<T>(factory, aggregateId, scenarioName);
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
                private readonly Func<string, T> _factory;
                private readonly string _aggregateId;
                private readonly string _name;

                private readonly Action<T> _runGiven;
                private readonly Func<T, Task> _runWhen;
                private Action<T> _runThen;

                private object[] _given;
                private Func<T, Task> _when;
                private object[] _expect;
                
                private Exception _occurredException;

                public ScenarioBuilder(Func<string, T> factory, string aggregateId, string name)
                {
                    _factory = factory;
                    _aggregateId = aggregateId;
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
                        var uncommittedEvents = new List<object>(aggregate.GetUncommittedEvents().Cast<object>());
                        if (false == uncommittedEvents.SequenceEqual(expectedEvents, MessageEqualityComparer.Instance))
                        {
                            throw new ScenarioException(this, "The ocurred events did not equal the expected events.");
                        }
                    };
                    return this;
                }

                public Then ThenNothingHappened()
                {
                    GuardThenNotSet();
                    _expect = new object[0];

                    _runThen = aggregate =>
                    {
                        var uncommittedEvents = new List<object>(aggregate.GetUncommittedEvents().Cast<object>());
                        if (uncommittedEvents.Any())
                        {
                            throw new ScenarioException(this, "No events were expected, yet some events occurred.");
                        }
                    };
                    return this;
                }

                public Then ThenShouldThrow<TException>(Func<TException, bool> isMatch = null) where TException : Exception
                {
                    GuardThenNotSet();
                    isMatch = isMatch ?? (_ => true);
                    _runThen = _ => ((ScenarioResult)this).AssertExceptionMatches(_occurredException, isMatch);
                    return this;
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
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

                async Task<ScenarioResult> IScenario.Run()
                {
                    var aggregate = _factory(_aggregateId);

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

                    return this;
                }

                public static implicit operator ScenarioResult(ScenarioBuilder<T> builder)
                {
                    return new ScenarioResult(builder._name, builder._given, builder._when, builder._expect, builder._occurredException);
                }
            }

            internal class MessageEqualityComparer : IEqualityComparer<object>
            {
                private static bool ReflectionEquals(object x, object y)
                {
                    if (ReferenceEquals(x, y))
                        return true;

                    if (ReferenceEquals(x, null))
                        return false;

                    if (ReferenceEquals(y, null))
                        return false;

                    var type = x.GetType();

                    if (type != y.GetType())
                        return false;

                    if (x == y)
                        return true;

                    if (type.IsValueType)
                        return x.Equals(y);

                    var fieldValues = from field in type.GetFields()
                                      select new
                                      {
                                          member = (MemberInfo)field,
                                          x = field.GetValue(x),
                                          y = field.GetValue(y)
                                      };

                    var propertyValues = from property in type.GetProperties()
                                         select new
                                         {
                                             member = (MemberInfo)property,
                                             x = property.GetValue(x),
                                             y = property.GetValue(y)
                                         };

                    var values = fieldValues.Concat(propertyValues);

                    var differences = (from value in values
                                       where false == ReflectionEquals(value.x, value.y)
                                       select value).ToList();

                    return false == differences.Any();
                }

                new public bool Equals(Object x, Object y)
                {
                    return ReflectionEquals(x, y);
                }

                public int GetHashCode(Object obj)
                {
                    return 0;
                }

                public static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();
            }
        }
    }
}