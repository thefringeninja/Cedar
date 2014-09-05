namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;

    public class ScenarioResult
    {
        public readonly string Name;
        public readonly object Given;
        public readonly object When;
        public readonly object Expect;
        public readonly Exception OccurredException;
        public readonly TimeSpan? Duration;
        public readonly bool Passed;

        public ScenarioResult(string name, bool passed, object given, object when, object expect, TimeSpan? duration = null, Exception occurredException = null)
        {
            Name = name;
            Given = given;
            When = when;
            Expect = expect;
            OccurredException = occurredException;
            Duration = duration;
            Passed = passed;
        }

        public async Task Print(IScenarioResultPrinter printer)
        {
            await printer.PrintResult(this);
        }

        public ScenarioResult WithScenarioException(Scenario.ScenarioException ex)
        {
            return new ScenarioResult(Name, Passed, Given, When, Expect, Duration, ex);
        }
    }
}