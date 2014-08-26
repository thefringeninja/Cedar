namespace Cedar.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cedar.Annotations;
    using NEventStore;

    public class CommitDispatcher
    {
        private delegate Task DispatchDomainEventDelegate(
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            object domainEvent,
            CancellationToken cancellationToken);

        private readonly Func<object, CancellationToken, Task> _dispatchMessage;
        private readonly Dictionary<Type, DispatchDomainEventDelegate> _dispatcherDelegateCache = new Dictionary<Type, DispatchDomainEventDelegate>();
        private readonly MethodInfo _dispatchCommitMethod;

        public CommitDispatcher(Func<object, CancellationToken, Task> dispatchMessage)
        {
            _dispatchMessage = dispatchMessage;
            _dispatchCommitMethod = GetType().GetMethod("DispatchDomainEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            Contract.Assert(_dispatchCommitMethod != null);
        }

        public async Task DispatchDomainEvent(ICommit commit)
        {
            int version = commit.StreamRevision;
            foreach (var eventMessage in commit.Events)
            {
                var dispatchDelegate = GetDispatchDelegate(eventMessage.Body.GetType());
                await dispatchDelegate(commit, version, eventMessage.Headers, eventMessage.Body, CancellationToken.None);
                version++;
            }
        }

        private DispatchDomainEventDelegate GetDispatchDelegate(Type type)
        {
            // Cache dispatch delages - a bit of a perf optimization
            DispatchDomainEventDelegate dispatchDelegate;
            if (_dispatcherDelegateCache.TryGetValue(type, out dispatchDelegate))
            {
                return dispatchDelegate;
            }
            var dispatchGenericMethod = _dispatchCommitMethod.MakeGenericMethod(type);
            dispatchDelegate = (commit, version, eventHeaders, domainEvent, cancellationToken) =>
                (Task)dispatchGenericMethod.Invoke(this, new[] { commit, version, eventHeaders, domainEvent, cancellationToken });
            _dispatcherDelegateCache.Add(type, dispatchDelegate);
            return dispatchDelegate;
        }

        [UsedImplicitly]
        private Task DispatchDomainEvent<TEvent>(
            ICommit commit,
            int version,
            IReadOnlyDictionary<string, object> eventHeaders,
            TEvent @event,
            CancellationToken cancellationToken)
            where TEvent : class
        {
            var message = new DomainEventMessage<TEvent>(commit, version, eventHeaders, @event);
            return _dispatchMessage(message, cancellationToken);
        }
    }
}