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

        internal static void EnsureNullOrWhiteSpace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("{0} is null ow whitespace".FormatWith(argumentName));
            }
        }
    }
}
