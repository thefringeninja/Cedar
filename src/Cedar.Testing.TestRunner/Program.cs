namespace Cedar.Testing.TestRunner
{
    using System;
    using PowerArgs;

    class Program
    {
        static void Main(string[] args)
        {
            var options = Args.Parse<TestRunnerOptions>(args);

            if (options.Help || String.IsNullOrEmpty(options.Assembly))
            {
                Console.WriteLine(ArgUsage.GetUsage<TestRunnerOptions>(options: new ArgUsageOptions
                {
                    ShowType = false,
                    ShowPosition = false,
                    ShowPossibleValues = false,
                    AppendDefaultValueToDescription = true
                }));
                return;
            }

            var runner = new ScenarioRunner(options);

            runner.Run().Wait();
        }
    }
}
