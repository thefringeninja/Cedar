namespace Cedar.Testing.Printing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Inflector;
    using PowerAssert;

    public static class PrintingExtensions
    {
        public static IEnumerable<string> NicePrint(this object target, string prefix = "\t")
        {
            if(target == null)
            {
                return NicePrintUnknown(prefix);
            }

            var s = target as string;

            if(s != null)
            {
                return NicePrintString(s, prefix);
            }

            var exception = target as Exception;

            if(exception != null)
            {
                return NicePrintException(exception, prefix);
            }

            var enumerable = target as IEnumerable;

            if(enumerable != null && false == target is IQueryable)
            {
                return NicePrintEnumerable(enumerable, prefix);
            }

            var expression = target as LambdaExpression;

            if(expression != null)
            {
                return NicePrintExpression(expression, prefix);
            }

            return NicePrintString(target.ToString(), prefix);
        }

        public static string GetCategoryName(this Type foundOn)
        {
            return foundOn.Name.RemovePunctuation().Underscore().Titleize();
        }

        public static string GetCategoryId(this Type foundOn)
        {
            return foundOn.FullName.RemovePunctuation().Underscore();
        }

        private static string RemovePunctuation(this string target)
        {
            return target.Replace('.', ' ');
        }

        private static IEnumerable<string> NicePrintExpression(LambdaExpression target, string prefix)
        {
            yield return prefix + PAssertFormatter.CreateSimpleFormatFor(target);
        }

        private static IEnumerable<string> NicePrintEnumerable(IEnumerable target, string prefix)
        {
            return target.Cast<object>().SelectMany(item => item.NicePrint(prefix));
        }

        private static IEnumerable<string> NicePrintException(Exception ex, string prefix)
        {
            yield return prefix + ex;
        }

        private static IEnumerable<string> NicePrintString(string target, string prefix)
        {
            return target.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, index) => prefix + line);
        }

        private static IEnumerable<string> NicePrintUnknown(string prefix)
        {
            yield return prefix + "???";
        }
    }
}