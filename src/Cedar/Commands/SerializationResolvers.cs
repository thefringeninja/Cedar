namespace Cedar.Commands
{
    using System;
    using Cedar.Serialization;

    public static class SerializationResolvers
    {
        public static ResolveSerializer Default()
        {
            //TODO support xml?
            return name => 
                string.Equals(name, "json", StringComparison.OrdinalIgnoreCase) 
                    ? new DefaultJsonSerializer() 
                    : null;
        }
    }
}