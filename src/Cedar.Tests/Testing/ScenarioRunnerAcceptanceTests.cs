namespace Cedar.Testing
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.Testing.Execution;
    using Cedar.Testing.TestRunner;
    using FluentAssertions;
    using Xunit;

    public class ScenarioRunnerAcceptanceTests
    {
        [Fact]
        public void can_run_tests_from_separate_app_domain()
        {
            var program = new Program(new TestRunnerOptions
            {
                Assembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cedar.Tests.dll")
            });

            program.Run();
        }

        [Fact]
        public async Task returns_an_enumerable_of_scenario_results()
        {
            var results = await new ScenarioRunner(typeof(EnumerableTests).Assembly, false, "what", "plain").RunTests();

            results.Count().Should().Be(5);
        }
    }
}