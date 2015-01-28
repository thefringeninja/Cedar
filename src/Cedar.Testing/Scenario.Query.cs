namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
    >;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >
    >;

    public static partial class Scenario
    {
        public static Query.IGiven ForQuery(IHandlerResolver handlerResolver, MidFunc middleware, [CallerMemberName] string scenarioName = null)
        {
            return new Query.ScenarioBuilder(handlerResolver, middleware, scenarioName);
        }

        public static class Query
        {
            public interface IGiven : IWhen
            {
                IWhen Given(params object[] events);
            }

            public interface IWhen : IThen
            {
                IThen When(Func<Task<HttpRequestMessage>> request);
            }

            public interface IThen : IScenario
            {
                IThen ThenShould(Expression<Func<HttpResponse, bool>> assertion);
            }

            internal class ScenarioBuilder : IGiven
            {
                private static readonly MethodInfo DispatchDomainEventMethod;

                private readonly IHandlerResolver _module;
                private readonly string _name;
                private bool _passed;
                private object[] _given;
                private HttpRequest _when;
                private object _results;
                private readonly Func<Task> _runGiven;
                private readonly Func<Task> _runWhen;
                private readonly Action _runThen;
                private readonly Stopwatch _timer;
                private readonly IList<Expression<Func<HttpResponse, bool>>> _assertions;
                private Func<Task<HttpRequestMessage>> _request;

                static ScenarioBuilder()
                {
                    DispatchDomainEventMethod = typeof(HandlerModuleExtensions)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Single(method => method.Name == "Dispatch" && method.GetParameters().First().ParameterType == typeof(IHandlerResolver));
                }

                public ScenarioBuilder(IHandlerResolver module, MidFunc middleware, string name)
                {
                    _module = module;
                    _name = name;
                    _given = new object[0];

                    _runGiven = async () =>
                    {
                        foreach(var @event in _given.Select(WrapInEnvelopeIfNecessary))
                        {
                            var task = (Task) DispatchDomainEventMethod.MakeGenericMethod(@event.GetType())
                                .Invoke(null, new object[] {_module, @event, new CancellationToken(),});

                            await task;
                        }
                    };
                    _runWhen = async () =>
                    {
                        using(var client = middleware.Terminate().CreateClient())
                        {
                            _when = await _request();

                            _results = (HttpResponse) await client.SendAsync(_when);
                        }
                    };
                    _runThen = () =>
                    {
                        var response = (HttpResponse)_results;
                        
                        var failed = (from assertion in _assertions
                            let result = assertion.Compile()(response)
                            where false == result
                            select assertion).ToList();

                        if(failed.Any())
                        {
                            throw new ScenarioException("The following assertions failed:" + Environment.NewLine + failed.Aggregate(
                                new StringBuilder(),
                                (builder, assertion) =>
                                    builder.Append('\t').Append(PAssertFormatter.CreateSimpleFormatFor(assertion)).AppendLine()));
                        }
                    };
                    _assertions = new List<Expression<Func<HttpResponse, bool>>>();
                    _timer = new Stopwatch();
                }

                public IThen ThenShould(Expression<Func<HttpResponse, bool>> assertion)
                {
                    _assertions.Add(assertion);

                    return this;
                }

                public IThen When(Func<Task<HttpRequestMessage>> request)
                {
                    _request = request;

                    return this;
                }
                
                public IWhen Given(params object[] events)
                {
                    _given = events;

                    return this;
                }

                public string Name
                {
                    get { return _name; }
                }

                async Task<ScenarioResult> IScenario.Run()
                {
                    try
                    {
                        _timer.Start();

                        try
                        {
                            await _runGiven();
                        }
                        catch(Exception ex)
                        {
                            _results = ex;

                            return this;
                        }

                        try
                        {
                            await _runWhen();
                        }
                        catch(Exception ex)
                        {
                            _results = ex;

                            return this;
                        }

                        try
                        {
                            _runThen();

                            _passed = true;
                        }
                        catch(Exception ex)
                        {
                            _results = ex;
                        }

                        return this;
                    }
                    finally 
                    {
                        _timer.Stop();
                    }
                }

                public TaskAwaiter<ScenarioResult> GetAwaiter()
                {
                    IScenario scenario = this;

                    return scenario.Run().GetAwaiter();
                }

                public static implicit operator ScenarioResult(ScenarioBuilder builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._assertions, builder._results, builder._timer.Elapsed);
                }

                private static DomainEventMessage WrapInEnvelopeIfNecessary(object @event)
                {
                    return @event as DomainEventMessage
                           ?? (DomainEventMessage)Activator.CreateInstance(
                               typeof(DomainEventMessage<>).MakeGenericType(
                                   @event.GetType()),
                               new[] { "streamId", @event, 0, new Dictionary<string, object>(), null });
                }

            }
        }
    }
}