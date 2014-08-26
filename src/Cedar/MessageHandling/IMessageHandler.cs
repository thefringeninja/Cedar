namespace Cedar.MessageHandling
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandler<TMessage>
        where TMessage : class
    {
        Task Project(TMessage message, CancellationToken cancellationToken);
    }
}