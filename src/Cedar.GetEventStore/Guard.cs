namespace Cedar.GetEventStore
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

        internal static void EnsureNotEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException(String.Format("{0} is null ow whitespace", argumentName));
            }
        }
    }
}
