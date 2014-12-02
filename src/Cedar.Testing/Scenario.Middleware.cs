namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.Queries.Client;
    using Cedar.Testing.Owin;
    using Newtonsoft.Json;
    using AppFunc = System.Func<
        System.Collections.Generic.IDictionary<string, object>, 
        System.Threading.Tasks.Task
    >;
    
    using MidFunc = System.Func<
        System.Func<
            System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >, 
        System.Func<
            System.Collections.Generic.IDictionary<string, object>, 
            System.Threading.Tasks.Task
        >
    >;


    public class Query<TRequest, TResult>
    {
        public TRequest Request { get; set; }
        public QueryExecutionSettings QueryExecutionSettings { get; set; }

        public Query(TRequest request, QueryExecutionSettings queryExecutionSettings)
        {
            Request = request;
            QueryExecutionSettings = queryExecutionSettings;
        }
    }

    public static partial class Scenario
    {
        public static Middleware.IHttpClientRequest<TCommand, CommandResult> Does<TCommand>(this IAuthorization authorization, TCommand command)
            where TCommand : class
        {
            return new Middleware.HttpClientCommandRequest<TCommand>(authorization, command);
        }

        public static Middleware.IHttpClientRequest<TInput, TOutput> Queries<TInput, TOutput>(
            this IAuthorization authorization,
            TInput request)
        {
            return new Middleware.HttpClientQueryRequest<TInput, TOutput>(authorization, request);
        }

        public static Middleware.IHttpClientRequest<TCommand, CommandResult> ProcessedWithin<TCommand>(
            this Middleware.IHttpClientRequest<TCommand, CommandResult> request,
            TimeSpan within)
            where TCommand: class
        {
            return new Middleware.HttpClientCommandResultPollingRequest<TCommand>(request, within);
        }

        public static Middleware.IHttpClientRequest<TCommand, CommandResult> ProcessedWithin<TCommand>(
            this Middleware.IHttpClientRequest<TCommand, CommandResult> request,
            int withinMilliseconds)
            where TCommand : class
        {
            return request.ProcessedWithin(TimeSpan.FromMilliseconds(withinMilliseconds));
        }

        public static Middleware.IWithUsers ForMiddleware(
            MidFunc midFunc,
            IMessageExecutionSettings commandSettings,
            IMessageExecutionSettings querySettings,
            string vendor = null,
            [CallerMemberName] string scenarioName = null)
        {
            return new Middleware.ScenarioBuilder(midFunc, commandSettings, querySettings, name: scenarioName);
        }

        public static class Middleware
        {
            public interface IWithUsers : IGiven
            {
                IGiven WithUsers(params IAuthorization[] users);
            }

            public interface IGiven : IWhen
            {
                IWhen Given(params IHttpClientRequest<object, CommandResult>[] given);
            }

            public interface IThen : IScenario
            {
                IThen ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null) where TException : Exception;

                IExpectedResult<TInput, TOutput> Then<TInput, TOutput>(IHttpClientRequest<TInput, TOutput> then);
            }

            public interface IWhen : IThen
            {
                IThen When(IHttpClientRequest<object, CommandResult> when);
            }

            public interface IExpectedResult<TInput, TOutput>
            {
                IThen ShouldEqual(TOutput expected);
                TOutput Expected { get; }
            }

            internal class ScenarioBuilder : IWithUsers
            {
                private readonly IMessageExecutionSettings _commandSettings;
                private readonly IMessageExecutionSettings _querySettings;
                private readonly string _name;
                private readonly AppFunc _appFunc;
                private readonly IDictionary<string, HttpClient> _httpClients;

                private readonly Func<Task> _runGiven;
                private readonly Func<Task> _runWhen;
                private Func<Task> _runThen;
                private Func<Task> _then;


                private IHttpClientRequest<object, CommandResult>[] _given = new IHttpClientRequest<object, CommandResult>[0];
                private IHttpClientRequest<object, CommandResult> _when;
                private bool _passed;
                private readonly Stopwatch _timer;
                private object _results;
                private IList<object> _expect;
                private readonly IList<object> _queryResults;

                public ScenarioBuilder(
                    MidFunc midFunc,
                    IMessageExecutionSettings commandSettings,
                    IMessageExecutionSettings querySettings,
                    AppFunc next = null,
                    string name = null)
                {
                    _commandSettings = commandSettings;
                    _querySettings = querySettings;
                    _name = name;
                    next = next ?? (env =>
                    {
                        var context = new OwinContext(env);
                        context.Response.StatusCode = 404;
                        context.Response.ReasonPhrase = "Not Found";

                        return Task.FromResult(true);
                    });

                   
                    _appFunc = midFunc(next);
                    
                    _httpClients = new Dictionary<string, HttpClient>();

                    _runGiven = async () =>
                    {
                        foreach (IHttpClientRequest<object, CommandResult> userCommand in _given)
                        {
                            await Send(userCommand, commandSettings);
                        }
                    };

                    _runWhen = () => Send(_when, commandSettings);

                    _runThen = async () =>
                    {
                        await _then();

                        var expect = _expect.GetEnumerator();
                        var actual = _queryResults.GetEnumerator();

                        bool hasDifferences = false;

                        var messages = new StringBuilder();
                        
                        using (expect)
                        using(actual)
                        {
                            while(expect.MoveNext() & actual.MoveNext())
                            {
                                var expectedJson = JsonConvert.SerializeObject(expect.Current, Formatting.Indented);
                                var actualJson = JsonConvert.SerializeObject(actual.Current, Formatting.Indented);

                                if(expectedJson != actualJson)
                                {
                                    hasDifferences = true;
                                }

                                messages
                                    .AppendLine("Expected:")
                                    .Append(expectedJson).AppendLine()
                                    .AppendLine("Results:")
                                    .Append(actualJson).AppendLine()
                                    .Append(new string(Enumerable.Repeat('=', 40).ToArray())).AppendLine();
                            }
                        }

                        if(hasDifferences)
                        {
                            throw new ScenarioException(messages.ToString());
                        }
                    };

                    _then = () => Task.FromResult(true);

                    _timer = new Stopwatch();

                    _expect = new List<object>();
                    _queryResults = new List<object>();
                }

                public IGiven WithUsers(params IAuthorization[] users)
                {
                    foreach (IAuthorization user in users)
                    {
                        var httpClient =
                            new HttpClient(
                                new OwinHttpMessageHandler(_appFunc))
                            {
                                BaseAddress = new Uri("http://localhost/"),
                                DefaultRequestHeaders =
                                {
                                    Authorization = user.AuthorizationHeader
                                }
                            };
                        _httpClients.Add(user.Id, httpClient);
                    }

                    return this;
                }


                public IWhen Given(params IHttpClientRequest<object, CommandResult>[] given)
                {
                    _given = given;

                    return this;
                }

                public IThen When(IHttpClientRequest<object, CommandResult> when)
                {
                    _when = when;
                    
                    return this;
                }

                public IThen ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null)
                    where TException : Exception
                {
                    _expect = isMatch != null ? new object[] { typeof(TException), isMatch } : new[] { typeof(TException) };

                    _runThen = () =>
                    {
                        ((ScenarioResult)this).ThenShouldThrow(_results, isMatch);
                        return Task.FromResult(true);
                    };
                    

                    return this;
                }

                public IExpectedResult<TInput, TOutput> Then<TInput, TOutput>(IHttpClientRequest<TInput, TOutput> then)
                {
                    var query = new ExpectedResult<TInput, TOutput>(this, then);

                    var next = _then;

                    _then = async () =>
                    {
                        await next();
                        
                        _expect.Add(query.Expected);
                        
                        _queryResults.Add(await Send(query, _querySettings));
                    };

                    return query;
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
                    try
                    {
                        _timer.Start();

                        try
                        {
                            try
                            {
                                await _runGiven();
                            }
                            catch(Exception ex)
                            {
                                _results = new ScenarioException(ex.Message);

                                return this;
                            }
                            try
                            {
                                await _runWhen();
                            }
                            catch(Exception ex)
                            {
                                _results = ex;
                            }

                            await _runThen();

                            _passed = true;
                        }
                        catch(Exception ex)
                        {
                            _results = ex;

                            return this;
                        }

                        _timer.Stop();

                        return this;
                    }
                    finally
                    {
                        _timer.Stop();
                    }
                }

                private Task<TOutput> Send<TInput, TOutput>(IHttpClientRequest<TInput, TOutput> context, IMessageExecutionSettings settings)
                {
                    if (context == null) throw new ArgumentNullException("context");

                    HttpClient httpClient;

                    if (false == _httpClients.TryGetValue(context.AuthorizationId, out httpClient))
                    {
                        throw new InvalidOperationException(
                            string.Format("No Authorization for '{0}' was found. You must set it up using WithUsers.", context.AuthorizationId));
                    }

                    return context.Sender(httpClient, settings);
                }

                public static implicit operator ScenarioResult(ScenarioBuilder builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._expect, builder._results, builder._timer.Elapsed);
                }
            }

            public interface IHttpClientRequest<out TInput, TOutput>
            {
                Func<HttpClient, IMessageExecutionSettings, Task<TOutput>> Sender { get; }
                string AuthorizationId { get; }
                Guid Id { get; }
                TInput Input { get; }
            }

            internal class HttpClientCommandRequest<TCommand> : IHttpClientRequest<TCommand, CommandResult>
                where TCommand : class
            {
                private readonly IAuthorization _authorization;
                private readonly TCommand _command;
                private readonly Guid _id;
                private readonly Func<HttpClient, IMessageExecutionSettings, Task<CommandResult>> _sender;

                public HttpClientCommandRequest(IAuthorization authorization, TCommand command, Guid? commandId = null)
                {
                    if(authorization == null)
                    {
                        throw new ArgumentNullException("authorization");
                    }
                    if(command == null)
                    {
                        throw new ArgumentNullException("command");
                    }

                    _authorization = authorization;
                    _command = command;
                    _id = commandId ?? Guid.NewGuid();
                    _sender = (client, settings) => client.ExecuteCommand(
                        command,
                        _id,
                        settings);
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<CommandResult>> Sender
                {
                    get { return _sender; }
                }

                public string AuthorizationId
                {
                    get { return _authorization.Id; }
                }

                public TCommand Input
                {
                    get { return _command; }
                }

                public Guid Id
                {
                    get { return _id; }
                }

                public override string ToString()
                {
                    return _command + " (running as " + _authorization + ")";
                }
            }

            internal class HttpClientCommandResultPollingRequest<TCommand> : IHttpClientRequest<TCommand, CommandResult>
                where TCommand : class
            {
                private readonly IHttpClientRequest<TCommand, CommandResult> _inner;
                private readonly TimeSpan _within;

                public HttpClientCommandResultPollingRequest(IHttpClientRequest<TCommand, CommandResult> inner, TimeSpan within)
                {
                    _inner = inner;
                    _within = within;
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<CommandResult>> Sender
                {
                    get
                    {
                        return async (client, settings) =>
                        {
                            var initial = await _inner.Sender(client, settings);

                            var stopwatch = Stopwatch.StartNew();

                            while(stopwatch.Elapsed < _within)
                            {
                                try
                                {
                                    var result = await client.GetCommandStatus(Id, settings);

                                    if(result.HandlersCompleted)
                                    {
                                        return result;
                                    }
                                }
                                catch { }
                            }

                            return initial;
                        };
                    }
                }

                public string AuthorizationId
                {
                    get { return _inner.AuthorizationId; }
                }

                public Guid Id
                {
                    get { return _inner.Id; }
                }

                public TCommand Input
                {
                    get { return _inner.Input; }
                }
            }

            internal class HttpClientQueryRequest<TInput, TOutput> : IHttpClientRequest<TInput, TOutput>
            {
                private readonly IAuthorization _authorization;
                private readonly TInput _input;
                private readonly Guid _id;
                private readonly Func<HttpClient, IMessageExecutionSettings, Task<TOutput>> _sender;

                public HttpClientQueryRequest(
                    IAuthorization authorization,
                    TInput input,
                    Guid? id = null)
                {
                    if(authorization == null)
                    {
                        throw new ArgumentNullException("authorization");
                    }

                    _authorization = authorization;
                    _input = input;
                    _id = id ?? Guid.NewGuid();
                    _sender = (client, settings) => client.ExecuteQuery<TInput, TOutput>(input, _id, settings);
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<TOutput>> Sender
                {
                    get { return _sender; }
                }

                public string AuthorizationId
                {
                    get { return _authorization.Id; }
                }

                public Guid Id
                {
                    get { return _id; }
                }

                public TInput Input
                {
                    get { return _input; }
                }
            }

            internal class ExpectedResult<TInput, TOutput> : IExpectedResult<TInput, TOutput>, IHttpClientRequest<TInput, TOutput>
            {
                private readonly IThen _scenario;
                private readonly IHttpClientRequest<TInput, TOutput> _request;
                private TOutput _expected;

                public ExpectedResult(IThen scenario, IHttpClientRequest<TInput, TOutput> request)
                {
                    _scenario = scenario;
                    _request = request;
                }

                public IThen ShouldEqual(TOutput expected)
                {
                    _expected = expected;
                    return _scenario;
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<TOutput>> Sender
                {
                    get { return _request.Sender; }
                }

                string IHttpClientRequest<TInput, TOutput>.AuthorizationId
                {
                    get { return _request.AuthorizationId; }
                }

                Guid IHttpClientRequest<TInput, TOutput>.Id
                {
                    get { return _request.Id; }
                }

                TInput IHttpClientRequest<TInput, TOutput>.Input
                {
                    get { return _request.Input; }
                }

                public TOutput Expected
                {
                    get { return _expected; }
                }
            }
        }
    }
}