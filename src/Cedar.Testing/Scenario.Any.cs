namespace Cedar.Testing
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static partial class Scenario
    {
        public static Any.IGiven<T, T> For<T>([CallerMemberName] string scenarioName = null)
        {
            return For<T, T>(scenarioName);
        }

        public static Any.IGiven<T, TResult> For<T, TResult>([CallerMemberName] string scenarioName = null)
        {
            return new Any.ScenarioBuilder<T, TResult>(scenarioName);
        }

        public static class Any
        {
            public interface IGiven<T, TResult> : IWhen<T, TResult>
            {
                IWhen<T, TResult> Given(Expression<Func<T>> given);
            }

            public interface IWhen<T, TResult> : IThen<TResult>
            {
                IThen<TResult> When(Expression<Func<T, TResult>> when);

                IThen<TResult> When(Expression<Func<T, Task<TResult>>> when);
            }

            public interface IThen<TResult> : IScenario
            {
                IThen<TResult> ThenShouldEqual(TResult other);

                IThen<TResult> ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null) where TException : Exception;
            }

            internal class ScenarioBuilder<T, TResult> : IGiven<T, TResult>
            {
                private readonly string _name;
                
                private readonly Func<T> _runGiven;
                private Func<T, Task<TResult>> _runWhen;
                private Action<TResult> _runThen = _ => { };
                private Expression<Func<T>> _given;
                private LambdaExpression _when;
                private TResult _expect;
                private object _results;
                private bool _passed;
                private readonly Stopwatch _timer;

                public ScenarioBuilder(string name)
                {
                    _name = name;

                    _runGiven = () => _given.Compile()();
                    _timer = new Stopwatch();
                }

                public IWhen<T, TResult> Given(Expression<Func<T>> given)
                {
                    _given = given;

                    return this;
                }

                public IThen<TResult> When(Expression<Func<T, TResult>> when)
                {
                    _when = when;

                    _runWhen = instance => Task.FromResult(when.Compile()(instance));
                    
                    return this;
                }

                public IThen<TResult> When(Expression<Func<T, Task<TResult>>> when)
                {
                    _when = when;

                    _runWhen = instance => when.Compile()(instance);

                    return this;
                }

                public IThen<TResult> ThenShouldEqual(TResult other)
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

                public IThen<TResult> ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
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

                public static implicit operator ScenarioResult(ScenarioBuilder<T, TResult> builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._expect, builder._results, builder._timer.Elapsed);
                }
            }
        }
    }
}