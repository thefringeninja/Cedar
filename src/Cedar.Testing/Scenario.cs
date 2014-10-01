namespace Cedar.Testing
{
    using System;
    using System.Linq.Expressions;

    public static partial class Scenario
    {
        public class FatalScenarioException : Exception
        {
            public FatalScenarioException(string scenarioName, Exception innerException)
                : base(
                    String.Format("Scenario {0} failed to run. Please see the InnerException for details.",
                        scenarioName), innerException)
            { }
        }

        private static void ThenShouldThrow<TException>(this ScenarioResult scenario, object result, Expression<Func<TException, bool>> isMatch = null)
            where TException : Exception
        {
            if(false == result is TException)
            {
                throw new ScenarioException(scenario, "Expected results to be {0}, got {1} instead.");
            }
            if(isMatch != null)
            {
                scenario.AssertExceptionMatches((TException)result, isMatch.Compile());
            }
        }

        private static void AssertExceptionMatches<TException>(this ScenarioResult scenarioResult, Exception occurredException, Func<TException, bool> isMatch) where TException : Exception
        {
            if (occurredException == null)
            {
                throw new ScenarioException(scenarioResult,
                    String.Format("{0} was expected yet no exception ocurred.", typeof(TException).FullName));
            }

            if (false == occurredException is TException)
            {
                throw new ScenarioException(scenarioResult,
                    String.Format("{0} was expected yet {1} ocurred.", typeof(TException).FullName,
                        occurredException.GetType().FullName));
            }

            if (false == isMatch((TException)occurredException))
            {
                throw new ScenarioException(scenarioResult,
                    String.Format("The expected exception type occurred but it did not match the expectation."));
            }
        }

        private class ScenarioException : Exception
        {
            public ScenarioResult ExpectedResult
            {
                get { return _expectedResult; }
            }

            private readonly ScenarioResult _expectedResult;

            public ScenarioException(ScenarioResult expectedResult, string reason = null)
                : base("The scenario failed: " + (reason ?? "No reason given."))
            {
                _expectedResult = expectedResult;
            }
        }
    }
}