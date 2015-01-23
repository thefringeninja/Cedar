namespace Cedar.Example.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Handlers;
    using Polly;

    public class TransientExceptionHandling
    {
        public TransientExceptionHandling()
        {
            var handlerModule = new HandlerModule();

            // 1. Explicitly defining a retry policy in a handler
            handlerModule
                .For<Event>()
                .Handle((message, ct) =>
                {
                    return Policy.Handle<Exception>()
                        .RetryForeverAsync(ex => { /* log message */})
                        .ExecuteAsync(() => /* some async op */ Task.FromResult(0));
                });

            // 2. Re-using a policy via extension methods.
            handlerModule
                .For<Event>()
                .ConnectionTimeoutRetry()
                .Handle((message, ct) => Task.FromResult(0));
        }

        public class Event
        {}

    }

    public static class HandlerBuilderExtensions
    {
        public static IHandlerBuilder<T> ConnectionTimeoutRetry<T>(
            this IHandlerBuilder<T> handlerBuilder)
            where T : class
        {
            return handlerBuilder.Pipe(next => (message, ct) =>
            {
                return Policy.Handle<TimeoutException>()
                    .RetryForeverAsync(ex =>
                    {
                        /* log message, system warning etc */
                    })
                    .ExecuteAsync(() => next(message, ct));
            });
        }
    }
}