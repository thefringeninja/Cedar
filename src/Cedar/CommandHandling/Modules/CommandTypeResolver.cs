namespace Cedar.CommandHandling.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class CommandTypeResolver : ICommandTypeResolver
    {
        private readonly string _vendorName;
        private readonly Dictionary<string, Type> _commandTypes;

        public CommandTypeResolver(string vendorName, IEnumerable<Type> commandTypes)
        {
            _vendorName = vendorName;
            _commandTypes = commandTypes.ToDictionary(t => t.Name.ToLower(CultureInfo.InvariantCulture));
        }

        public Type GetCommandType(string contentType)
        {
            string commandTypeName = contentType
                .Replace(@"application/vnd." + _vendorName + ".", string.Empty)
                .Replace("+json", string.Empty)
                .Replace("+xml", string.Empty);

            return _commandTypes[commandTypeName];
        }
    }
}