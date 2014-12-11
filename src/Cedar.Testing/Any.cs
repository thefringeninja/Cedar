namespace Cedar.Testing
{
    using System;
    using System.Linq;

    public static class Any
    {
        public static readonly DateTime Date = DateTime.MaxValue.AddSeconds(-60);
        public static readonly Guid Guid = new Guid(Enumerable.Repeat((byte)0xff, 16).ToArray());
    }
}