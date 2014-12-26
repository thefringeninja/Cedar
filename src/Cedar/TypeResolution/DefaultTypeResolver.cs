namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using CuttingEdge.Conditions;

    /// <summary>
    ///     Default type resolver will match parsed media types with version
    ///     (i.e. Typename='foo.bar', Version=2) against type full names in
    ///     the format Foo.Bar_v2. If no version is specified then the type
    ///     full name will be Foo.Bar.
    /// 
    ///     Example: 'application/vnd.foo.bar-v2;xml' would match type
    ///     Foo.Bar_v2
    /// </summary>
    public class DefaultTypeResolver : ITypeResolver
    {
        private readonly Dictionary<string, Type> _knownTypes;

        public DefaultTypeResolver([NotNull] IEnumerable<Type> knownTypes)
        {
            Condition.Requires(knownTypes, "knownTypes").IsNotNull();

            _knownTypes = knownTypes.
                ToDictionary(t => t.FullName.ToLowerInvariant(), t => t);
        }

        public virtual Type Resolve(IParsedMediaType parsedMediaType)
        {
            var key = parsedMediaType.TypeName.ToLowerInvariant();
            if(parsedMediaType.Version.HasValue)
            {
                key += "_v" + parsedMediaType.Version.Value;
            }
            Type type;
            _knownTypes.TryGetValue(key, out type);
            return type;
        }
    }
}