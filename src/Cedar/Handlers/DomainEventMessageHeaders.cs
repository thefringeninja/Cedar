namespace Cedar.Handlers
{
    using System;

    public static class DomainEventMessageHeaders
    {
        public const string StreamId = "StreamId";
        public const string CommitId = "CommitId";
        public const string Type = "Type";
        public const string Timestamp = "Timestamp";

        public static Guid? GetCommitId(this DomainEventMessage @event)
        {
            object commitIdValue;
            Guid commitId;
            if(false == @event.Headers.TryGetValue(CommitId, out commitIdValue)
               || commitIdValue == null
               || false == Guid.TryParse(commitIdValue.ToString(), out commitId))
            {
                return default(Guid?);
            }
            return commitId;
        }
    }
}