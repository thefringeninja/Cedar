namespace Cedar
{
    using System;

    internal static class Guard
    {
        internal static void EnsureNotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        internal static void EnsureNotNullOrWhiteSpace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("{0} is null or whitespace".FormatWith(argumentName));
            }
        }
    }
}
