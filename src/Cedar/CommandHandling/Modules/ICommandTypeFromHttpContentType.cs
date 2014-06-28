namespace Cedar.CommandHandling.Modules
{
    using System;

    /// <summary>
    /// Provides a way to get a command type from a Content-Type
    /// </summary>
    public interface ICommandTypeFromHttpContentType
    {
        Type GetCommandType(string contentType);
    }
}