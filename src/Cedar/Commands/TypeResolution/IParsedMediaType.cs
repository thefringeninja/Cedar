namespace Cedar.Commands.TypeResolution
{
    /// <summary>
    ///     Represents a parsed media type. Example, given a media type 'application/vnd.foo.bar.v2+json'
    ///     an implementation would then result in Typename='foo.bar', Version='2' and SerializationType = 'json'
    /// </summary>
    public interface IParsedMediaType
    {
        /// <summary>
        ///     Gets the name of the type as extracted from the media type.
        /// </summary>
        /// <value>
        ///     The name of the type.
        /// </value>
        string TypeName { get; }

        /// <summary>
        ///     Gets the version of the type as extracted from the media type. If no version
        ///     is parsed, then it will be null.
        /// </summary>
        /// <value>
        ///     The version of the media type.
        /// </value>
        int? Version { get; }

        /// <summary>
        ///     Gets the serialization type.
        /// </summary>
        string SerializationType { get; }
    }
}