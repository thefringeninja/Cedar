namespace Cedar.CommandHandling.Modules
{
    using System;

    /// <summary>
    /// Provides a way to get a command type from an http Content-Type value
    /// </summary>
    public interface ICommandTypeFromHttpContentType
    {
        Type GetCommandType(string contentType);
    }
}