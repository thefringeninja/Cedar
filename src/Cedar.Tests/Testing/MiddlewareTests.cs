namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.Commands.Client;
    using Cedar.Handlers;
    using Cedar.Queries;
    using Cedar.Queries.Client;
    using Cedar.TypeResolution;
    using Newtonsoft.Json;
    using Xunit;

    public static class HttpClientExtensions
    {
        public static async Task<TResponse> AsJson<TResponse>(this HttpClient client, Uri requestUri)
        {
            var response = await client.GetAsync(requestUri);

            return JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
        }
    }

    public class MiddlewareTests
    {
        private readonly QueryExecutionSettings _queryExecutionSettings;
        private readonly CommandExecutionSettings _commandExecutionSettings;

        public MiddlewareTests()
        {
            _queryExecutionSettings = new QueryExecutionSettings("vendor");
            _commandExecutionSettings = new CommandExecutionSettings("vendor");
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

        class SomethingExploding
        {
            public override string ToString()
            {
                return "Don't run me.";
            }
        }

        class Query { }
        class QueryResult
        {
            public string Value { get; set; }
        }

            
        [Fact]
        public async Task a_passing_middleware_scenario_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, _commandExecutionSettings, _queryExecutionSettings)
                .WithUsers(user)
                .Given(user.Does(new Something {Value = "this"}))
                .When(user.Does(new SomethingElse {Value = "that"}))
                .Then(user.Queries<Query, QueryResult>(new Query()))
                .ShouldEqual(new QueryResult { Value = "that" });

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_passing_middleware_scenario_within_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, _commandExecutionSettings, _queryExecutionSettings)
                .WithUsers(user)
                .Given(user.Does(new Something { Value = "this" }).ProcessedWithin(200))
                .When(user.Does(new SomethingElse { Value = "that" }).ProcessedWithin(200))
                .Then(user.Queries<Query, QueryResult>(new Query()))
                .ShouldEqual(new QueryResult { Value = "that" });

            Assert.True(result.Passed);
        }

        [Fact]
        public async Task a_failing_middleware_scenario_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, _commandExecutionSettings, _queryExecutionSettings)
                .WithUsers(user)
                .Given(user.Does(new Something { Value = "this" }))
                .When(user.Does(new SomethingElse { Value = "that" }))
                .Then(user.Queries<Query, QueryResult>(new Query()))
                .ShouldEqual(new QueryResult { Value = "this" });

            Assert.False(result.Passed);
        }


        [Fact]
        public async Task a_middleware_scenario_throwing_expected_exception_in_given_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, _commandExecutionSettings, _queryExecutionSettings)
                .WithUsers(user)
                .Given(user.Does(new SomethingExploding()))
                .When(user.Does(new Something { Value = "this" }))
                .Then(user.Queries<Query, QueryResult>(new Query()))
                .ShouldEqual(new QueryResult { Value = "this" });

            Assert.False(result.Passed);
            Assert.IsType<ScenarioException>(result.Results);
        }
        /*
        [Fact]
        public async Task a_middleware_scenario_throwing_expected_exception_in_when_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, commandPath: "/commands")
                .WithUsers(user)
                .Given(user.Does(new Something { Value = "this" }))
                .When(user.Does(new SomethingExploding()))
                .ThenShould(user.Queries(client => client.ExecuteQuery<Query, QueryResult>(new Query(), Guid.NewGuid(), _queryExecutionSettings)),
                    q => q.Value == "this");

            Assert.False(result.Passed);
            Assert.IsType<Scenario.ScenarioException>(result.Results);
        }*/

        [Fact]
        public async Task a_middleware_scenario_throwing_an_expected_exception_should()
        {
            var user = Authorization.Basic("user", "password");

            var result = await Scenario.ForMiddleware(MySystem, _commandExecutionSettings, _queryExecutionSettings)
                .WithUsers(user)
                .Given(user.Does(new Something {Value = "this"}))
                .When(user.Does(new SomethingExploding()))
                .ThenShouldThrow<Exception>();

            Assert.True(result.Passed);
        }

        static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> MySystem
        {
            get
            {
                var result = new QueryResult();
                
                var commandModule = new HandlerModule();
                commandModule.For<CommandMessage<Something>>()
                    .Handle(message => result.Value = message.Command.Value);
                commandModule.For<CommandMessage<SomethingElse>>()
                    .Handle(message => result.Value = message.Command.Value);

                var commands = CommandHandlingMiddleware.HandleCommands(
                    new DefaultHandlerConfiguration(
                        commandModule,
                        new DefaultRequestTypeResolver(
                            "vendor",
                            new[]
                            {
                                typeof (Something), typeof (SomethingElse)
                            })));

                var queryModule = new QueryHandlerModule();
                queryModule.For<Query, QueryResult>()
                    .HandleQuery((_, __) => Task.FromResult(result));

                var queries = QueryHandlingMiddleware.HandleQueries(new DefaultHandlerConfiguration(
                    queryModule,
                    new DefaultRequestTypeResolver(
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