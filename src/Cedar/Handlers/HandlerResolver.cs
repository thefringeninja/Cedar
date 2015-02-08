namespace Cedar.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HandlerResolver : IHandlerResolver
    {
        private readonly Dictionary<Type, List<object>> _handlers = new Dictionary<Type, List<object>>(); 

        public HandlerResolver(params HandlerModule[] handlerModules)
        {
            foreach(var module in handlerModules)
            {
                foreach(var registration in module.HandlerRegistrations)
                {
                    List<object> handlers;
                    if(!_handlers.TryGetValue(registration.RegistrationType, out handlers))
                    {
                        handlers = new List<object>();
                    }
                    handlers.Add(registration.HandlerInstance);
                    _handlers[registration.RegistrationType] = handlers;
                }
            }
        }

        public IEnumerable<Handler<TMessage>> ResolveAll<TMessage>() where TMessage : class
        {
            return _handlers[typeof(Handler<TMessage>)]
                .Select(handler => (Handler<TMessage>) handler);
        }
    }
}