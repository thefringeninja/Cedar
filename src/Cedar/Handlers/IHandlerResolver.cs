namespace Cedar.Handlers
{
    using System.Collections.Generic;

    public interface IHandlerResolver
    {
        IEnumerable<IHandler<T>> ResolveAll<T>() where T : class;
    }
}