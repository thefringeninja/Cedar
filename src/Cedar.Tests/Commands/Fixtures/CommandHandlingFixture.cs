namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.ContentNegotiation;
    using Microsoft.Owin;

    public class CommandHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;
        private readonly CommandExecutionSettings _commandExecutionSettings;

        public CommandHandlingFixture()
        {
            const string vendor = "vendor";

            var handlerModule = new TestHandlerModule();
           
            var commandTypeFromContentTypeResolver = new DefaultContentTypeMapper(
                vendor,
                new[]
                {
                    typeof (TestCommand),
                    typeof (TestCommandWithoutHandler),
                    typeof (TestCommandWhoseHandlerThrows)
                });
            var options = new DefaultHandlerSettings(handlerModule, commandTypeFromContentTypeResolver);
            _midFunc = CommandHandlingMiddleware.HandleCommands(options);
            _commandExecutionSettings = new CommandExecutionSettings(vendor);
        }

        public CommandExecutionSettings CommandExecutionSettings
        {
            get { return _commandExecutionSettings; }
        }

        public HttpClient CreateHttpClient()
        {
            return CreateHttpClient(env =>
            {
                var context = new OwinContext(env);
                context.Response.StatusCode = 404;
                context.Response.ReasonPhrase = "Not Found";
                return Task.FromResult(0);
            });
        }

        public HttpClient CreateHttpClient(Func<IDictionary<string, object>, Task> next)
        {
            var appFunc = _midFunc(next);
            return new HttpClient(new OwinHttpMessageHandler(appFunc))
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}