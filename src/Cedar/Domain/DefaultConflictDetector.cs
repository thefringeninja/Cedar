namespace Cedar.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The conflict detector is used to determine if the events to be committed represent
    ///     a true business conflict as compared to events that have already been committed, thus
    ///     allowing reconciliation of optimistic concurrency problems.
    /// </summary>
    /// <remarks>
    ///     The implementation contains some internal lambda "magic" which allows casting between
    ///     TCommitted, TUncommitted, and System.Object and in a completely type-safe way.
    /// </remarks>
    public class DefaultConflictDetector : IConflictDetector
    {
        //declaring different delegate so that ConflictDelegate(object, object) can be marked obsolete

        private readonly IDictionary<Type, IDictionary<Type, ConflictPredicate>> _actions =
            new Dictionary<Type, IDictionary<Type, ConflictPredicate>>();

        public void Register<TUncommitted, TCommitted>(ConflictDelegate<TUncommitted, TCommitted> handler)
            where TUncommitted : class
            where TCommitted : class
        {
            IDictionary<Type, ConflictPredicate> inner;
            if (!_actions.TryGetValue(typeof (TUncommitted), out inner))
            {
                _actions[typeof (TUncommitted)] = inner = new Dictionary<Type, ConflictPredicate>();
            }

            inner[typeof (TCommitted)] =
                (uncommitted, committed) => handler(uncommitted as TUncommitted, committed as TCommitted);
        }

        public bool ConflictsWith(IEnumerable<object> uncommittedEvents, IEnumerable<object> committedEvents)
        {
            return (from object uncommitted in uncommittedEvents
                from object committed in committedEvents
                where Conflicts(uncommitted, committed)
                select uncommittedEvents).Any();
        }

        private bool Conflicts(object uncommitted, object committed)
        {
            IDictionary<Type, ConflictPredicate> registration;
            if (!_actions.TryGetValue(uncommitted.GetType(), out registration))
            {
                return uncommitted.GetType() == committed.GetType();
                    // no reg, only conflict if the events are the same time
            }

            ConflictPredicate callback;
            if (!registration.TryGetValue(committed.GetType(), out callback))
            {
                return true;
            }

            return callback(uncommitted, committed);
        }

        private delegate bool ConflictPredicate(object uncommitted, object committed);
    }
}