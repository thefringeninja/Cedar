namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;

    public static class MediaTypeParsers
    {
        private static readonly TryParseMediaType[] MediaTypeParserCollection = new[]
        {
            MediaTypeWithoutVersion,
            MediaTypeWithMinusVersion,
            MediaTypeWithQualifierVersion,
            MediaTypeWithDotVersion
        };

        /// <summary>
        /// Gets the collection of default parsers. The order is MediaTypeWithoutVersion, MediaTypeWithMinusVersion,
        /// MediaTypeWithQualifierVersion, MediaTypeWithDotVersion.
        /// </summary>
        /// <value>
        /// The default parsers.
        /// </value>
        public static IEnumerable<TryParseMediaType> DefaultParsers { get { return MediaTypeParserCollection; } } 

        /// <summary>
        /// Gets the media type parser that handles media types that have the version
        /// in the format 'application/vnd.{TypeName}+{SerializationType}'
        /// </summary>
        /// <value>
        /// A media type parser.
        /// </value>
        public static TryParseMediaType MediaTypeWithoutVersion
        {
            get 
            { 
                return (string mediaType, out ITypeNameAndVersion typeNameAndVersion) =>
                {
                    typeNameAndVersion = null;

                    // 'application/vnd.{TypeName}+{SerializationType}' -> {TypeName}+{SerializationType}
                    mediaType = mediaType.Replace("application/vnd.", string.Empty);

                    // {TypeName}+{SerializationType} -> [{TypeName} , {SerializationType}]
                    var typeAndSerialization = mediaType.Split('+');
                    if(typeAndSerialization.Length != 2)
                    {
                        return false;
                    }
                    string typeName = typeAndSerialization[0];
                    string serializationType = typeAndSerialization[1];

                    typeNameAndVersion = new TypeNameAndVersion(typeName, null, serializationType);
                    return true;
                }; 
            }
        }

        /// <summary>
        /// Gets the media type parser that handles media types that have the version
        /// in the format 'application/vnd.{TypeName}.v{Version}+{SerializationType}'
        /// </summary>
        /// <value>
        /// A media type parser.
        /// </value>
        public static TryParseMediaType MediaTypeWithDotVersion
        {
            get
            {
                return (string mediaType, out ITypeNameAndVersion typeNameAndVersion) =>
                {
                    typeNameAndVersion = null;

                    // 'application/vnd.{TypeName}.v{Version}+{SerializationType}' -> {TypeName}.v{Version}+{SerializationType}
                    mediaType = mediaType.Replace("application/vnd.", string.Empty);
                    
                    // {TypeName}.v{Version}+{SerializationType} -> [{TypeName}.v{Version} , {SerializationType}]
                    var typeAndSerialization = mediaType.Split('+');
                    if (typeAndSerialization.Length != 2)
                    {
                        return false;
                    }
                    string typeAndVersion = typeAndSerialization[0]; // {TypeName}.v{Version} 
                    string serializationType = typeAndSerialization[1]; // {SerializationType}

                    // {TypeName}.v{Version} -> 
                    var lastIndexOf = typeAndVersion.LastIndexOf(".v", StringComparison.Ordinal);
                    if(lastIndexOf <= 0)
                    {
                        return false;
                    }

                    // {TypeName}.v{Version}
                    // ----------
                    string type = typeAndVersion.Substring(0, lastIndexOf);
                    // {TypeName}.v{Version}
                    //             ---------
                    string versionString = typeAndVersion.Substring(lastIndexOf + 2);

                    int version;
                    if(!int.TryParse(versionString, out version))
                    {
                        return false;
                    }
                    typeNameAndVersion = new TypeNameAndVersion(type, version, serializationType);
                    return true;
                };
            }
        }

        /// <summary>
        /// Gets the media type parser that handles media types that have the version
        /// in the format 'application/vnd.{TypeName}-v{Version}+{SerializationType}'
        /// </summary>
        /// <value>
        /// A media type parser.
        /// </value>
        public static TryParseMediaType MediaTypeWithMinusVersion
        {
            get
            {
                return (string mediaType, out ITypeNameAndVersion typeNameAndVersion) =>
                {
                    typeNameAndVersion = null;

                    // 'application/vnd.{TypeName}.v{Version}+{SerializationType}' -> {TypeName}-v{Version}+{SerializationType}
                    mediaType = mediaType.Replace("application/vnd.", string.Empty);

                    // {TypeName}.v{Version}+{SerializationType} -> [{TypeName}-v{Version} , {SerializationType}]
                    var typeAndSerialization = mediaType.Split('+');
                    if (typeAndSerialization.Length != 2)
                    {
                        return false;
                    }
                    string typeAndVersion = typeAndSerialization[0]; // {TypeName}.v{Version} 
                    string serializationType = typeAndSerialization[1]; // {SerializationType}

                    // {TypeName}.v{Version} -> 
                    var lastIndexOf = typeAndVersion.LastIndexOf("-v", StringComparison.Ordinal);
                    if (lastIndexOf <= 0)
                    {
                        return false;
                    }

                    // {TypeName}-v{Version}
                    // ----------
                    string type = typeAndVersion.Substring(0, lastIndexOf);
                    // {TypeName}-v{Version}
                    //             ---------
                    string versionString = typeAndVersion.Substring(lastIndexOf + 2);

                    int version;
                    if (!int.TryParse(versionString, out version))
                    {
                        return false;
                    }
                    typeNameAndVersion = new TypeNameAndVersion(type, version, serializationType);
                    return true;
                };
            }
        }

        /// <summary>
        /// Gets the media type parser that handles media types that have the version
        /// in the format 'application/vnd.{TypeName}+{SerializationType};v={Version}'
        /// </summary>
        /// <value>
        /// A media type parser.
        /// </value>
        public static TryParseMediaType MediaTypeWithQualifierVersion
        {
            get
            {
                return (string mediaType, out ITypeNameAndVersion typeNameAndVersion) =>
                {
                    typeNameAndVersion = null;

                    // 'application/vnd.{TypeName}+{SerializationType};v={Version}' -> {TypeName}+{SerializationType};v={Version}
                    mediaType = mediaType.Replace("application/vnd.", string.Empty);

                    // {TypeName}+{SerializationType};v={Version} -> [{TypeName}+{SerializationType} , v={Version}]
                    var typeAndVersion = mediaType.Split(';');
                    if (typeAndVersion.Length != 2)
                    {
                        return false;
                    }
                    string typeAndSerialization = typeAndVersion[0]; // {TypeName}+{SerializationType}
                    string versionString = typeAndVersion[1].Replace("v=", string.Empty); // {Version}

                    int version;
                    if (!int.TryParse(versionString, out version))
                    {
                        return false;
                    }

                    // {TypeName}+{SerializationType} ->  [{TypeName}, {SerializationType}]
                    var strings = typeAndSerialization.Split('+');
                    if (strings.Length != 2)
                    {
                        return false;
                    }

                    string typeName = strings[0];
                    string serializationType = strings[1];

                    typeNameAndVersion = new TypeNameAndVersion(typeName, version, serializationType);
                    return true;
                };
            }
        }
    }
}