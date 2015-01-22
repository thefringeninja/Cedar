namespace Cedar.Internal
{
    using System.Globalization;

    internal static class StringExtensions
    {
        internal static string FormatWith(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}