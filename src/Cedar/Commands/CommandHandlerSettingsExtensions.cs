namespace Cedar.Commands
{
    using System.IO;

    public static class CommandHandlerSettingsExtensions
    {
        public static string Serialize(this CommandHandlerSettings options, object target)
        {
            using (var writer = new StringWriter())
            {
                options.Serialize(writer, target);

                return writer.ToString();
            }
        }
    }
}