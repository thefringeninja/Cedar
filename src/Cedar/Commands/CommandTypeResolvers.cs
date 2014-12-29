namespace Cedar.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cedar.Annotations;
    using CuttingEdge.Conditions;

    public static class CommandTypeResolvers
    {
        /// <summary>
        ///     Resolves a type from a parsed media type in the format {TypeFullName}_v{VersionNumber}.
        /// </summary>
        /// <param name="knownTypes">The known types that can be resolved.</param>
        /// <returns></returns>
        public static ResolveCommandType FullNameWithVersionSuffix([NotNull] IEnumerable<Type> knownTypes)
        {
            Condition.Requires(knownTypes, "knownTypes").IsNotNull();

            var knownTypeDictionary = knownTypes.
               ToDictionary(t => t.FullName.ToLowerInvariant(), t => t);

            return (commandName, version) =>
            {
                var key = commandName.ToLowerInvariant();
                if (version.HasValue)
                {
                    key += "_v" + version.Value;
                }
                Type resolvedType;
                return knownTypeDictionary.TryGetValue(key, out resolvedType) ? resolvedType : null;
            };
        }
    }
}