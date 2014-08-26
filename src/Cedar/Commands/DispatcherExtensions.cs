namespace Cedar.Commands
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;

    public static class DispatcherExtensions
    {
        [UsedImplicitly]
        public static Task<int> DispatchCommand<TCommand>(
            this IDispatcher dispatcher,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return dispatcher.Dispatch(commandMessage, cancellationToken);
        }
    }
}