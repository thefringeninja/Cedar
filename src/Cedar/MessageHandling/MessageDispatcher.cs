namespace Cedar.MessageHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;

    public class MessageDispatcher
    {
        private readonly IMessageHandlerResolver _messageHandlerResolver;
        private readonly Dictionary<Type, Func<object, CancellationToken, Task>> _dispatcherDelegateCache
            = new Dictionary<Type, Func<object, CancellationToken, Task>>();
        private readonly MethodInfo _dispatchEventMethod;

        public MessageDispatcher(IMessageHandlerResolver messageHandlerResolver)
        {
            if (messageHandlerResolver == null)
            {
                throw new ArgumentNullException("messageHandlerResolver");
            }

            _messageHandlerResolver = messageHandlerResolver;
            _dispatchEventMethod = GetType().GetMethod("DispatchMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            Contract.Assert(_dispatchEventMethod != null);
        }

        public async Task DispatchMessage(object message, CancellationToken cancellationToken)
        {
            var dispatchDelegate = GetDispatchDelegate(message.GetType());
            await dispatchDelegate(message, cancellationToken);
        }

        private Func<object, CancellationToken, Task> GetDispatchDelegate(Type type)
        {
            // Cache dispatch delages - a bit of a perf optimization
            Func<object, CancellationToken, Task> dispatchDelegate;
            if (_dispatcherDelegateCache.TryGetValue(type, out dispatchDelegate))
            {
                return dispatchDelegate;
            }
            var dispatchGenericMethod = _dispatchEventMethod.MakeGenericMethod(type);
            dispatchDelegate = (message, cancellationToken) =>
                (Task)dispatchGenericMethod.Invoke(this, new[] { message, cancellationToken });
            _dispatcherDelegateCache.Add(type, dispatchDelegate);
            return dispatchDelegate;
        }

        [UsedImplicitly]
        private async Task DispatchMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
            where TMessage : class
        {
            IEnumerable<IMessageHandler<TMessage>> messageHandlers = _messageHandlerResolver.ResolveAll<TMessage>();
            foreach (var projector in messageHandlers)
            {
                await projector.Handle(message, cancellationToken);
            }
        }
    }
}