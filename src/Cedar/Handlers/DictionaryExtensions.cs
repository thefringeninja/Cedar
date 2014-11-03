namespace Cedar.Handlers
{
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        public static IDictionary<string, object> Merge(this IDictionary<string, object> target,
            params IDictionary<string, object>[] others)
        {
            return target.Merge(others.AsEnumerable());
        }

        public static IDictionary<string, object> Merge(this IDictionary<string, object> target,
            IEnumerable<IDictionary<string, object>> others)
        {
            Guard.EnsureNotNull(target, "target");
            Guard.EnsureNotNull(others, "others");

            foreach(var pair in others.SelectMany(other => other))
            {
                target[pair.Key] = pair.Value;
            }

            return target;
        }
    }
}