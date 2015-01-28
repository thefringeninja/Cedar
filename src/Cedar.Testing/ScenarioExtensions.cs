namespace Cedar.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using PowerAssert;

    public static class ScenarioExtensions
    {


        public static Scenario.Query.IThen When(
            this Scenario.Query.IWhen scenario,
            HttpRequestMessage when)
        {
            return scenario.When(() => Task.FromResult(when));
        }
    }
}