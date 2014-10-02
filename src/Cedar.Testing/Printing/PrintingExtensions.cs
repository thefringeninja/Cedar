namespace Cedar.Testing.Printing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using PowerAssert;

    public static class PrintingExtensions
    {
        public static IEnumerable<string> NicePrint(this object target, string prefix = "\t")
        {
            if(target == null)
            {
                yield return prefix + "???";
                yield break;
            }

            var s = target as string;
            if(s != null)
            {
                yield return prefix + s;
                yield break;
            }

            var ex = target as Exception;

            if(ex != null)
            {
                yield return ex.ToString();

                yield break;
            }

            if(target is IEnumerable && false == target is IQueryable)
            {
                foreach(var printed in (target as IEnumerable).Cast<object>().SelectMany(item => item.NicePrint(prefix)))
                {
                    yield return printed;
                }
                yield break;
            }
            if(target is LambdaExpression)
            {
                yield return PAssertFormatter.CreateSimpleFormatFor((LambdaExpression) target);
            }

            yield return prefix + target;
        }
    }
}