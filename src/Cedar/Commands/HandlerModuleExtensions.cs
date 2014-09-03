namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Handlers;

    public static class HandlerModuleExtensions
    {
        [UsedImplicitly]
        public static Task DispatchCommand<TCommand>(
            this IEnumerable<HandlerModule> handlerModules,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            Guard.EnsureNotNull(handlerModules, "handlerModules");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(command, "command");

            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return handlerModules.DispatchSingle(commandMessage, cancellationToken);
        }
    }
}