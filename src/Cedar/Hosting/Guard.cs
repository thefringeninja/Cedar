namespace Cedar.Hosting
{
    using System;
    using Cedar.Extensions;

    internal static class Guard
    {
        internal static void Ensure(bool value, string message)
        {
            if (!value)
            {
                throw new InvalidOperationException(message);
            }
        }
        internal static void Ensure(bool value, string argumentName, string message)
        {
            if (!value)
            {
                throw new ArgumentException(message, argumentName);
            }
        }

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
