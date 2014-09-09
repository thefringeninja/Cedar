namespace Cedar.Testing.Printing.TeamCity
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class TeamCityPrinter : IScenarioResultPrinter
    {
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
        private bool _disposed;

        public TeamCityPrinter(TextWriter output)
        {
            _output = output;
        }

        public Task Flush()
        {
            return _output.FlushAsync();
        }

        public Task PrintCategoryFooter(string category)
        {
            return _output.WriteLineAsync(SuiteFinished(category));
        }

        public Task PrintCategoryHeader(string category)
        {
            return _output.WriteLineAsync(SuiteStarted(category));
        }

        public async Task PrintResult(ScenarioResult result)
        {
            await _output.WriteLineAsync(Started(result.Name));

            if (false == result.Passed)
            {
                await _output.WriteLineAsync(Failed(result.Name, result.OccurredException));
            }

            await _output.WriteLineAsync(Finished(result.Name, result.Duration));

            await _output.FlushAsync();
        }

        public string FileExtension { get { return "teamcity"; } }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _output.Dispose();
        }
    }
}