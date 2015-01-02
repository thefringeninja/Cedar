namespace Cedar.Commands.TypeResolution
{
    using System;

    /// <summary>
    ///     Resolves a <see cref="Type"/> from a parsed media type.
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        ///     Resolves a type.
        /// </summary>
        /// <param name="parsedMediaType">The parsed media type.</param>
        /// <returns>A type or null if none found.</returns>
        Type Resolve(IParsedMediaType parsedMediaType);
    }
}