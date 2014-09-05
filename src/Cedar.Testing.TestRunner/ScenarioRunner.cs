namespace Cedar.Testing.TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;

    public class ScenarioRunner
    {
        private readonly TestRunnerOptions _options;

        public ScenarioRunner(TestRunnerOptions options)
        {
            _options = options;
        }

        public async Task Run()
        {
            var assembly = await LoadTestAssembly();
            var results = await RunTests(assembly);

            await PrintResults(results.GroupBy(x => x.Key, x => x.Value));
        }

        private async Task<Assembly> LoadTestAssembly()
        {
            var assembly = _options.Assembly;
            
            if (false == Path.HasExtension(assembly))
                assembly = assembly + ".dll";
            
            using (var stream = File.OpenRead(assembly))
            {
                var buffer = new byte[stream.Length];
                
                await stream.ReadAsync(buffer, 0, buffer.Length);

                return Assembly.Load(buffer);
            }
        }

        private bool IsRunningUnderTeamCity
        {
            get { return _options.Teamcity; }
        }

        private IEnumerable<IScenarioResultPrinter> GetPrinters()
        {
            if (IsRunningUnderTeamCity)
            {
                yield return new TeamCityTestServicePrinter();
            }

            yield return new PlainTextPrinter(Console.Out);
        }

        private Task<KeyValuePair<string, ScenarioResult>[]> RunTests(Assembly assembly)
        {
            var scenarios = FindScenarios.InAssemblies(assembly);

            var results = Task.WhenAll(scenarios.Select(RunScenario));

            return results;
        }

        private static async Task<KeyValuePair<string, ScenarioResult>> RunScenario(Func<KeyValuePair<string, Task<ScenarioResult>>> run)
        {
            var pair = run();

            return new KeyValuePair<string, ScenarioResult>(pair.Key,
                await pair.Value.ContinueWith<ScenarioResult>(HandleFailingScenario));
        }

        private static ScenarioResult HandleFailingScenario(Task<ScenarioResult> task)
        {
            if (false == task.IsFaulted)
            {
                return task.Result;
            }

            var exception = task.Exception.InnerException as Scenario.ScenarioException;

            // means something really wrong happened
            if (exception == null)
            {
                return new ScenarioResult(null, true, null, null, null, occurredException: task.Exception.InnerException);
            }

            return exception.ExpectedResult.WithScenarioException(exception);
        }

        private async Task PrintResults(IEnumerable<IGrouping<string, ScenarioResult>> results)
        {
            results = results.ToList();

            foreach (var printer in GetPrinters())
            {
                foreach (var category in results)
                {
                    await printer.PrintCategoryHeader(category.Key);

                    foreach (var result in category)
                    {
                        await printer.PrintResult(result);
                    }

                    await printer.PrintCategoryFooter(category.Key);
                }
            }

        }
    }
}