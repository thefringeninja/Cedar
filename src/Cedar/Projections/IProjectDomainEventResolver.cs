namespace Cedar.Projections
{
    using System.Collections.Generic;

    public interface IProjectDomainEventResolver
    {
        IEnumerable<IProjectDomainEvent<T>> ResolveAll<T>() where T : class;
    }
}