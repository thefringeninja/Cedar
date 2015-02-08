namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using CuttingEdge.Conditions;

    /// <summary>
    ///     A set of extensions around <see cref="IHandlerResolver" />
    ///     that assist in dispatching a message.
    /// </summary>
    public static class HandlerResolverExtensions
    {
        /// <summary>
        ///     Resolves all handlers that can handle the message type and dispatches the message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="handlerResolver">The handler resolver.</param>
        /// <param name="message">The message to be dispatched.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task" />Task the represents the operation.</returns>
        public static async Task Dispatch<TMessage>(
            [NotNull] this IHandlerResolver handlerResolver,
            TMessage message,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(message, "message").IsNotNull();

            IEnumerable<Handler<TMessage>> handlers = handlerResolver.ResolveAll<TMessage>();
            foreach (var handler in handlers)
            {
                await handler(message, cancellationToken);
            }
        }
    }
}