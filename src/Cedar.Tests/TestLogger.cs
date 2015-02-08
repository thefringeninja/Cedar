namespace Cedar
{
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    internal static class TestLogger
    {
        internal static void Configure()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}"
            };
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

            LogManager.Configuration = config;
        }
    }
}