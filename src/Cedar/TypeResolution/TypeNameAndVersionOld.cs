namespace Cedar.TypeResolution
{
    public class TypeNameAndVersionOld
    {
        private readonly string _name;
        private readonly int? _version;

        public TypeNameAndVersionOld(string name, int? version = null)
        {
            _name = name;
            _version = version;
        }

        public string Name
        {
            get { return _name; }
        }

        public int? Version
        {
            get { return _version; }
        }
    }
}