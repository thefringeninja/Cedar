namespace Cedar.ContentNegotiation
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using Cedar.Commands;
    using Cedar.Handlers;

    public static class HandlerModulesDispatchCommand
    {
        [UsedImplicitly]
        public static Task DispatchCommand<TCommand>(
            this IEnumerable<IHandlerResolver> handlerModules,
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

    public static class HandlerModuleDispatchCommand
    {
        // This in a seperate class because of a generic limitation in reflection
        // http://blogs.msdn.com/b/yirutang/archive/2005/09/14/466280.aspx

        /// <summary>
        /// Dispatches the command.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="handlerModule">The handler module.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="requstUser">The requst user.</param>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represesnts the operation.</returns>
        [UsedImplicitly]
        public static Task DispatchCommand<TCommand>(
            this IHandlerResolver handlerModule,
            Guid commandId,
            ClaimsPrincipal requstUser,
            TCommand command,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            Guard.EnsureNotNull(handlerModule, "handlerModules");
            Guard.EnsureNotNull(requstUser, "requstUser");
            Guard.EnsureNotNull(command, "command");

            var commandMessage = new CommandMessage<TCommand>(commandId, requstUser, command);
            return handlerModule.DispatchSingle(commandMessage, cancellationToken);
        }
    }
}