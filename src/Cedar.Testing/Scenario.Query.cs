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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Cedar.Testing.LibOwin;
    using PowerAssert;
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
                IThen When(Task<HttpRequestMessage> request);
                IThen When(HttpRequest request);
                IThen When(Task<HttpRequest> request);
            }

            public interface IThen : IScenario
            {
                IThen ThenShould(Expression<Func<HttpResponseMessage, bool>> assertion);
            }

            public class HttpRequest
            {
                private readonly HttpRequestMessage _request;

                internal HttpRequest(HttpRequestMessage request)
                {
                    _request = request;
                }

                public static implicit operator HttpRequestMessage(HttpRequest request)
                {
                    return request._request;
                }
                public static implicit operator HttpRequest(HttpRequestMessage request)
                {
                    return new HttpRequest(request);
                }

                public override string ToString()
                {
                    var flattenedHeaders = (from headers in _request.Headers.Union((_request.Content ?? new StringContent(String.Empty)).Headers)
                        let values = headers.Value
                        from value in values
                        select new { key = headers.Key, value });

                    var builder = new StringBuilder();

                    builder.Append(_request.Method.ToString().ToUpper())
                        .Append(' ')
                        .Append(_request.RequestUri.PathAndQuery)
                        .Append(' ')
                        .Append("HTTP/").Append(_request.Version.ToString(2))
                        .AppendLine();

                    flattenedHeaders.Aggregate(builder,
                        (sb, header) => sb.Append(header.key).Append(':').Append(' ').Append(header.value).AppendLine());
                    
                    return builder.ToString();
                }
            }

            public class HttpResponse
            {
                private readonly HttpResponseMessage _response;

                internal HttpResponse(HttpResponseMessage response)
                {
                    _response = response;
                }

                public static implicit operator HttpResponseMessage(HttpResponse response)
                {
                    return response._response;
                }
                public static implicit operator HttpResponse(HttpResponseMessage response)
                {
                    return new HttpResponse(response);
                }

                public override string ToString()
                {
                    var flattenedHeaders = (from headers in _response.Headers.Union((_response.Content ?? new StringContent(String.Empty)).Headers)
                                            let values = headers.Value
                                            from value in values
                                            select new { key = headers.Key, value });

                    var builder = new StringBuilder();

                    builder
                        .Append("HTTP/").Append(_response.Version.ToString(2)).Append(' ')
                        .Append((int) _response.StatusCode).Append(' ')
                        .Append(_response.ReasonPhrase)
                        .AppendLine();

                    flattenedHeaders.Aggregate(builder,
                        (sb, header) => sb.Append(header.key).Append(':').Append(' ').Append(header.value).AppendLine());

                    return builder.ToString();

                }
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
                private readonly IList<Expression<Func<HttpResponseMessage, bool>>> _assertions;
                private Task<HttpRequest> _request;

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
                        var client = new HttpClient(new OwinHttpMessageHandler(middleware(env =>
                        {
                            var context = new OwinContext(env)
                            {
                                Response = { StatusCode = 404, ReasonPhrase = "Not Found" }
                            };

                            return Task.FromResult(0);
                        })))
                        {
                            BaseAddress = new Uri("http://localhost/")
                        };
                        
                        _when = await _request;
                        
                        _results = (HttpResponse)await client.SendAsync(_when);
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
                    _assertions = new List<Expression<Func<HttpResponseMessage, bool>>>();
                    _timer = new Stopwatch();
                }

                public IThen ThenShould(Expression<Func<HttpResponseMessage, bool>> assertion)
                {
                    _assertions.Add(assertion);

                    return this;
                }

                public IThen When(HttpRequest request)
                {
                    _request = Task.FromResult(request);

                    return this;
                }

                public IThen When(Task<HttpRequest> request)
                {
                    _request = request;

                    return this;
                }

                public IThen When(Task<HttpRequestMessage> request)
                {
                    _request = request.ContinueWith(task => (HttpRequest)task.Result);

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