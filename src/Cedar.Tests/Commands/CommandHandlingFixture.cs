namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public class CommandHandlingFixture
    {
        private readonly Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>> _midFunc;

        public CommandHandlingFixture()
        {
            var module = new CommandHandlerModule();
            module.For<TestCommand>()
                .Handle((_, __) => Task.FromResult(0));
            module.For<TestCommandWhoseHandlerThrowsStandardException>()
                .Handle((_, __) => { throw new InvalidOperationException(); });
            module.For<TestCommandWhoseHandlerThrowProblemDetailsException>()
                .Handle((_, __) =>
                {
                    var problemDetails = new HttpProblemDetails(HttpStatusCode.BadRequest)
                    {
                        Type = new Uri("http://localhost/type"),
                        Detail = "You done goof'd",
                        Instance = new Uri("http://localhost/errors/1"),
                        Title = "Jimmies Ruslted"
                    };
                    throw new HttpProblemDetailsException(problemDetails);
                });
            module.For<TestCommandWhoseHandlerThrowsExceptionThatIsConvertedToProblemDetails>()
               .Handle((_, __) => { throw new ApplicationException("Custom application exception"); });

            var handlerResolver = new CommandHandlerResolver(module);
            var commandHandlingSettings = new CommandHandlingSettings(handlerResolver)
            {
                CreateProblemDetails = CreateProblemDetails
            };

            _midFunc = CommandHandlingMiddleware.HandleCommands(commandHandlingSettings);
        }

        private static HttpProblemDetails CreateProblemDetails(Exception ex)
        {
            var applicationExcepion = ex as ApplicationException;
            if(applicationExcepion != null)
            {
                return new HttpProblemDetails(HttpStatusCode.BadRequest)
                {
                    Title = "Application Exception",
                    Detail = applicationExcepion.Message
                };
            }
            return null;
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

    public class TestCommand { }

    public class TestCommandWithoutHandler { }

    public class TestCommandWhoseHandlerThrowsStandardException { }

    public class TestCommandWhoseHandlerThrowProblemDetailsException { }

    public class TestCommandWhoseHandlerThrowsExceptionThatIsConvertedToProblemDetails { }

}