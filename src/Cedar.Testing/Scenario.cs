namespace Cedar.Testing
{
    using System;
    using System.Linq.Expressions;

    public static partial class Scenario
    {
        private static void ThenShouldThrow<TException>(this ScenarioResult scenario, object result, Expression<Func<TException, bool>> isMatch = null)
            where TException : Exception
        {
            if(false == result is TException)
            {
                throw new ScenarioException(
                    String.Format(
                        "Expected results to be {0}, got {1} instead.",
                        typeof(TException).FullName,
                        result == null
                            ? "null"
                            : result.GetType().FullName));
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
                throw new ScenarioException(String.Format("{0} was expected yet no exception ocurred.", typeof(TException).FullName));
            }

            if (false == occurredException is TException)
            {
                throw new ScenarioException(String.Format("{0} was expected yet {1} ocurred.", typeof(TException).FullName,
                        occurredException.GetType().FullName));
            }

            if (false == isMatch((TException)occurredException))
            {
                throw new ScenarioException(String.Format("The expected exception type occurred but it did not match the expectation."));
            }
        }
    }
}