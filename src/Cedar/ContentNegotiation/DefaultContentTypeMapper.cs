namespace Cedar.ContentNegotiation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Cedar.Commands;

    /// <summary>
    /// Represents and way to get command type from a http Content-Type. ConentType is expected
    /// to be in the form of 'application/vnd.{VendorName}.{CommandTypeName}+json' or 
    /// 'application/vnd.{vendorName}.{commandtype}+xml' where CommanandTypeName ia a command's
    /// type Name in lowercase.
    /// </summary>
    public class DefaultContentTypeMapper : IContentTypeMapper
    {
        private readonly string _vendorName;
        private readonly Dictionary<string, Type> _mapping;

        public DefaultContentTypeMapper(string vendorName, IEnumerable<Type> knownTypes)
        {
            Guard.EnsureNullOrWhiteSpace(vendorName, "vendorName");
            Guard.EnsureNotNull(knownTypes, "knownTypes");

            _vendorName = vendorName;
            _mapping = knownTypes
                .ToDictionary(t => t.Name.ToLower(CultureInfo.InvariantCulture));
        }

        public Type GetFromContentType(string contentType)
        {
            string typeName = contentType
                .Replace(@"application/vnd." + _vendorName + ".", string.Empty)
                .Replace("+json", string.Empty)
                .Replace("+xml", string.Empty);

            Type type;
            return _mapping.TryGetValue(typeName, out type)
                ? type
                : null;
        }
    }
}