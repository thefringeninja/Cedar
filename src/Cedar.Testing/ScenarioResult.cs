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
        public readonly TimeSpan? Duration;
        public readonly bool Passed;
        public readonly object Results;

        public ScenarioResult(string name, bool passed, object given, object when, object expect, object results, TimeSpan? duration = null)
        {
            Name = name;
            Given = given;
            When = when;
            Expect = expect;
            Duration = duration;
            Passed = passed;
            Results = results;
        }

        public async Task Print(IScenarioResultPrinter printer)
        {
            await printer.PrintResult(this);
        }
    }
}