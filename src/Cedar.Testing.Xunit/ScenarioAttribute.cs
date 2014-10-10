namespace Cedar.Testing.Xunit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
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

                var task = (Task<ScenarioResult>) methodInfo.Invoke(testClass, null);

                var scenarioResult = task.Result;

                if(scenarioResult.Passed)
                {
                    return new PassedResult(_inner.MethodName, _inner.TypeName, _inner.DisplayName,
                        new MultiValueDictionary<string, string>());
                }

                FailedResult xunitResult;
                if(false == TryHandleException(scenarioResult, out xunitResult))
                {
                    xunitResult = new FailedResult(_method, new Exception(), DisplayName);
                }

                return xunitResult;
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

            private bool TryHandleException(ScenarioResult scenarioResult, out FailedResult xunitResult)
            {
                xunitResult = null;

                var ex = scenarioResult.Results as Exception;

                if(ex == null)
                {
                    return false;
                }

                xunitResult = new FailedResult(_method, ex, DisplayName);

                return true;
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