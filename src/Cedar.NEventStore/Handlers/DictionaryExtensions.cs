namespace Cedar.NEventStore.Handlers
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class DictionaryExtensions
    {
        public static IDictionary<string, object> Merge(
            this IDictionary<string, object> target,
            params IDictionary<string, object>[] others)
        {
            return target.Merge(others.AsEnumerable());
        }

        private static IDictionary<string, object> Merge(
            this IDictionary<string, object> target,
            IEnumerable<IDictionary<string, object>> others)
        {
            foreach(var pair in others.SelectMany(other => other))
            {
                target[pair.Key] = pair.Value;
            }

            return target;
        }
    }
}