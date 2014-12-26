namespace Cedar.TypeResolution
{
    public interface IParsedMediaAndSerializationType : IParsedMediaType
    {
        string SerializationType { get; }
    }
}