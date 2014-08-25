namespace Cedar.Projections
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectDomainEvent<TEvent>
        where TEvent : class
    {
        Task Project(IDomainEventContext context, TEvent domainEvent, CancellationToken cancellationToken);
    }
}