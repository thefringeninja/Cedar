namespace Cedar.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a generalized mechanism for handling messages.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message that is handled.</typeparam>
    public interface IHandler<TMessage>
        where TMessage : class
    {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the handle operation.</returns>
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}