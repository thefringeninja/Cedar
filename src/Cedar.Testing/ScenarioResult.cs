namespace Cedar.Testing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;

    public class ScenarioResult
    {
        private readonly string _name;
        private readonly object _given;
        private readonly object _when;
        private readonly object _expect;
        private readonly Exception _occurredException;

        public ScenarioResult(string name, object given, object when, object expect, Exception occurredException = null)
        {
            _name = name;
            _given = given;
            _when = when;
            _expect = expect;
            _occurredException = occurredException;
        }

        public async Task Print(TextWriter writer, IScenarioFormatter formatter)
        {
            await formatter.WriteHeader(writer);
            await formatter.WriteScenarioName(_name, writer);
            await formatter.WriteGiven(_given, writer);
            await formatter.WriteWhen(_when, writer);
            await formatter.WriteExpect(_expect, writer);
            if (_occurredException != null)
                await formatter.WriteOcurredException(_occurredException, writer);
            await formatter.WriteFooter(writer);
        }

        public ScenarioResult WithScenarioException(Scenario.ScenarioException ex)
        {
            return new ScenarioResult(_name, _given, _when, _expect, ex);
        }
    }
}