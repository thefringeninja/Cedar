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

    public static partial class Scenario
    {
        public delegate Task AppFunc(IDictionary<string, object> env);

        public delegate AppFunc MidFunc(AppFunc next);

        public static IHttpClientRequest Does<TCommand>(this IAuthorization authorization, TCommand command)
            where TCommand : class
        {
            return new HttpClientCommandRequest<TCommand>(authorization.Id, command);
        }

        public static IHttpClientRequest<TResponse> Queries<TResponse>(this IAuthorization authorization,
            Func<HttpClient, Task<TResponse>> query)
        {
            return new HttpClientQueryRequest<TResponse>(authorization.Id, query);
        }

        public static ISubcutaneousGiven ForSystem(MidFunc midFunc)
        {
            return new SubcutaneousScenario(midFunc);
        }

        private class HttpClientCommandRequest<TCommand> : IHttpClientRequest
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

        private class HttpClientQueryRequest<TResponse> : IHttpClientRequest<TResponse>
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

        public interface IHttpClientRequest : IHttpClientRequest<Unit>
        {}

        public interface IHttpClientRequest<TResponse>
        {
            Func<HttpClient, Task<TResponse>> Sender { get; }
            string AuthorizationId { get; }
            Guid Id { get; }
        }

        public interface ISubcutaneousGiven : ISubcutaneousWhen
        {
            ISubcutaneousWhen Given(params IHttpClientRequest[] given);
        }

        public interface ISubcutaneousThen
        {
            void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception;
            ISubcutaneousThen ThenShould<TResponse>(IHttpClientRequest<TResponse> response, params Expression<Func<TResponse, bool>>[] assertions);
            TaskAwaiter GetAwaiter();
        }

        public interface ISubcutaneousWhen : ISubcutaneousThen
        {
            ISubcutaneousThen When(IHttpClientRequest when);
        }

        public interface ISubcutaneousWithUsers : ISubcutaneousGiven
        {
            ISubcutaneousGiven WithUsers(params IAuthorization[] users);
        }

        private interface IAssertion
        {
            Task<bool> IsTrue();
        }

        class ExpressionAssertion<TResponse> : IAssertion
        {
            private readonly Func<Task<TResponse>> _execute;
            private readonly Expression<Func<TResponse, bool>>[] _assertions;
			
            private Exception _thrownException;
            private bool? _isTrue;

            public ExpressionAssertion(Func<Task<TResponse>> execute, params Expression<Func<TResponse, bool>>[] assertions)
            {
				if (assertions == null) throw new ArgumentNullException("assertions");
				if (assertions.Length == 0) throw new ArgumentException("Expected at least one assertion.", "assertions");

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

        private class SubcutaneousScenario : ISubcutaneousWithUsers
        {
            private readonly AppFunc _appFunc;
            private readonly IDictionary<string, HttpClient> _httpClients;

            private Func<Task> _given = () => Task.FromResult(true);
            private Func<Task> _when;
            private readonly IList<IAssertion> _assertions;
            public SubcutaneousScenario(MidFunc midFunc, AppFunc next = null)
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

            public ISubcutaneousWhen Given(params IHttpClientRequest[] given)
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

            public ISubcutaneousGiven WithUsers(params IAuthorization[] users)
            {
                foreach (IAuthorization user in users)
                {
                    var httpClient =
                        new HttpClient(new OwinHttpMessageHandler(new Func<IDictionary<string, object>, Task>(_appFunc)))
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

            public void ThenShouldThrow<TException>(Func<TException, bool> equals = null) where TException : Exception
            {
                equals = equals ?? (_ => true);
                Action then = () => _when();

                _given();

                then.ShouldThrow<TException>()
                    .And.Should().Match<TException>(ex => equals(ex));
            }

            public ISubcutaneousThen ThenShould<TResponse>(IHttpClientRequest<TResponse> response, params Expression<Func<TResponse, bool>>[] assertions)
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

            public ISubcutaneousThen When(IHttpClientRequest when)
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