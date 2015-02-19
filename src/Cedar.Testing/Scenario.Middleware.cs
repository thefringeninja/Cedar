namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
        >;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
            >
        >;

    static partial class Scenario
    {
        public static Middleware.IWhen ForMiddleware(MidFunc middleware, [CallerMemberName] string scenarioName = null)
        {
            return new Middleware.ScenarioBuilder(middleware, scenarioName);
        }

        public static class Middleware
        {
            public interface IWhen : IScenario
            {
                IWhen When(Func<HttpResponseMessage, Task<HttpRequestMessage>> when,
                    Func<HttpResponseMessage, bool> canContinue = null,
                    TimeSpan? timeout = default(TimeSpan?));

                IWhen ThenShould(Expression<Func<HttpResponse, bool>> assertion);
            }

            internal class ScenarioBuilder : IWhen
            {
                private readonly MidFunc _middleware;
                private readonly string _name;
                private readonly Stopwatch _timer;
                private object _given;
                private object _when;
                private object _results;
                private readonly IList<object> _expect;
                private bool _passed;

                private readonly IList<
                    Tuple<
                        Func<HttpResponseMessage, Task<HttpRequestMessage>>,
                        Func<HttpResponseMessage, bool>,
                        TimeSpan,
                        IList<Expression<Func<HttpResponse, bool>>>
                    >
                > _items;

                private IList<Expression<Func<HttpResponse, bool>>> CurrentAssertions
                {
                    get { return _items.Last().Item4; }
                }

                public ScenarioBuilder(MidFunc middleware, string name)
                {
                    _middleware = middleware;
                    _name = name;
                    _timer = new Stopwatch();
                    _items = new List<
                        Tuple<
                            Func<HttpResponseMessage, Task<HttpRequestMessage>>,
                            Func<HttpResponseMessage, bool>,
                            TimeSpan,
                            IList<Expression<Func<HttpResponse, bool>>>
                        >
                    >();

                    _expect = new List<object>();
                }

                public IWhen When(
                    Func<HttpResponseMessage, Task<HttpRequestMessage>> when, 
                    Func<HttpResponseMessage, bool> canContinue = null, 
                    TimeSpan? timeout = default(TimeSpan?))
                {
                    IList<Expression<Func<HttpResponse, bool>>> assertions =
                        new List<Expression<Func<HttpResponse, bool>>>();

                    _items.Add(Tuple.Create(when, canContinue ?? (_ => true), timeout ?? TimeSpan.FromSeconds(5), assertions));

                    return this;
                }

                public IWhen ThenShould(Expression<Func<HttpResponse, bool>> assertion)
                {
                    CurrentAssertions.Add(assertion);

                    return this;
                }

                string IScenario.Name
                {
                    get { return _name; }
                }

                async Task<ScenarioResult> IScenario.Run()
                {
                    try
                    {
                        _timer.Start();

                        using(var client = _middleware.Terminate().CreateClient())
                        {
                            HttpResponse lastResponse = null;

                            foreach (var item in _items)
                            {
                                var executeRequest = item.Item1;
                                var canContinue = item.Item2;
                                var timeout = item.Item3;
                                var assertions = item.Item4;

                                HttpRequest request = await executeRequest(lastResponse);

                                _expect.Add(request);

                                HttpResponseMessage response = null;

                                var stopwatch = Stopwatch.StartNew();

                                while(response == null
                                      || (false == canContinue(response)))
                                {
                                    response = await client.SendAsync(request);

                                    if(timeout <= stopwatch.Elapsed)
                                    {
                                        break;
                                    }
                                }

                                lastResponse = response;

                                _expect.Add(lastResponse);

                                if(false == canContinue(lastResponse))
                                {
                                    throw new ScenarioException("Timed out waiting for the response to match.");
                                }

                                assertions.ForEach(_expect.Add);

                                (from assertion in assertions
                                    let result = assertion.Compile()(lastResponse)
                                    where false == result
                                    select assertion).ScenarioFailedIfAny();
                            }

                            _results = _expect;

                            _passed = true;

                            return this;
                        }
                    }
                    catch(Exception ex)
                    {
                        _results = ex;
                    }
                    finally
                    {
                        _timer.Stop();
                    }

                    return this;
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                public static implicit operator ScenarioResult(ScenarioBuilder builder)
                {
                    return new ScenarioResult(builder._name,
                        builder._passed,
                        builder._given,
                        builder._when,
                        builder._expect,
                        builder._results,
                        builder._timer.Elapsed);
                }
            }
        }
    }
}