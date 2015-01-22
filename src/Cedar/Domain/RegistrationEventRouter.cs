namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;

    public class RegistrationEventRouter : IEventRouter
    {
        private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();
        private IAggregate _regsitered;

        public virtual void Register<T>(Action<T> handler)
        {
            _handlers[typeof (T)] = @event => handler((T) @event);
        }

        public virtual void Register(IAggregate aggregate)
        {
            if (aggregate == null)
            {
                throw new ArgumentNullException("aggregate");
            }

            _regsitered = aggregate;
        }

        public virtual void Dispatch(object eventMessage)
        {
            Action<object> handler;

            if (!_handlers.TryGetValue(eventMessage.GetType(), out handler))
            {
                _regsitered.ThrowHandlerNotFound(eventMessage);
            }

            handler(eventMessage);
        }
    }
}