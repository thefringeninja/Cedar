namespace Cedar.Projections
{
    using System.Collections.Generic;

    public interface IProjectionResolver
    {
        IEnumerable<IProjectDomainEvent<TEvent>> ResolveAll<TEvent>() where TEvent : class;
    }
}