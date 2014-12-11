namespace Cedar.Testing
{
    using System;

    public static class Any
    {
        public static readonly DateTime Date = DateTime.MaxValue.AddSeconds(-60);
        public static readonly Guid Guid = Guid.NewGuid();
    }
}