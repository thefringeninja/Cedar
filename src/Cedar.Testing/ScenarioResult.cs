namespace Cedar.Testing
{
    using System;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;

    public class ScenarioResult
    {
        private readonly string _name;
        private readonly object _given;
        private readonly object _when;
        private readonly object _expect;
        private readonly Exception _occurredException;
        private readonly TimeSpan? _duration;
        private readonly bool _passed;

        public ScenarioResult(string name, bool passed, object given, object when, object expect, TimeSpan? duration = null, Exception occurredException = null)
        {
            _name = name;
            _given = given;
            _when = when;
            _expect = expect;
            _occurredException = occurredException;
            _duration = duration;
            _passed = passed;
        }

        public async Task Print(IScenarioPrinter printer)
        {
            using (await printer.WriteHeader(_name, _duration, _passed))
            {
                await printer.WriteGiven(_given);
                await printer.WriteWhen(_when);
                await printer.WriteExpect(_expect);
                if (_occurredException != null)
                    await printer.WriteOcurredException(_occurredException);
            }
        }

        public ScenarioResult WithScenarioException(Scenario.ScenarioException ex)
        {
            return new ScenarioResult(_name, _passed, _given, _when, _expect, _duration, ex);
        }
    }
}