namespace Cedar.Commands
{
    using Cedar.Serialization;

    /// <summary>
    ///     A delegate to resolve a <see cref="ISerializer"/>.
    /// </summary>
    /// <param name="serializerName">The name of the serializer. e.g. 'json', 'xml'.</param>
    /// <returns>A serializer; null if none resolved.</returns>
    public delegate ISerializer ResolveSerializer(string serializerName);
}