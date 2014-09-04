namespace Cedar.Testing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

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

        public Task Print(Stream stream)
        {
            throw new NotImplementedException();
        }

        public ScenarioResult WithScenarioException(Scenario.ScenarioException ex)
        {
            return new ScenarioResult(_name, _given, _when, _expect, ex);
        }
    }
}