namespace Cedar.TypeResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using CuttingEdge.Conditions;

    public static class TypeResolvers
    {
        /// <summary>
        ///     Resolves a type from a parsed media type in the format {TypeFullName}_v{VersionNumber}.
        /// </summary>
        /// <param name="knownTypes">The known types that can be resolved.</param>
        /// <returns></returns>
        public static TryResolveType FullNameWithVersionSuffix([NotNull] IEnumerable<Type> knownTypes)
        {
            Condition.Requires(knownTypes, "knownTypes").IsNotNull();

            var knownTypeDictionary = knownTypes.
               ToDictionary(t => t.FullName.ToLowerInvariant(), t => t);

            return (IParsedMediaType parsedMediaType, out Type resolvedType) =>
            {
                var key = parsedMediaType.TypeName.ToLowerInvariant();
                if(parsedMediaType.Version.HasValue)
                {
                    key += "_v" + parsedMediaType.Version.Value;
                }
                return knownTypeDictionary.TryGetValue(key, out resolvedType);
            };
        }
    }
}