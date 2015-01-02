namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.LibOwin;

    public class CommandHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;
        private readonly MessageExecutionSettings _messageExecutionSettings;

        public CommandHandlingFixture()
        {
            const string vendor = "vendor";

            var handlerModule = new TestHandlerModule();

            var handlerResolver = new CommandHandlerResolver(handlerModule);

            var typeResolver = CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix(handlerResolver.KnownCommandTypes);

            var commandHandlingSettings = new CommandHandlingSettings(handlerResolver, typeResolver);

            _midFunc = CommandHandlingMiddleware.HandleCommands(commandHandlingSettings);
            _messageExecutionSettings = new CommandExecutionSettings(vendor);
        }

        public MessageExecutionSettings MessageExecutionSettings
        {
            get { return _messageExecutionSettings; }
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