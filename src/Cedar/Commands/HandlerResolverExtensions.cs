namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;

    public static class HandlerResolverExtensions
    {
        [UsedImplicitly]
        public static Task DispatchCommand<TCommand>(
            this IHandlerResolver handlerResolver,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            Guard.EnsureNotNull(handlerResolver, "handlerResolver");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(command, "command");

            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return handlerResolver.DispatchSingle(commandMessage, cancellationToken);
        }
    }
}