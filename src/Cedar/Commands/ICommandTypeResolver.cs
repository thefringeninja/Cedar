namespace Cedar.Commands
{
    using System;

    /// <summary>
    /// Provides a way to get a command type from an http Content-Type value
    /// </summary>
    public interface ICommandTypeResolver
    {
        Type GetFromContentType(string contentType);
    }
}