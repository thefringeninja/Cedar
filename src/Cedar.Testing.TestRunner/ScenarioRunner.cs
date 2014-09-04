namespace Cedar.Testing.TestRunner
{
    using System;
    using System.Collections.Generic;
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
            var assembly = await _options.LoadAssembly();
            var results = await RunTests(assembly);
            await PrintResults(results);
        }

        private Task<ScenarioResult[]> RunTests(Assembly assembly)
        {
            var scenarios = FindScenarios.InAssemblies(assembly);

            var results = Task.WhenAll(scenarios.Select(RunScenario));

            return results;
        }

        private static Task<ScenarioResult> RunScenario(Func<Task<ScenarioResult>> run)
        {
            return run().ContinueWith<ScenarioResult>(HandleFailingScenario);
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
                return new ScenarioResult(null, null, null, null, task.Exception.InnerException);
            }

            return exception.ExpectedResult.WithScenarioException(exception);
        }

        private async Task PrintResults(IEnumerable<ScenarioResult> results)
        {
            var formatter = new PlainTextFormatter();

            foreach (var result in results)
            {
                await result.Print(Console.Out, formatter);
            }
            
        }
    }
}