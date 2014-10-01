namespace Cedar.Testing
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static partial class Scenario
    {
        public static Any.IGiven<T> For<T>([CallerMemberName] string scenarioName = null)
        {
            return new Any.ScenarioBuilder<T>(scenarioName);
        }

        public static class Any
        {
            public interface IGiven<T> : IWhen<T>
            {
                IWhen<T> Given(T instance);
                IWhen<T> Given(Expression<Func<T>> given);
            }

            public interface IWhen<T> : IThen<T>
            {
                IThen<T> When(Expression<Func<T, Task<T>>> when);
            }

            public interface IThen<T> : IScenario
            {
                IThen<T> ThenShouldEqual(T other);

                IThen<T> ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null) where TException : Exception;
            }

            internal class ScenarioBuilder<T> : IGiven<T>
            {
                private readonly string _name;
                
                private readonly Func<T> _runGiven;
                private readonly Func<T, Task<T>> _runWhen;
                private Action<T> _runThen = _ => { };
                private Expression<Func<T>> _given;
                private Expression<Func<T, Task<T>>> _when;
                private T _expect;
                private object _results;
                private bool _passed;
                private readonly Stopwatch _timer;

                public ScenarioBuilder(string name)
                {
                    _name = name;

                    _runGiven = () => _given.Compile()();
                    _runWhen = instance => _when.Compile()(instance);
                    _timer = new Stopwatch();
                }

                public IWhen<T> Given(T instance)
                {
                    return Given(() => instance);
                }

                public IWhen<T> Given(Expression<Func<T>> given)
                {
                    _given = given;

                    return this;
                }

                public IThen<T> When(Expression<Func<T, Task<T>>> when)
                {
                    _when = when;

                    return this;
                }

                public IThen<T> ThenShouldEqual(T other)
                {
                    _runThen = instance =>
                    {
                        if (false == instance.Equals(other))
                        {
                            throw new ScenarioException(String.Format("{0} was expected to equal {1}.", instance, other));
                        }
                    };
                    return this;
                }

                public IThen<T> ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception
                {
                    _runThen = _ =>  ((ScenarioResult)this).ThenShouldThrow(_results, isMatch);

                    return this;
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                string IScenario.Name
                {
                    get { return _name; }
                }



                async Task<ScenarioResult> IScenario.Run()
                {
                    _timer.Start();
                    try
                    {
                        T sut;

                        try
                        {
                            sut = _runGiven();
                        }
                        catch (Exception ex)
                        {
                            _results = new ScenarioException(ex.Message);

                            return this;
                        }

                        try
                        {
                            _expect = await _runWhen(sut);
                        }
                        catch (Exception ex)
                        {
                            _results = ex;
                        }

                        _runThen(_expect);

                        _passed = true;
                    }
                    catch (Exception ex)
                    {
                        _results = ex;

                        return this;
                    }

                    _timer.Stop();
                    
                    return this;
                }

                public static implicit operator ScenarioResult(ScenarioBuilder<T> builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._expect, builder._results, builder._timer.Elapsed);
                }
            }
        }
    }
}