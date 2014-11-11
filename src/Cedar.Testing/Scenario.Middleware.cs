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
        public static Middleware.IHttpClientRequest Does<TCommand>(this IAuthorization authorization, TCommand command)
            where TCommand : class
        {
            return new Middleware.HttpClientCommandRequest<TCommand>(authorization, command);
        }

        public static Middleware.IHttpClientRequest<TResponse> Queries<TResponse>(this IAuthorization authorization,
            Func<HttpClient, Task<TResponse>> query)
        {
            return new Middleware.HttpClientQueryRequest<TResponse>(authorization, query);
        }

        public static Middleware.IHttpClientRequest<TResponse> Queries<TRequest, TResponse>(this IAuthorization user, Query<TRequest, TResponse> query)
        {
            return user.Queries((client) => client.ExecuteQuery<TRequest, TResponse>(query.Request, Guid.NewGuid(), query.QueryExecutionSettings));
        }


        public static Middleware.IWithUsers ForMiddleware(MidFunc midFunc, string vendor = null, string commandPath = null, [CallerMemberName] string scenarioName = null)
        {
            return new Middleware.ScenarioBuilder(midFunc, commandPath: commandPath, name: scenarioName, vendor: vendor);
        }

        public static class Middleware
        {
            public interface IWithUsers : IGiven
            {
                IGiven WithUsers(params IAuthorization[] users);
            }

            public interface IGiven : IWhen
            {
                IWhen Given(params IHttpClientRequest[] given);
            }

            public interface IThen : IScenario
            {
                IThen ThenShouldThrow<TException>(Expression<Func<TException, bool>> isMatch = null) where TException : Exception;

                IThen ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
                    params Expression<Func<TResponse, bool>>[] assertions);
            }

            public interface IWhen : IThen
            {
                IThen When(IHttpClientRequest when);
            }

            internal class ScenarioBuilder : IWithUsers
            {
                private readonly string _name;
                private readonly AppFunc _appFunc;
                private readonly IDictionary<string, HttpClient> _httpClients;

                private readonly Func<Task> _runGiven;
                private readonly Func<Task> _runWhen;
                private Func<Task> _runThen;

                private readonly IList<IAssertion> _assertions;
                private IHttpClientRequest[] _given = new IHttpClientRequest[0];
                private IHttpClientRequest _when;
                private readonly IMessageExecutionSettings _messageExecutionSettings;
                private bool _passed;
                private readonly Stopwatch _timer;
                private object _results;
                private object[] _expect;

                public ScenarioBuilder(MidFunc midFunc, AppFunc next = null, string vendor = null, string commandPath = null, string name = null)
                {
                    _name = name;
                    next = next ?? (env =>
                    {
                        var context = new OwinContext(env);
                        context.Response.StatusCode = 404;
                        context.Response.ReasonPhrase = "Not Found";
                        return Task.FromResult(true);
                    });

                    _messageExecutionSettings = new CommandExecutionSettings(vendor ?? "vendor", path: commandPath ?? "commands");
                    
                    _appFunc = midFunc(next);
                    
                    _httpClients = new Dictionary<string, HttpClient>();

                    _runGiven = async () =>
                    {
                        foreach (IHttpClientRequest userCommand in _given)
                        {
                            await Send(userCommand);
                        }
                    };

                    _runWhen = () => Send(_when);

                    _runThen = async () =>
                    {
                        _expect = _assertions.ToArray();
                        var results = await Task.WhenAll(_assertions.Select(assertion => assertion.Run()));
                        _results = results;
                        var failed = (from result in results
                            from assertionResult in result
                            where !assertionResult.Passed
                            select assertionResult).ToList();

                        if (failed.Any())
                        {
                            throw new ScenarioException("One or more assertions failed:" 
                                + Environment.NewLine 
                                + failed.Aggregate(new StringBuilder(), (builder, assertionResult) => builder.Append(assertionResult).AppendLine()));
                        }
                    };

                    _assertions = new List<IAssertion>();
                    _timer = new Stopwatch();
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


                public IWhen Given(params IHttpClientRequest[] given)
                {
                    _given = given;

                    return this;
                }

                public IThen When(IHttpClientRequest when)
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

                public IThen ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
                    params Expression<Func<TResponse, bool>>[] assertions)
                {
                    _assertions.Add(new AsyncAssertion<TResponse>(() => Send(response), assertions));
                    
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
                        try
                        {
                            await _runGiven();
                        }
                        catch (Exception ex)
                        {
                            _results = new ScenarioException(ex.Message);

                            return this;
                        }
                        try
                        {
                            await _runWhen();
                        }
                        catch (Exception ex)
                        {
                            _results = ex;
                        }

                        await _runThen();

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

                private Task<TResponse> Send<TResponse>(IHttpClientRequest<TResponse> context)
                {
                    if (context == null) throw new ArgumentNullException("context");

                    HttpClient httpClient;

                    if (false == _httpClients.TryGetValue(context.AuthorizationId, out httpClient))
                    {
                        throw new InvalidOperationException(
                            "No Authorization for '{0}' was found. You must set it up using WithUsers.");
                    }

                    return context.Sender(httpClient, _messageExecutionSettings);
                }

                public static implicit operator ScenarioResult(ScenarioBuilder builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._expect, builder._results, builder._timer.Elapsed);
                }
            }

            private interface IAssertion
            {
                Task<IEnumerable<IAssertionResult>> Run();
            }

            private interface IAssertionResult
            {
                bool Passed { get; }
            }

            private class AssertionResult : IAssertionResult {
                private readonly bool _passed;
                private readonly Expression _expression;

                public AssertionResult(bool passed, Expression expression )
                {
                    _passed = passed;
                    _expression = expression;
                }

                public bool Passed { get { return _passed; } }

                public override string ToString()
                {
                    return _expression + ": " + (Passed ? "Passed" : "Failed");
                }
            }

            private class AsyncAssertion<TResponse> : IAssertion
            {
                private readonly Func<Task<TResponse>> _execute;
                private readonly Expression<Func<TResponse, bool>>[] _assertions;

                public AsyncAssertion(Func<Task<TResponse>> execute, params Expression<Func<TResponse, bool>>[] assertions)
                {
                    if(assertions == null)
                    {
                        throw new ArgumentNullException("assertions");
                    }
                    if(assertions.Length == 0)
                    {
                        throw new ArgumentException("Expected at least one assertion.", "assertions");
                    }

                    _execute = execute;
                    _assertions = assertions;
                }

                public async Task<IEnumerable<IAssertionResult>> Run()
                {
                    var response = await _execute();

                    return (from assertion in _assertions
                        let test = assertion.Compile()
                        select new AssertionResult(test(response), assertion))
                        .ToList();
                }

                public override string ToString()
                {
                    return _assertions
                        .Aggregate(new StringBuilder(), (builder, assertion) => builder.Append(assertion).AppendLine())
                        .ToString();
                }
            }

            public interface IHttpClientRequest : IHttpClientRequest<Unit>
            { }

            public interface IHttpClientRequest<TResponse>
            {
                Func<HttpClient, IMessageExecutionSettings, Task<TResponse>> Sender { get; }
                string AuthorizationId { get; }
                Guid Id { get; }
            }

            internal class HttpClientCommandRequest<TCommand> : IHttpClientRequest
                where TCommand : class
            {
                private readonly IAuthorization _authorization;
                private readonly TCommand _command;
                private readonly Guid _id;
                private readonly Func<HttpClient, IMessageExecutionSettings, Task<Unit>> _sender;

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
                    _sender = async (client, settings) =>
                    {
                        await client.ExecuteCommand(
                            command,
                            Id,
                            settings);
                        return Unit.Nothing;
                    };
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<Unit>> Sender
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

                public override string ToString()
                {
                    return _command + " (running as " + _authorization + ")";
                }
            }

            internal class HttpClientQueryRequest<TResponse> : IHttpClientRequest<TResponse>
            {
                private readonly IAuthorization _authorization;
                private readonly Guid _id;
                private readonly Func<HttpClient, IMessageExecutionSettings, Task<TResponse>> _sender;

                public HttpClientQueryRequest(
                    IAuthorization authorization,
                    Func<HttpClient, Task<TResponse>> query,
                    Guid? id = null)
                {
                    if(authorization == null)
                    {
                        throw new ArgumentNullException("authorization");
                    }

                    _authorization = authorization;
                    _id = id ?? Guid.NewGuid();
                    _sender = (client, _) => query(client);
                }

                public Func<HttpClient, IMessageExecutionSettings, Task<TResponse>> Sender
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
            }
            
            public class Unit
            {
                public static readonly Unit Nothing = new Unit();

                private Unit()
                {}
            }
        }
    }
}