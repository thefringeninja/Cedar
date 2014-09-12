namespace Cedar.TypeResolution
{
    public class VersionedName
    {
        private readonly string _name;
        private readonly int? _version;

        public VersionedName(string name, int? version)
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