namespace Cedar
{
    using System.Globalization;

    internal static class StringExtensions
    {
        internal static string FormatWith(this string format, object arg)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg);
        }

        internal static string FormatWith(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}