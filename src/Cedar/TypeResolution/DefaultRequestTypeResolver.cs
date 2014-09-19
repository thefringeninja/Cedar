namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Commands;


    /// <summary>
    /// Represents and way to get command type from a http Content-Type. ConentType is expected
    /// to be in the form of 'application/vnd.{VendorName}.{CommandTypeName}+json' or 
    /// 'application/vnd.{vendorName}.{commandtype}+xml' where CommanandTypeName ia a command's
    /// type Name in lowercase.
    /// </summary>
    public class DefaultRequestTypeResolver : IRequestTypeResolver
    {
        private class TypeDescriptor
        {
            private readonly string _vendor;
            private readonly Type _type;
            private readonly VersionedName _name;

            public TypeDescriptor(string vendor, Type type, Func<Type, VersionedName> getVersionedName)
            {
                _vendor = vendor;
                _type = type;

                _name = getVersionedName(type);
            }

            public override string ToString()
            {
                return FullName;
            }

            public Type Type
            {
                get { return _type; }
            }

            public int? Version
            {
                get { return _name.Version; }
            }

            public string FullName
            {
                get { return "application/vnd." + _vendor + "."  + _name.Name + ".v" + _name.Version; }
            }

            public string Name
            {
                get { return _name.Name; }
            }
        }

        private readonly string _vendorName;
        private readonly Func<IRequest, IEnumerable<string>> _getInputMediaTypes;
        private readonly Func<IRequest, IEnumerable<string>> _getOutputMediaTypes;
        private readonly ILookup<string, TypeDescriptor> _mapping;

        public DefaultRequestTypeResolver(
            string vendorName, 
            IEnumerable<Type> knownTypes, 
            Func<Type, VersionedName> getVersionedName = null, 
            Func<IRequest, IEnumerable<string>> getInputMediaTypes = null, 
            Func<IRequest, IEnumerable<string>> getOutputMediaTypes = null)
        {
            Guard.EnsureNullOrWhiteSpace(vendorName, "vendorName");
            Guard.EnsureNotNull(knownTypes, "knownTypes");

            _vendorName = vendorName;
            _getInputMediaTypes = getInputMediaTypes ?? (request => request.FromContentType().Union(request.FromPath(_vendorName)));
            _getOutputMediaTypes = getOutputMediaTypes ?? (request => request.FromAcceptHeader());
            _mapping = knownTypes.Select(type => new TypeDescriptor(vendorName, type, getVersionedName ?? RequestTypeMappingStrategies.FromTypeName))
                .ToLookup(x => x.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        string GetName(string mediaType)
        {
            Guard.EnsureNotNull(mediaType, "mediaType");

            return mediaType.Split('+').First().Remove(0, ("application/vnd." + _vendorName + ".").Length)
                .Split('.')
                .FirstOrDefault();
        }

        int? GetVersion(string mediaType)
        {
            Guard.EnsureNotNull(mediaType, "mediaType");

            mediaType = mediaType.Split('+').First(); // get rid of the serailization format if any
            foreach (var piece in mediaType.Split('.').Reverse()) // look for the last instance of 'v{digits}'
            {
                int version;
                if (false == piece.StartsWith("v")) continue;
                if (false == Int32.TryParse(piece.Remove(0, 1), out version)) continue;

                return version;
            }
            return default(int?);
        }

        public Type ResolveInputType(IRequest request)
        {
            return MatchMediaType(_getInputMediaTypes(request));
        }

        private Type MatchMediaType(IEnumerable<string> mediaTypes)
        {
            foreach (var mediaType in mediaTypes)
            {
                if (false == mediaType.StartsWith("application/vnd." + _vendorName)) continue;
                var versionedName = new VersionedName(GetName(mediaType), GetVersion(mediaType));
                var typeDescriptors = _mapping[versionedName.Name].OrderBy(typeDescriptor => typeDescriptor.Version).ToList();

                if (false == typeDescriptors.Any()) continue;

                if (false == versionedName.Version.HasValue)
                {
                    return typeDescriptors.Last().Type;
                }

                foreach (var typeDescriptor in typeDescriptors)
                {
                    if (typeDescriptor.Version >= versionedName.Version)
                    {
                        return typeDescriptor.Type;
                    }
                }
            }

            return null;
        }

        public Type ResolveOutputType(IRequest request)
        {
            return MatchMediaType(_getOutputMediaTypes(request));
        }
    }
}