namespace Cedar.Testing
{
    using System;
    using System.IO;
    using Cedar.Testing.TestRunner;
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
    }
}