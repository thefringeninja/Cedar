namespace Cedar.Handlers
{
    using System;

    public static class EventMessageHeaders
    {
        public const string StreamId = "StreamId";
        public const string CommitId = "CommitId";
        public const string Type = "Type";
        public const string Timestamp = "Timestamp";

        public static Guid? GetCommitId(this EventMessage @event)
        {
            object commitIdValue;
            Guid commitId;
            if(!@event.Headers.TryGetValue(CommitId, out commitIdValue)
               || commitIdValue == null
               || !Guid.TryParse(commitIdValue.ToString(), out commitId))
            {
                return default(Guid?);
            }
            return commitId;
        }
    }
}