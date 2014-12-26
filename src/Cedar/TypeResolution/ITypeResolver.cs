namespace Cedar.TypeResolution
{
    using System;

    public interface ITypeResolver
    {
        Type Resolve(string typeName, int? version);
    }
}