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
    using Microsoft.Owin;

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

        public static Middleware.WithUsers ForMiddleware(MidFunc midFunc, string commandPath = null, [CallerMemberName] string scenarioName = null)
        {
            return new Middleware.ScenarioBuilder(midFunc, commandPath: commandPath, name: scenarioName);
        }

        public static class Middleware
        {
            public interface WithUsers : Given
            {
                Given WithUsers(params IAuthorization[] users);
            }

            public interface Given : When
            {
                When Given(params IHttpClientRequest[] given);
            }

            public interface Then : IScenario
            {
                Then ThenShouldThrow<TException>(Func<TException, bool> isMatch = null) where TException : Exception;

                Then ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
                    params Expression<Func<TResponse, bool>>[] assertions);
            }

            public interface When : Then
            {
                Then When(IHttpClientRequest when);
            }

            internal class ScenarioBuilder : WithUsers
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
                private Exception _occurredException;
                private readonly ICommandExecutionSettings _commandExecutionSettings;
                private bool _passed = false;
                private readonly Stopwatch _timer;

                public ScenarioBuilder(MidFunc midFunc, AppFunc next = null, string commandPath = null, string name = null)
                {
                    _name = name;
                    next = next ?? (env =>
                    {
                        var context = new OwinContext(env);
                        context.Response.StatusCode = 404;
                        context.Response.ReasonPhrase = "Not Found";
                        return Task.FromResult(true);
                    });

                    _commandExecutionSettings = new CommandExecutionSettings("vendor", path: commandPath);
                    
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
                        var results = await Task.WhenAll(_assertions.Select(assertion => assertion.Run()));

                        var failed = (from result in results
                            from assertionResult in result
                            where !assertionResult.Passed
                            select assertionResult).ToList();

                        if (failed.Any())
                        {
                            throw new ScenarioException(this, "One or more assertions failed:" 
                                + Environment.NewLine 
                                + failed.Aggregate(new StringBuilder(), (builder, assertionResult) => builder.Append(assertionResult).AppendLine()));
                        }
                    };

                    _assertions = new List<IAssertion>();
                    _timer = new Stopwatch();
                }

                public Given WithUsers(params IAuthorization[] users)
                {
                    foreach (IAuthorization user in users)
                    {
                        var httpClient =
                            new HttpClient(
                                new OwinHttpMessageHandler(_appFunc))
                            {
                                BaseAddress = new Uri("http://localhost"),
                                DefaultRequestHeaders =
                                {
                                    Authorization = user.AuthorizationHeader
                                }
                            };
                        _httpClients.Add(user.Id, httpClient);
                    }

                    return this;
                }


                public When Given(params IHttpClientRequest[] given)
                {
                    _given = given;

                    return this;
                }

                public Then When(IHttpClientRequest when)
                {
                    _when = when;
                    
                    return this;
                }

                public Then ThenShouldThrow<TException>(Func<TException, bool> isMatch = null)
                    where TException : Exception
                {
                    isMatch = isMatch ?? (_ => true);

                    _runThen = () =>
                    {
                        ((ScenarioResult) this).AssertExceptionMatches(_occurredException, isMatch);
                        return Task.FromResult(true);
                    };

                    return this;
                }

                public Then ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
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
                    
                    await _runGiven();

                    try
                    {
                        await _runWhen();
                    }
                    catch (AggregateException ex)
                    {
                        _occurredException = ex;
                    }

                    await _runThen();

                    _passed = true;
                    
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

                    return context.Sender(httpClient, _commandExecutionSettings);
                }

                public static implicit operator ScenarioResult(ScenarioBuilder builder)
                {
                    return new ScenarioResult(builder._name, builder._passed, builder._given, builder._when, builder._assertions, duration: builder._timer.Elapsed, occurredException: builder._occurredException);
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

                public AsyncAssertion(Func<Task<TResponse>> execute,
                    params Expression<Func<TResponse, bool>>[] assertions)
                {
                    if (assertions == null) throw new ArgumentNullException("assertions");
                    if (assertions.Length == 0)
                        throw new ArgumentException("Expected at least one assertion.", "assertions");

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
                    return _assertions.Aggregate(new StringBuilder(),
                        (builder, assertion) => builder.Append(assertion).AppendLine()).ToString();
                }
            }

            public interface IHttpClientRequest : IHttpClientRequest<Unit>
            { }

            public interface IHttpClientRequest<TResponse>
            {
                Func<HttpClient, ICommandExecutionSettings, Task<TResponse>> Sender { get; }
                string AuthorizationId { get; }
                Guid Id { get; }
            }

            internal class HttpClientCommandRequest<TCommand> : IHttpClientRequest
                where TCommand: class 
            {
                private readonly IAuthorization _authorization;

                public HttpClientCommandRequest(IAuthorization authorization, TCommand command, Guid? commandId = null)
                {
                    if (authorization == null) throw new ArgumentNullException("authorization");
                    if (command == null) throw new ArgumentNullException("command");

                    _authorization = authorization;
                    Command = command;
                    Id = commandId ?? Guid.NewGuid();

                    Sender = async (client, settings) =>
                    {
                        await client.ExecuteCommand(
                            command,
                            Id,
                            settings);
                        return Unit.Nothing;
                    };
                }

                public TCommand Command { get; private set; }
                public Func<HttpClient, ICommandExecutionSettings, Task<Unit>> Sender { get; private set; }
                public string AuthorizationId { get { return _authorization.Id; }}
                public Guid Id { get; private set; }

                public override string ToString()
                {
                    return Command + " (running as " + _authorization + ")";
                }
            }

            internal class HttpClientQueryRequest<TResponse> : IHttpClientRequest<TResponse>
            {
                private readonly IAuthorization _authorization;

                public HttpClientQueryRequest(IAuthorization authorization, Func<HttpClient, Task<TResponse>> query,
                    Guid? id = null)
                {
                    if (authorization == null) throw new ArgumentNullException("authorization");

                    _authorization = authorization;
                    Id = id ?? Guid.NewGuid();
                    Sender = (client, _) => query(client);
                }

                public Func<HttpClient, ICommandExecutionSettings, Task<TResponse>> Sender { get; private set; }
                public string AuthorizationId { get { return _authorization.Id; } }
                public Guid Id { get; private set; }
            }
            
            public class Unit
            {
                public static readonly Unit Nothing = new Unit();

                private Unit()
                {}

                public static Task<Unit> Return
                {
                    get { return Task.FromResult(Nothing); }
                }
            }
        }
    }
}