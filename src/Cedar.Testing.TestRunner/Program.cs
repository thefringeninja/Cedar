namespace Cedar.Testing.TestRunner
{
    using System;
    using System.Reflection;
    using Cedar.Testing.Execution;
    using PowerArgs;

    public class Program
    {
        private readonly AppDomain _appDomain;
        private readonly TestRunnerOptions _options;

        public Program(TestRunnerOptions options)
        {
            _options = options;

            _appDomain = TestAppDomain.Create(options.Assembly);
        }

        public void Run()
        {
            var runner = (IScenarioRunner) _appDomain.CreateInstanceAndUnwrap(
                typeof(IScenarioRunner).Assembly.FullName,
                typeof(ScenarioRunner).FullName,
                true,
                BindingFlags.Default,
                null,
                new object[] { _options.Assembly, _options.Teamcity, _options.Output, _options.Formatters },
                null,
                null);

            runner.Run();
        }

        private static void Main(string[] args)
        {
            var options = Args.Parse<TestRunnerOptions>(args);

            if(options.Help || String.IsNullOrEmpty(options.Assembly))
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

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                Console.WriteLine(e.ExceptionObject);

                Environment.Exit(1);
            };

            var program = new Program(options);
            program.Run();
        }
    }
}