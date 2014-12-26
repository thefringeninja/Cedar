namespace Cedar.TypeResolution
{
    public delegate bool TryParseMediaType(string mediaType, out IParsedMediaAndSerializationType parsedMediaType);
}