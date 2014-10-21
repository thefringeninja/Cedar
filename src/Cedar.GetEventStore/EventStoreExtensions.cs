namespace Cedar
{
    using System;

    public static class EventStoreExtensions
    {
        public static string FormatStreamName(string streamId, string bucketId = null)
        {
            bucketId = bucketId ?? "default";

            Guard.EnsureNotEmpty(bucketId, "bucketId");

            return String.Format("[{0}].{1}", bucketId, streamId);
        }
    }
}