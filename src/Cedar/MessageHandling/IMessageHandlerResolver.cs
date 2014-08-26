namespace Cedar.MessageHandling
{
    using System.Collections.Generic;

    public interface IMessageHandlerResolver
    {
        IEnumerable<IMessageHandler<TEvent>> ResolveAll<TEvent>() where TEvent : class;
    }
}