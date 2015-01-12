namespace Cedar.Testing.Xunit
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

                var task = (Task<ScenarioResult>)methodInfo.Invoke(testClass, null);

                var scenarioResult = task.Result;

                var methodResult = scenarioResult.Passed
                    ? HandleSuccess(scenarioResult)
                    : HandleFailure(scenarioResult);
                
                Trace.Write(methodResult.Output);
                
                return methodResult;
            }

            private MethodResult HandleSuccess(ScenarioResult scenarioResult)
            {
                return new PassedResult(_inner.MethodName, _inner.TypeName, _inner.DisplayName,
                    new MultiValueDictionary<string, string>())
                {
                    Output = PrintResult(scenarioResult)
                };
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

            private static string PrintResult(ScenarioResult scenarioResult)
            {
                var builder = new StringBuilder();

                using (var printer = new PlainTextPrinter(_ => new StringWriter(builder)))
                {
                    printer.PrintResult(scenarioResult).Wait();
                }

                return builder.ToString();
            }

            private MethodResult HandleFailure(ScenarioResult scenarioResult)
            {
                var exception = scenarioResult.Results as Exception
                    ?? new Exception();

                return new FailedResult(_method, exception, DisplayName)
                {
                    Output = PrintResult(scenarioResult)
                };
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