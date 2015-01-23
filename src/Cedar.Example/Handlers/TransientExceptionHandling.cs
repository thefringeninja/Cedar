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
            handlerModule
                .For<Event>()
                .Handle((message, ct) =>
                {
                    return Policy.Handle<Exception>()
                        .RetryForeverAsync(ex => { /* log message */})
                        .ExecuteAsync(() => /* some async op */ Task.FromResult(0));
                });
        }

        public class Event
        {}
    }
}