namespace Cedar.Projections
{
    using System.Collections.Generic;

    public interface IProjectDomainEventResolver
    {
        IEnumerable<DomainEventMessage<T>> ResolveAll<T>() where T : class;
    }
}