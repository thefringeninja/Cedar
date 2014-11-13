namespace Cedar.Handlers
{
    using System;

    public class EventHandled
    {
        public static EventHandled Successfuly(DomainEventMessage message)
        {
            var commitId = message.GetCommitId();

            return commitId.HasValue ? new EventHandled(commitId.Value) : Undefined();
        }

        public static EventHandled Unsuccessfuly(DomainEventMessage message, Exception exception)
        {
            var commitId = message.GetCommitId();

            return commitId.HasValue ? new EventHandled(commitId.Value, exception) : Undefined();
        }

        private static EventHandled Undefined()
        {
            return new EventHandled(Guid.Empty);
        }

        private EventHandled(Guid commitId, Exception exception = null)
        {
            CommitId = commitId;
            Exception = exception;
        }

        public readonly Guid CommitId;
        public readonly Exception Exception;

        public bool Successful
        {
            get { return Exception == null; }
        }
    }
}
