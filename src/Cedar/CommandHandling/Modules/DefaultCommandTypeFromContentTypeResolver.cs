namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Cedar.Hosting;

    /// <summary>
    /// Represents and way to get command type from a http Content-Type. ConentType is expected
    /// to be in the form of 'application/vnd.{VendorName}.{CommandTypeName}+json' or 
    /// 'application/vnd.{vendorName}.{commandtype}+xml' where CommanandTypeName ia a command's
    /// type Name in lowercase.
    /// </summary>
    public class DefaultCommandTypeFromContentTypeResolver : ICommandTypeFromHttpContentType
    {
        private readonly string _vendorName;
        private readonly Dictionary<string, Type> _commandTypes;

        public DefaultCommandTypeFromContentTypeResolver(string vendorName, IEnumerable<Type> commandTypes)
        {
            Guard.EnsureNullOrWhiteSpace(vendorName, "vendorName");
            Guard.EnsureNotNull(commandTypes, "commandTypes");

            _vendorName = vendorName;
            _commandTypes = commandTypes.ToDictionary(t => t.Name.ToLower(CultureInfo.InvariantCulture));
        }

        public Type GetCommandType(string contentType)
        {
            string commandTypeName = contentType
                .Replace(@"application/vnd." + _vendorName + ".", string.Empty)
                .Replace("+json", string.Empty)
                .Replace("+xml", string.Empty);

            if (!_commandTypes.ContainsKey(commandTypeName))
            {
                throw new NotSupportedException(string.Format("Command {0} is not supported.", commandTypeName));
            }
            return _commandTypes[commandTypeName];
        }
    }
}