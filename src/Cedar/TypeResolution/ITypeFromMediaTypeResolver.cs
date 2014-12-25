namespace Cedar.TypeResolution
{
    using System;

    public interface ITypeFromMediaTypeResolver
    {
        Type Get(string contentType);
    }
}