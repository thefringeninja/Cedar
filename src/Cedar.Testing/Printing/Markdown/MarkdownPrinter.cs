namespace Cedar.Testing.Printing.Markdown
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Inflector;

    public class MarkdownPrinter : IScenarioResultPrinter
    {
        private readonly TextWriter _output;
        private bool _disposed;

        public MarkdownPrinter(Func<string, TextWriter> factory)
        {
            _output = factory(FileExtension);
        }

        public async Task PrintResult(ScenarioResult result)
        {
            await WriteHeader(result.Name, result.Duration, result.Passed);
            await WriteGiven(result.Given);
            await WriteWhen(result.When);
            await WriteExpect(result.Expect);
            await WriteResults(result.Results);
            await WriteFooter();

            await _output.FlushAsync();
        }

        public Task Flush()
        {
            return _output.FlushAsync();
        }

        public string FileExtension { get { return "md"; } }

        private async Task WriteHeader(string scenarioName, TimeSpan? duration, bool passed)
        {
            await _output.WriteAsync("###" + (scenarioName ?? "???")
                .Split('.').Last()
                .Underscore().Titleize());
            await _output.WriteAsync(" - " + (passed ? "PASSED" : "FAILED"));
            await _output.WriteLineAsync(" (completed in "
                                         + (duration.HasValue
                                             ? duration.Value.TotalMilliseconds + "ms"
                                             : "???")
                                         + ")");
            await _output.WriteLineAsync();
        }

        private async Task WriteFooter()
        {
            await _output.WriteLineAsync(new string('-', 80));
            await _output.WriteLineAsync();
        }

        private Task WriteGiven(object given)
        {
            return WriteSection("Given", given);
        }

        private Task WriteWhen(object when)
        {
            return WriteSection("When", when);
        }

        private Task WriteExpect(object expect)
        {
            return WriteSection("Expect", expect);
        }
        
        private async Task WriteResults(object results)
        {
            await _output.WriteLineAsync("<pre>");
            await WriteSection("Results", results);
            await _output.WriteLineAsync("</pre>");
        }

        public async Task PrintCategoryHeader(string category)
        {
            await _output.WriteLineAsync("##" + (String.IsNullOrEmpty(category) ? "???" : category).Replace('.', ' ').Underscore().Humanize());
            await _output.WriteLineAsync();
        }

        public async Task PrintCategoryFooter(string category)
        {
            await _output.WriteLineAsync();
        }

        private async Task WriteSection(string sectionName, object section, string prefix = "- ")
        {
            await _output.WriteLineAsync("####" + sectionName + ":");
            foreach (var line in section.NicePrint(prefix))
                await _output.WriteLineAsync(line);
            await _output.WriteLineAsync();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _output.Dispose();
        }
    }
}