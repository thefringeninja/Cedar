namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using FluentAssertions;
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
            return new Middleware.HttpClientCommandRequest<TCommand>(authorization.Id, command);
        }

        public static Middleware.IHttpClientRequest<TResponse> Queries<TResponse>(this IAuthorization authorization,
            Func<HttpClient, Task<TResponse>> query)
        {
            return new Middleware.HttpClientQueryRequest<TResponse>(authorization.Id, query);
        }

        public static Middleware.Given ForMiddleware(MidFunc midFunc)
        {
            return new Middleware.ScenarioBuilder(midFunc);
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

            public interface Then
            {
                void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception;

                Then ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
                    params Expression<Func<TResponse, bool>>[] assertions);

                TaskAwaiter GetAwaiter();
            }

            public interface When : Then
            {
                Then When(IHttpClientRequest when);
            }

            internal class ScenarioBuilder : WithUsers
            {
                private readonly AppFunc _appFunc;
                private readonly IDictionary<string, HttpClient> _httpClients;

                private Func<Task> _given = () => Task.FromResult(true);
                private Func<Task> _when;
                private readonly IList<IAssertion> _assertions;

                public ScenarioBuilder(MidFunc midFunc, AppFunc next = null)
                {
                    next = next ?? (env =>
                    {
                        var context = new OwinContext(env);
                        context.Response.StatusCode = 404;
                        context.Response.ReasonPhrase = "Not Found";
                        return Task.FromResult(true);
                    });
                    _appFunc = midFunc(next);
                    _httpClients = new Dictionary<string, HttpClient>();
                    _assertions = new List<IAssertion>();
                }

                public When Given(params IHttpClientRequest[] given)
                {
                    _given = async () =>
                    {
                        foreach (IHttpClientRequest userCommand in given)
                        {
                            await Send(userCommand);
                        }
                    };

                    return this;
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

                public void ThenShouldThrow<TException>(Func<TException, bool> equals = null)
                    where TException : Exception
                {
                    equals = equals ?? (_ => true);
                    Action then = () => _when();

                    _given();

                    then.ShouldThrow<TException>()
                        .And.Should().Match<TException>(ex => equals(ex));
                }

                public Then ThenShould<TResponse>(IHttpClientRequest<TResponse> response,
                    params Expression<Func<TResponse, bool>>[] assertions)
                {
                    _assertions.Add(new ExpressionAssertion<TResponse>(() => Send(response), assertions));
                    return this;
                }

                public TaskAwaiter GetAwaiter()
                {
                    return Task.Run(async () =>
                    {
                        await _given();
                        await _when();

                        await Task.WhenAll(_assertions.Select(x => x.IsTrue()));
                    }).GetAwaiter();
                }

                public Then When(IHttpClientRequest when)
                {
                    _when = () => Send(when);
                    return this;
                }

                private Task<TResponse> Send<TResponse>(IHttpClientRequest<TResponse> context)
                {
                    HttpClient httpClient;

                    if (false == _httpClients.TryGetValue(context.AuthorizationId, out httpClient))
                    {
                        throw new InvalidOperationException(
                            "No Authorization for '{0}' was found. You must set it up using WithUsers.");
                    }

                    return context.Sender(httpClient);
                }
            }

            private interface IAssertion
            {
                Task<bool> IsTrue();
            }

            private class ExpressionAssertion<TResponse> : IAssertion
            {
                private readonly Func<Task<TResponse>> _execute;
                private readonly Expression<Func<TResponse, bool>>[] _assertions;

                private Exception _thrownException;
                private bool? _isTrue;

                public ExpressionAssertion(Func<Task<TResponse>> execute,
                    params Expression<Func<TResponse, bool>>[] assertions)
                {
                    if (assertions == null) throw new ArgumentNullException("assertions");
                    if (assertions.Length == 0)
                        throw new ArgumentException("Expected at least one assertion.", "assertions");

                    _execute = execute;
                    _assertions = assertions;
                }

                public Exception ThrownException
                {
                    get { return _thrownException; }
                }

                public async Task<bool> IsTrue()
                {
                    if (!_isTrue.HasValue)
                    {
                        try
                        {
                            var response = await _execute();

                            _isTrue = _assertions.Select(a => a.Compile())
                                .All(assertion => assertion(response));
                        }
                        catch (Exception ex)
                        {
                            _thrownException = ex;
                            _isTrue = false;
                        }
                    }

                    return _isTrue.Value;
                }
            }

            public interface IHttpClientRequest : IHttpClientRequest<Unit>
            { }

            public interface IHttpClientRequest<TResponse>
            {
                Func<HttpClient, Task<TResponse>> Sender { get; }
                string AuthorizationId { get; }
                Guid Id { get; }
            }

            internal class HttpClientCommandRequest<TCommand> : IHttpClientRequest
            {
                public HttpClientCommandRequest(string authorizationId, TCommand command, Guid? commandId = null)
                {
                    var settings = new CommandExecutionSettings("vendor");
                    AuthorizationId = authorizationId;
                    Command = command;
                    Id = commandId ?? Guid.NewGuid();

                    Sender = client => client.ExecuteCommand(
                        command,
                        Id,
                        settings)
                        .ContinueWith(_ => Unit.Nothing);
                }

                public TCommand Command { get; private set; }
                public Func<HttpClient, Task<Unit>> Sender { get; private set; }
                public string AuthorizationId { get; private set; }

                public Guid Id { get; private set; }
            }

            internal class HttpClientQueryRequest<TResponse> : IHttpClientRequest<TResponse>
            {
                public HttpClientQueryRequest(string authorizationId, Func<HttpClient, Task<TResponse>> query,
                    Guid? id = null)
                {
                    AuthorizationId = authorizationId;

                    Id = id ?? Guid.NewGuid();

                    Sender = query;
                }

                public Func<HttpClient, Task<TResponse>> Sender { get; private set; }
                public string AuthorizationId { get; private set; }
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