namespace Cedar.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHandler<TMessage>
        where TMessage : class
    {
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}