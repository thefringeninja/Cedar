namespace Cedar.Handlers
{
    /// <summary>
    /// Provides a mechanism to fluently build a message handler pipeline.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message the handler will handle.</typeparam>
    public interface IHandlerBuilder<TMessage>
    {
        /// <summary>
        /// Pipes the message through handler middleware.
        /// </summary>
        /// <param name="pipe">The next handler middleware to invoke.</param>
        /// <returns>The <see cref="IHandlerBuilder{TMessage}"/> instance.</returns>
        IHandlerBuilder<TMessage> Pipe(Pipe<TMessage> pipe);

        /// <summary>
        /// Handles the message and is the last stage in a handler pipeline.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A <see cref="ICreateHandlerBuilder"/> to allow you to optionally define more pipelines.</returns>
        ICreateHandlerBuilder Handle(Handler<TMessage> handler);
    }
}