namespace Cedar.Example.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Security.Claims;
    using Cedar.Commands;
    using Cedar.Handlers;

    internal static class IHandlerBuilderExtensions
    {
        internal static IHandlerBuilder<TMessage> LogExceptions<TMessage>(this IHandlerBuilder<TMessage> handleBuilder) where TMessage : class
        {
            return handleBuilder.Pipe(next => async (message, ct) =>
            {
                try
                {
                    await next(message, ct);
                }
                catch (Exception)
                {
                    //logger.ErrorException("Exception occured while blah blah", ex);
                    throw;
                }
            });
        }

        internal static IHandlerBuilder<TMessage> PerformanceCounter<TMessage>(this IHandlerBuilder<TMessage> handleBuilder) where TMessage : class
        {
            return handleBuilder.Pipe(next => async (message, ct) =>
            {
                var startNew = Stopwatch.StartNew();
                try
                {
                    await next(message, ct);
                }
                finally
                {
                    var elapsed = startNew.ElapsedMilliseconds;
                    // output perf counter
                }
            });
        }

        internal static IHandlerBuilder<CommandMessage<TMessage>> RequiresClaim<TMessage>(
            this IHandlerBuilder<CommandMessage<TMessage>> handlerBuilder,
            Func<Claim, bool> claimPredicate)
        {
            return handlerBuilder;
        }

        internal static IHandlerBuilder<CommandMessage<TMessage>> DenyAnonymous<TMessage>(
            this IHandlerBuilder<CommandMessage<TMessage>> handlerBuilder)
        {
            return handlerBuilder.Pipe(next => (message, ct) =>
            {
                if (message.User.Identity.IsAuthenticated)
                {
                    throw new InvalidOperationException("Not authenticated");
                }
                return next(message, ct);
            });
        }

        internal static IHandlerBuilder<CommandMessage<TMessage>> ValidateWith<TMessage>(
            this IHandlerBuilder<CommandMessage<TMessage>> handlerBuilder, object validator)
        {
            return handlerBuilder;
        }

        internal static IHandlerBuilder<CommandMessage<TMessage>> LogAuthorizeAndValidate<TMessage, TValidator>(
            this IHandlerBuilder<CommandMessage<TMessage>> handleBuilder, TValidator validator)
        {
            return handleBuilder
                .LogExceptions()
                .DenyAnonymous()
                .ValidateWith(validator);
        }
    }
}