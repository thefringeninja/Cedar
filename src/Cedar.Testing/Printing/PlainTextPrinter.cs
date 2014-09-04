namespace Cedar.Testing.Printing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class PlainTextPrinter : IScenarioPrinter
    {
        private readonly TextWriter _output;

        public PlainTextPrinter(TextWriter output)
        {
            _output = output;
        }

        public Task WriteHeader()
        {
            return Task.FromResult(true);
        }

        public async Task WriteFooter()
        {
            await _output.WriteLineAsync(new string('-', 80));
            await _output.WriteLineAsync();
        }

        public async Task WriteScenarioName(string name, bool passed)
        {
            await _output.WriteLineAsync((name ?? "???").Underscore().Titleize() + " - " + (passed ? "PASSED" : "FAILED"));
            await _output.WriteLineAsync();
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

        private async Task WriteSection(string sectionName, object section)
        {
            await _output.WriteLineAsync(sectionName + ":");
            await _output.WriteLineAsync(section.NicePrint());
            await _output.WriteLineAsync();
        }
    }
}