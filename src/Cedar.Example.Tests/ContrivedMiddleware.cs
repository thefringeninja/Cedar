namespace Cedar.Example.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands;
    using Cedar.ContentNegotiation;
    using Cedar.Handlers;
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

        class QueryResult
        {
            public string Value { get; set; }
        }
        [Fact]
        public async Task Passes()
        {
            var user = Authorization.Basic("user", "password");

            await Scenario.ForMiddleware(MySystem, commandPath:"/commands")
                .WithUsers(user)
                .Given(user.Does(new Something {Value = "this"}))
                .When(user.Does(new SomethingElse {Value = "that"}))
                .ThenShould(user.Queries(client => client.AsJson<QueryResult>(new Uri("http://localhost/results"))),
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
                .ThenShould(user.Queries(client => client.AsJson<QueryResult>(new Uri("http://localhost/results"))),
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
                .ThenShould(user.Queries(client => client.AsJson<QueryResult>(new Uri("http://localhost/results"))),
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
                .ThenShould(user.Queries(client => client.AsJson<QueryResult>(new Uri("http://localhost/results"))),
                    result => result.Value == "this");
        }

        static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> MySystem
        {
            get
            {
                var result = new QueryResult();
                var module = new HandlerModule();

                module.For<CommandMessage<Something>>()
                    .Handle(async (message, _) => result = new QueryResult {Value = message.Command.Value});
                module.For<CommandMessage<SomethingElse>>()
                    .Handle(async (message, _) => result = new QueryResult { Value = message.Command.Value });

                var commands = CommandHandlingMiddleware.HandleCommands(
                    new DefaultHandlerSettings(
                        module,
                        new DefaultContentTypeMapper(
                            "vendor",
                            new[]
                            {
                                typeof (Something), typeof (SomethingElse)
                            })));

                var queries = new MidFunc(_ => env =>
                {
                    var context = new OwinContext(env);

                    return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                });

                var router = new MidFunc(
                    next => 
                    env =>
                    {
                        var context = new OwinContext(env);

                        MidFunc selected = app => app;

                        if (context.Request.Path.StartsWithSegments(new PathString("/results")))
                            selected = queries;
                        else if (context.Request.Path.StartsWithSegments(new PathString("/commands")))
                        {
                            context.Request.Path = new PathString(context.Request.Path.Value.Remove(0, "/commands".Length));
                            selected = commands;
                        }

                        return selected(next)(env);
                    });
                return router;
            }
        }
    }
}