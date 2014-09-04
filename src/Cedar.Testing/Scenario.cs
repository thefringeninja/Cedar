namespace Cedar.Testing
{
    using System;

    public static partial class Scenario
    {
        public static void AssertExceptionMatches<TException>(this ScenarioResult result, Exception occurredException, Func<TException, bool> isMatch) where TException : Exception
        {
            if (occurredException == null)
            {
                throw new ScenarioException(result,
                    String.Format("{0} was expected yet no exception ocurred.", typeof(TException).FullName));
            }

            if (false == occurredException is TException)
            {
                throw new ScenarioException(result,
                    String.Format("{0} was expected yet {1} ocurred.", typeof(TException).FullName,
                        occurredException.GetType().FullName));
            }

            if (false == isMatch((TException)occurredException))
            {
                throw new ScenarioException(result,
                    String.Format("The expected exception type occurred but it did not match the expectation."));
            }
        }

        public class FatalScenarioException : Exception
        {
            public FatalScenarioException(string scenarioName, Exception innerException)
                : base(
                    String.Format("Scenario {0} failed to run. Please see the InnerException for details.",
                        scenarioName), innerException)
            {}
        }

        public class ScenarioException : Exception
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