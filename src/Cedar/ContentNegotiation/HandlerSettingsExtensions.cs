namespace Cedar.ContentNegotiation
{
    using System.IO;

    public static class HandlerSettingsExtensions
    {
        public static string Serialize(this HandlerSettings options, object target)
        {
            using (var writer = new StringWriter())
            {
                options.Serialize(writer, target);

                return writer.ToString();
            }
        }
    }
}