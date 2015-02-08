namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using TinyIoC;

    public class HandlerResolver : IHandlerResolver
    {
        private readonly TinyIoCContainer _container = new TinyIoCContainer();

        public HandlerResolver(params HandlerModule[] handlerModules)
        {
            foreach(var module in handlerModules)
            {
                foreach(var handlerRegistration in module.HandlerRegistrations)
                {
                    _container.Register(
                        handlerRegistration.RegistrationType,
                        handlerRegistration.HandlerInstance);
                }
            }
        }

        public IEnumerable<Handler<TMessage>> ResolveAll<TMessage>() where TMessage : class
        {
            return _container.ResolveAll<Handler<TMessage>>();
        }
    }
}