namespace Cedar.Testing.Printing
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Extensions
    {

        public static IEnumerable<string> NicePrint(this object target, string prefix = "\t")
        {
            if (target == null)
            {
                yield return prefix + "???";
                yield break;
            }

            var s = target as string;
            if (s != null)
            {
                yield return prefix + s;
                yield break;
            }

            if (target is IEnumerable && false == target is IQueryable)
            {
                foreach (var item in (target as IEnumerable))
                {
                    yield return prefix + (item ?? "???");
                }
                yield break;
            }

            yield return prefix + target;
        }
    }
}