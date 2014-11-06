namespace Cedar
{
    using System;

    public static class StreamExtensions
    {
        public static string FormatStreamNameWithBucket(this string streamId, string bucketId = null)
        {
            bucketId = bucketId ?? "default";

            Guard.EnsureNotNullOrWhiteSpace(bucketId, "bucketId");
            Guard.EnsureNotNullOrWhiteSpace(streamId, "streamId");

            return String.Format("[{0}].{1}", bucketId, streamId);
        }

        public static string FormatStreamNameWithoutBucket(this string streamId)
        {
            Guard.EnsureNotNullOrWhiteSpace(streamId, "streamId");

            var split = streamId.Split(new[] {'.'}, 2);

            if(split.Length < 2)
            {
                throw new ArgumentException(String.Format("Expected {0} to be prefixed with a bucket.", streamId), "streamId");
            }

            return split[1];
        }
    }
}