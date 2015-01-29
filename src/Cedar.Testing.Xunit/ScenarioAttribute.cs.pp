// ReSharper disable CheckNamespace
namespace $rootnamespace$.XunitSupport
// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Cedar.Testing;
    using Cedar.Testing.Printing.PlainText;
    using global::Xunit;
    using global::Xunit.Sdk;

    public class ScenarioAttribute
        : FactAttribute
    {
        private class CedarTestCommand : ITestCommand
        {
            private readonly FactCommand _inner;
            private readonly IMethodInfo _method;

            public CedarTestCommand(FactCommand inner, IMethodInfo method)
            {
                _inner = inner;
                _method = method;
            }

            public MethodResult Execute(object testClass)
            {
                var methodInfo = testClass.GetType()
                    .GetMethod(_inner.MethodName);

                var testRunResults = methodInfo.Invoke(testClass, null);

                var scenarioResults = GetScenarioResults(testRunResults).ToArray();

                var methodResult = scenarioResults.All(result => result.Passed)
                    ? HandleSuccess(scenarioResults)
                    : HandleFailure(scenarioResults);

                Trace.Write(methodResult.Output);

                return methodResult;
            }

            public XmlNode ToStartXml()
            {
                return _inner.ToStartXml();
            }

            public string DisplayName
            {
                get { return _inner.DisplayName; }
            }

            public bool ShouldCreateInstance
            {
                get { return _inner.ShouldCreateInstance; }
            }

            public int Timeout
            {
                get { return _inner.Timeout; }
            }

            private IEnumerable<ScenarioResult> GetScenarioResults(object testRunResult)
            {
                if(testRunResult == null)
                {
                    throw new ArgumentNullException("testRunResult");
                }

                var single = testRunResult as Task<ScenarioResult>;

                if(single != null)
                {
                    yield return single.Result;
                    yield break;
                }

                var multiple = testRunResult as IEnumerable<Task<ScenarioResult>>;

                if(multiple != null)
                {
                    var results = Task.WhenAll(multiple);

                    foreach(var result in results.Result)
                    {
                        yield return result;
                    }
                    yield break;
                }

                throw new InvalidOperationException(
                    string.Format("Expected the result of the test run to be {0} or {1}. Received {2} instead.",
                        typeof(Task<ScenarioResult>).FullName,
                        typeof(IEnumerable<Task<ScenarioResult>>).FullName,
                        testRunResult.GetType().FullName));
            }

            private MethodResult HandleSuccess(params ScenarioResult[] scenarioResults)
            {
                return new PassedResult(_inner.MethodName,
                    _inner.TypeName,
                    _inner.DisplayName,
                    new MultiValueDictionary<string, string>())
                {
                    Output = PrintResults(scenarioResults)
                };
            }

            private MethodResult HandleFailure(params ScenarioResult[] scenarioResults)
            {
                var exception = new AggregateException(from result in scenarioResults
                    let ex = result.Results as Exception
                    where ex != null
                    select ex);

                return new FailedResult(_method, exception, DisplayName)
                {
                    Output = PrintResults(scenarioResults)
                };
            }

            private static string PrintResults(params ScenarioResult[] scenarioResult)
            {
                var builder = new StringBuilder();

                using(var printer = new PlainTextPrinter(_ => new StringWriter(builder)))
                {
                    Task.WhenAll(scenarioResult.Select(printer.PrintResult)).Wait();
                }

                return builder.ToString();
            }
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method)
                .OfType<FactCommand>()
                .Select(command => new CedarTestCommand(command, method));
        }
    }
}