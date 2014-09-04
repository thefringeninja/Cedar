namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public interface IScenario
    {
        string Name { get; }
        
        Task<ScenarioResult> Run();

        TaskAwaiter<ScenarioResult> GetAwaiter();
    }

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
    }

    public class ScenarioFailedException : Exception
    {
        public ScenarioFailedException(string scenarioName, Exception innerException)
            : base(string.Format("The scenario {0} failed to run. Please see the InnerException for details", scenarioName), innerException)
        {
            
        }
    }
}