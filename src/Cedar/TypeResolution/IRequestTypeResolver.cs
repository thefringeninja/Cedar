namespace Cedar.TypeResolution
{
    using System;

    /// <summary>
    /// Provides a way to get a command type from an http Content-Type value
    /// </summary>
    public interface IRequestTypeResolver
    {
        Type ResolveInputType(IRequest request);
        Type ResolveOutputType(IRequest request);
    }
}