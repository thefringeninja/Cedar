namespace Cedar.Example.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;
    using Cedar.Queries;
    using Cedar.Queries.Client;
    using Cedar.Testing;
    using Microsoft.Owin;
    using Newtonsoft.Json;
    using Xunit;

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

    public static class HttpClientExtensions
    {
        public static async Task<TResponse> AsJson<TResponse>(this HttpClient client, Uri requestUri)
        {
            var response = await client.GetAsync(requestUri);

            return JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
        }
    }

    public class ContrivedMiddleware
    {
        private readonly QueryExecutionSettings _queryExecutionSettings;

        public ContrivedMiddleware()
        {
            _queryExecutionSettings = new QueryExecutionSettings("vendor");
        }

        class Something {
            public string Value { get; set; }
            public override string ToString()
            {
                return "Something = " + Value;
            }
        }

        class SomethingElse
        {
            public string Value { get; set; }
            public override string ToString()
            {
                return "Something else = " + Value;
            }
        }
        class Query { }
        class QueryResult
        {
            public string Value { get; set; }
        }

        class QueryHandler : IQueryHandler<Query, QueryResult>
        {
            private readonly QueryResult _result;

            public QueryHandler(QueryResult result)
            {
                _result = result;
            }

            public Task<QueryResult> PerformQuery(Query input)
            {
                return Task.FromResult(_result);
            }
        }
        [Fact]
        public async Task Passes()
        {
            var user = Authorization.Basic("user", "password");

            await Scenario.ForMiddleware(MySystem, commandPath:"/commands")
                .WithUsers(user)
                .Given(user.Does(new Something {Value = "this"}))
                .When(user.Does(new SomethingElse {Value = "that"}))
                .ThenShould(user.Queries(client => client.ExecuteQuery<Query, QueryResult>(new Query(), Guid.NewGuid(), _queryExecutionSettings)),
                    result => result.Value == "that");
        }

        [Fact]
        public async Task<ScenarioResult> AlsoPasses()
        {
            var user = Authorization.Basic("user", "password");

            return await Scenario.ForMiddleware(MySystem, commandPath: "/commands")
                .WithUsers(user)
                .Given(user.Does(new Something() { Value = "this" }))
                .When(user.Does(new SomethingElse() { Value = "that" }))
                .ThenShould(user.Queries(client => client.ExecuteQuery<Query, QueryResult>(new Query(), Guid.NewGuid(), _queryExecutionSettings)),
                    result => result.Value == "that");
        }

        [Fact]
        public async Task DoesNotPass()
        {
            var user = Authorization.Basic("user", "password");

            await Scenario.ForMiddleware(MySystem, commandPath: "/commands")
                .WithUsers(user)
                .Given(user.Does(new Something { Value = "this" }))
                .When(user.Does(new SomethingElse { Value = "that" }))
                .ThenShould(user.Queries(client => client.ExecuteQuery<Query, QueryResult>(new Query(), Guid.NewGuid(), _queryExecutionSettings)),
                    result => result.Value == "this");
        }

        [Fact]
        public async Task<ScenarioResult> AlsoDoesNotPass()
        {
            var user = Authorization.Basic("user", "password");

            return await Scenario.ForMiddleware(MySystem, commandPath: "/commands")
                .WithUsers(user)
                .Given(user.Does(new Something() { Value = "this" }))
                .When(user.Does(new SomethingElse() { Value = "that" }))
                .ThenShould(user.Queries(client => client.ExecuteQuery<Query, QueryResult>(new Query(), Guid.NewGuid(), _queryExecutionSettings)),
                    result => result.Value == "this");
        }

        static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> MySystem
        {
            get
            {
                var result = new QueryResult();
                
                var commandModule = new HandlerModule();
                commandModule.For<CommandMessage<Something>>()
                    .Handle(async (message, _) => result.Value = message.Command.Value);
                commandModule.For<CommandMessage<SomethingElse>>()
                    .Handle(async (message, _) => result.Value = message.Command.Value);

                var commands = CommandHandlingMiddleware.HandleCommands(
                    new DefaultHandlerSettings(
                        commandModule,
                        new DefaultContentTypeMapper(
                            "vendor",
                            new[]
                            {
                                typeof (Something), typeof (SomethingElse)
                            })));

                var queryModule = new QueryHandlerModule();
                queryModule.For(new QueryHandler(result));

                var queries = QueryHandlingMiddleware.HandleQueries(new DefaultHandlerSettings(
                    queryModule,
                    new DefaultContentTypeMapper(
                        "vendor",
                        new[]
                        {
                            typeof(QueryResult), typeof(Query)
                        })));


                return app => commands(queries(app));
            }
        }
    }
}