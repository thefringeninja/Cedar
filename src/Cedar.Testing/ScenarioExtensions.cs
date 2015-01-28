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
        internal static void ScenarioFailedIfAny(this IEnumerable<LambdaExpression> failed)
        {
            failed = failed.ToList();

            if(failed.Any())
            {
                throw new ScenarioException("The following assertions failed:" + Environment.NewLine + failed.Aggregate(
                    new StringBuilder(),
                    (builder, assertion) =>
                        builder.Append('\t').Append(PAssertFormatter.CreateSimpleFormatFor(assertion)).AppendLine()));
            }
        }

        public static Scenario.Middleware.IThen When(
            this Scenario.Middleware.IWhen scenario,
            Func<Task<HttpRequestMessage>> request)
        {
            return scenario.When(_ => request());
        }

        public static Scenario.Middleware.IThen When(
            this Scenario.Middleware.IWhen scenario,
            HttpRequestMessage when)
        {
            return scenario.When(_ => when);
        }

        public static Scenario.Middleware.IThen When(
            this Scenario.Middleware.IWhen scenario,
            Func<HttpRequestMessage> when)
        {
            return scenario.When(_ => when());
        }

        public static Scenario.Middleware.IThen When(
            this Scenario.Middleware.IWhen scenario, 
            Func<HttpResponseMessage, HttpRequestMessage> when)
        {
            return scenario.When(response => Task.FromResult(when(response)));
        }

        public static Scenario.Query.IThen When(
            this Scenario.Query.IWhen scenario,
            HttpRequestMessage when)
        {
            return scenario.When(() => Task.FromResult(when));
        }
    }
}