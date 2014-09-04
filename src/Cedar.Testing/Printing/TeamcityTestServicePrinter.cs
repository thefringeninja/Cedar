namespace Cedar.Testing.Printing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class TeamCityTestServicePrinter : IScenarioPrinter
    {
        private readonly Stack<Tuple<string, TimeSpan?, bool>> _state = new Stack<Tuple<string, TimeSpan?, bool>>();

        private const string TeamCityServiceMessageFormat = "###teamcity[{0} {1}]";

        private static string Started(string name)
        {
            return String.Format(TeamCityServiceMessageFormat, "testStarted", String.Format("name='{0}'", name));
        }

        private static string Failed(string name, Exception exception)
        {
            return String.Format(TeamCityServiceMessageFormat, "testFailed",
                String.Format("name='{0}' message='{1}' details='{2}'", name, exception.Message, exception));
        }

        private static string Finished(string name, TimeSpan? duration)
        {
            return String.Format(TeamCityServiceMessageFormat, "testFinished",
                String.Format("name='{0}' duration='{1}'", name,
                    duration.HasValue ? (int) duration.Value.TotalMilliseconds : -1));
        }

        private static string SuiteStarted(string name)
        {
            return String.Format(TeamCityServiceMessageFormat, "testSuiteStarted", String.Format("name='{0}'", name));
        }

        private static string SuiteFinished(string name)
        {
            return String.Format(TeamCityServiceMessageFormat, "testSuiteFinished", String.Format("name='{0}'", name));
        }

        private readonly TextWriter _output;

        public TeamCityTestServicePrinter()
        {
            _output = Console.Out;
        }

        public async Task<IDisposable> WriteHeader(string scenarioName, TimeSpan? duration, bool passed)
        {
            // yes, this is awful
            _state.Push(Tuple.Create(scenarioName, duration, passed));
            await _output.WriteLineAsync(Started(scenarioName));

            return new DisposableAction(async () =>
            {
                _state.Pop();
                await _output.WriteLineAsync(Finished(scenarioName, duration));
            });
        }

        public Task WriteGiven(object given)
        {
            return Task.FromResult(true);
        }

        public Task WriteWhen(object when)
        {
            return Task.FromResult(true);
        }

        public Task WriteExpect(object expect)
        {
            return Task.FromResult(true);
        }

        public async Task WriteOcurredException(Exception occurredException)
        {
            var current = _state.Peek();

            if (false == current.Item3)
            {
                await _output.WriteLineAsync(Failed(current.Item1, occurredException));
            }
        }

        public Task WriteStartCategory(string category)
        {
            return _output.WriteLineAsync(SuiteStarted(category));
        }

        public Task WriteEndCategory(string category)
        {
            return _output.WriteLineAsync(SuiteFinished(category));
        }

        public Task Flush()
        {
            return _output.FlushAsync();
        }
    }
}