namespace Cedar.Commands.TypeResolution
{
    using System;

    /// <summary>
    ///     A delegate to resolve a <see cref="Type"/> from a command name and optional version.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <param name="version">The version of the command.</param>
    /// <returns>A command type; null if none resolved.</returns>
    public delegate Type ResolveCommandType(string commandName, int? version);
}