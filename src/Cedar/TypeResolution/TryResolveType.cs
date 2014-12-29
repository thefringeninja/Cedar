namespace Cedar.TypeResolution
{
    using System;

    /// <summary>
    ///     A delegate to try to resolve a <see cref="Type"/> from a parsed media type.
    /// </summary>
    /// <param name="parsedMediaType">The parsed media type.</param>
    /// <param name="resolvedType">The resolved type.</param>
    /// <returns>true if a type is found; otherwise false.</returns>
    public delegate bool TryResolveType(IParsedMediaType parsedMediaType, out Type resolvedType);
}