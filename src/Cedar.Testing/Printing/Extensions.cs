namespace Cedar.Testing.Printing
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    internal static class Extensions
    {
        public static string NicePrint(this object target)
        {
            if (target == null)
            {
                return "\t???";
            }

            var s = target as string;
            if (s != null)
            {
                return "\t" + s;
            }

            if (target is IEnumerable && false == target is IQueryable)
            {
                return (target as IEnumerable)
                    .OfType<object>()
                    .Aggregate(new StringBuilder(),
                        (builder, x) => builder.AppendLine("\t" + x.ToString()),
                        builder => builder.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
            }

            return "\t" + target;
        }
    }
}