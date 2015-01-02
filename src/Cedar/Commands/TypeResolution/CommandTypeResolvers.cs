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
        ///     Matching is case insensitive.
        /// </summary>
        /// <param name="knownCommandTypes">The known types that can be resolved.</param>
        /// <returns>An instance of <see cref="ResolveCommandType"/>.</returns>
        public static ResolveCommandType FullNameWithUnderscoreVersionSuffix([NotNull] IEnumerable<Type> knownCommandTypes)
        {
            Condition.Requires(knownCommandTypes, "knownCommandTypes").IsNotNull();

            var knownTypeDictionary = knownCommandTypes.
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

        /// <summary>
        ///     Resolves a type from a parsed media type in the format {TypeFullName}v{VersionNumber}.
        ///     Matching is case insensitive.
        /// </summary>
        /// <param name="knownCommandTypes">The known types that can be resolved.</param>
        /// <returns>An instance of <see cref="ResolveCommandType"/>.</returns>
        public static ResolveCommandType FullNameWithVersionSuffix([NotNull] IEnumerable<Type> knownCommandTypes)
        {
            Condition.Requires(knownCommandTypes, "knownCommandTypes").IsNotNull();

            var knownTypeDictionary = knownCommandTypes.
               ToDictionary(t => t.FullName.ToLowerInvariant(), t => t);

            return (commandName, version) =>
            {
                var key = commandName.ToLowerInvariant();
                if (version.HasValue)
                {
                    key += "v" + version.Value;
                }
                Type resolvedType;
                return knownTypeDictionary.TryGetValue(key, out resolvedType) ? resolvedType : null;
            };
        }

        /// <summary>
        ///     Combine the resolvers in the order FullNameWithUnderscoreVersionSuffix, FullNameWithVersionSuffix.
        /// </summary>
        /// <param name="knownCommandTypes">The known types that can be resolved.</param>
        /// <returns>An instance of <see cref="ResolveCommandType"/>.</returns>
        public static ResolveCommandType All([NotNull] IEnumerable<Type> knownCommandTypes)
        {
            Condition.Requires(knownCommandTypes, "knownCommandTypes").IsNotNull();

            return (commandName, version) => FullNameWithUnderscoreVersionSuffix(knownCommandTypes)(commandName, version)
                                             ?? FullNameWithVersionSuffix(knownCommandTypes)(commandName, version);
        }
    }
}