namespace Cedar.MessageHandling
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandler<TMessage>
        where TMessage : class
    {
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}