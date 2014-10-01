namespace Cedar.Testing
{
    using System;

    public class ScenarioException : Exception
    {
        public ScenarioException(string reason = null)
            : base("The scenario failed: " + (reason ?? "No reason given."))
        {}
    }
}