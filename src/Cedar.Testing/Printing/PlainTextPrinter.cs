namespace Cedar.Testing.Printing
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Inflector;

    public class PlainTextPrinter : IScenarioPrinter
    {
        private readonly TextWriter _output;

        public PlainTextPrinter(TextWriter output)
        {
            _output = output;
        }

        public async Task<IDisposable> WriteHeader(string scenarioName, TimeSpan? duration, bool passed)
        {
            await _output.WriteAsync((scenarioName ?? "???")
                .Split('.').Last()
                .Underscore().Titleize());
            await _output.WriteAsync(" - " + (passed ? "PASSED" : "FAILED"));
            await _output.WriteLineAsync(" (completed in " + (duration.HasValue ? duration.Value.TotalMilliseconds + "ms" : "???") + ")");
            await _output.WriteLineAsync();

            return new DisposableAction(async () =>
            {
                await _output.WriteLineAsync(new string('-', 80));
                await _output.WriteLineAsync();
            });
        }

        public Task WriteGiven(object given)
        {
            return WriteSection("Given", given);
        }

        public Task WriteWhen(object when)
        {
            return WriteSection("When", when);
        }

        public Task WriteExpect(object expect)
        {
            return WriteSection("Expect", expect);
        }

        public Task WriteOcurredException(Exception occurredException)
        {
            return WriteSection("Exception", occurredException);
        }

        public async Task WriteStartCategory(string category)
        {
            await _output.WriteLineAsync(new string('=', 80));
            await _output.WriteLineAsync((String.IsNullOrEmpty(category) ? "???" : category).Underscore().Humanize());
            await _output.WriteLineAsync(new string('=', 80));
            await _output.WriteLineAsync();
        }

        public async Task WriteEndCategory(string category)
        {
            await _output.WriteLineAsync();
        }

        public Task Flush()
        {
            return _output.FlushAsync();
        }

        private async Task WriteSection(string sectionName, object section)
        {
            await _output.WriteLineAsync(sectionName + ":");
            await _output.WriteLineAsync(section.NicePrint());
            await _output.WriteLineAsync();
        }
    }
}