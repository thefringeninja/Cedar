namespace Cedar.GetEventStore
{
    using System;
    using CuttingEdge.Conditions;

    public static class StringExtensions
    {
        public static string FormatStreamNameWithBucket(this string streamId, string bucketId = null)
        {
            Condition.Requires(streamId, "streamId").IsNotNullOrWhiteSpace();

            bucketId = string.IsNullOrWhiteSpace(bucketId) ? "default" : bucketId;

            return String.Format("[{0}].{1}", bucketId, streamId);
        }

        public static string FormatStreamNameWithoutBucket(this string streamId)
        {
            Condition.Requires(streamId, "streamId").IsNotNullOrWhiteSpace();

            var split = streamId.Split(new[] {'.'}, 2);

            if(split.Length < 2)
            {
                throw new ArgumentException(String.Format("Expected {0} to be prefixed with a bucket.", streamId), "streamId");
            }

            return split[1];
        }
    }
}