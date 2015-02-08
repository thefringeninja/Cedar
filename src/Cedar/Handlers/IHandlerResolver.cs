namespace Cedar.Handlers
{
    using System.Collections.Generic;

    public interface IHandlerResolver
    {
        IEnumerable<Handler<TMessage>> ResolveAll<TMessage>() where TMessage : class;
    }
}