namespace Cedar.Handlers
{
    using System.Threading.Tasks;

    public static class HandlerBuilderExtensions
    {
        /// <summary>
        /// Handles the message and is the last stage in a handler pipeline.
        /// </summary>
        /// <param name="handlerBuilder">The <see cref="IHandlerBuilder{TMessage}"/>instance.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>A <see cref="ICreateHandlerBuilder"/> to allow you to optionally define more pipelines and handlers..</returns>
        public static ICreateHandlerBuilder Handle<TMessage>(this IHandlerBuilder<TMessage> handlerBuilder, HandlerSync<TMessage> handler)
        {
            return handlerBuilder.Handle((message, _) =>
            {
                handler(message);
                return Task.FromResult(0);
            });
        }
    }
}