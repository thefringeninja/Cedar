namespace Cedar.TypeResolution
{
    using System;
    using System.IO;
    using System.Linq;

    public interface IRequest
    {
        Uri Uri { get; }

        ILookup<string, string> Headers { get; }
        Stream Body { get; }
    }
}