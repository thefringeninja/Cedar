namespace Cedar.TypeResolution
{
    public class TypeNameAndVersion : ITypeNameAndVersion
    {
        private readonly string _typeName;
        private readonly int? _version;
        private readonly string _serializationType;

        public TypeNameAndVersion(string typeName, int? version, string serializationType)
        {
            _typeName = typeName;
            _version = version;
            _serializationType = serializationType;
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        public int? Version
        {
            get { return _version; }
        }

        public string SerializationType
        {
            get { return _serializationType; }
        }
    }
}