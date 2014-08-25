namespace Cedar.Projections
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectDomainEvent<TEvent>
        where TEvent : class
    {
        Task Project(IDomainEventContext domainEventContext, TEvent domainEvent, CancellationToken cancellationToken);
    }
}