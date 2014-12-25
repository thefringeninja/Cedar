namespace Cedar.TypeResolution
{
    public interface ITypeNameAndVersion
    {
        string TypeName { get; }

        int? Version { get; }

        string SerializationType { get; }
    }
}