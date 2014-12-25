namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CuttingEdge.Conditions;

    /// <summary>
    ///     Represents and way to get command type from a http Content-Type. ConentType is expected
    ///     to be in the form of 'application/vnd.{VendorName}.{RequestType}+json' or
    ///     'application/vnd.{vendorName}.{commandtype}+xml' where CommanandTypeName ia a command's
    ///     type Name in lowercase.
    /// </summary>
    public class DefaultRequestTypeResolver : IRequestTypeResolver
    {
        private readonly Func<IRequest, IEnumerable<string>> _getInputMediaTypes;
        private readonly Func<IRequest, IEnumerable<string>> _getOutputMediaTypes;
        private readonly ILookup<string, TypeDescriptor> _mapping;
        private readonly string _vendorName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestTypeResolver"/> class.
        /// </summary>
        /// <param name="vendorName">Name of the vendor.</param>
        /// <param name="knownRequestTypes">The known types.</param>
        /// <param name="getVersionedName">Name of the get versioned.</param>
        /// <param name="getInputMediaTypes">The get input media types.</param>
        /// <param name="getOutputMediaTypes">The get output media types.</param>
        public DefaultRequestTypeResolver(
            string vendorName,
            IEnumerable<Type> knownRequestTypes,
            Func<Type, TypeNameAndVersionOld> getVersionedName = null,
            Func<IRequest, IEnumerable<string>> getInputMediaTypes = null,
            Func<IRequest, IEnumerable<string>> getOutputMediaTypes = null)
        {
            Condition.Requires(vendorName, "vendorName").IsNotNullOrWhiteSpace();
            Condition.Requires(knownRequestTypes, "knownRequestTypes").IsNotNull();

            _vendorName = vendorName;
            _getInputMediaTypes = getInputMediaTypes ?? (request => request.FromContentType().Union(request.FromPath(_vendorName)));
            _getOutputMediaTypes = getOutputMediaTypes ?? (request => request.FromAcceptHeader());
            _mapping = knownRequestTypes.Select(
                type => new TypeDescriptor(vendorName, type, getVersionedName ?? RequestTypeMappingStrategies.FromTypeName))
                .ToLookup(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        public Type ResolveInputType(IRequest request)
        {
            return MatchMediaType(_getInputMediaTypes(request));
        }

        public Type ResolveOutputType(IRequest request)
        {
            return MatchMediaType(_getOutputMediaTypes(request));
        }

        private string GetName(string mediaType)
        {
            return mediaType.Split('+').First().Remove(0, ("application/vnd." + _vendorName + ".").Length)
                .Split('.')
                .FirstOrDefault();
        }

        private int? GetVersion(string mediaType)
        {
            mediaType = mediaType.Split('+').First(); // get rid of the serailization format if any
            foreach(string piece in mediaType.Split('.').Reverse()) // look for the last instance of 'v{digits}'
            {
                int version;
                if(false == piece.StartsWith("v"))
                {
                    continue;
                }
                if(false == Int32.TryParse(piece.Remove(0, 1), out version))
                {
                    continue;
                }

                return version;
            }
            return default(int?);
        }

        private Type MatchMediaType(IEnumerable<string> mediaTypes)
        {
            foreach(string mediaType in mediaTypes)
            {
                if(false == mediaType.StartsWith("application/vnd." + _vendorName))
                {
                    continue;
                }
                var versionedName = new TypeNameAndVersionOld(GetName(mediaType), GetVersion(mediaType));
                List<TypeDescriptor> typeDescriptors =
                    _mapping[versionedName.Name].OrderBy(typeDescriptor => typeDescriptor.Version).ToList();

                if(false == typeDescriptors.Any())
                {
                    continue;
                }

                if(false == versionedName.Version.HasValue)
                {
                    return typeDescriptors.Last().Type;
                }

                foreach(TypeDescriptor typeDescriptor in typeDescriptors)
                {
                    if(typeDescriptor.Version >= versionedName.Version)
                    {
                        return typeDescriptor.Type;
                    }
                }
            }

            return null;
        }

        private class TypeDescriptor
        {
            private readonly TypeNameAndVersionOld _nameAndVersion;
            private readonly Type _type;
            private readonly string _vendor;

            public TypeDescriptor(string vendor, Type type, Func<Type, TypeNameAndVersionOld> getVersionedName)
            {
                _vendor = vendor;
                _type = type;
                _nameAndVersion = getVersionedName(type);
            }

            public Type Type
            {
                get { return _type; }
            }

            public int? Version
            {
                get { return _nameAndVersion.Version; }
            }

            public string FullName
            {
                get { return "application/vnd." + _vendor + "." + _nameAndVersion.Name + ".v" + _nameAndVersion.Version; }
            }

            public string Name
            {
                get { return _nameAndVersion.Name; }
            }

            public override string ToString()
            {
                return FullName;
            }
        }
    }
}