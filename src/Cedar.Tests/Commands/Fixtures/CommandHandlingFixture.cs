namespace Cedar.Commands.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Commands.Client;
    using Cedar.TypeResolution;
    using Microsoft.Owin;

    public class CommandHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;
        private readonly MessageExecutionSettings _messageExecutionSettings;

        public CommandHandlingFixture()
        {
            const string vendor = "vendor";

            var handlerModule = new TestHandlerModule();
           
            var commandTypeFromContentTypeResolver = new DefaultRequestTypeResolver(
                vendor,
                handlerModule);
            var options = new DefaultHandlerSettings(handlerModule, commandTypeFromContentTypeResolver);
            _midFunc = CommandHandlingMiddleware.HandleCommands(options);
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