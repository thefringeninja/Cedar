namespace Cedar.Testing
{
    using System;

    public class FatalScenarioException : Exception
    {
        public FatalScenarioException(string scenarioName, Exception innerException)
            : base(
                String.Format("Scenario {0} failed to run. Please see the InnerException for details.",
                    scenarioName), innerException)
        { }
    }
}