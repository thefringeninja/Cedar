namespace Cedar.TypeResolution
{
    using System;

    public delegate bool TryResolveType(IParsedMediaType parsedMediaType, out Type resolvedType);
}