namespace Cedar.Testing.Printing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class PlainTextFormatter : IScenarioFormatter
    {
        public Task WriteHeader(TextWriter writer)
        {
            return Task.FromResult(true);
        }

        public async Task WriteFooter(TextWriter writer)
        {
            await writer.WriteLineAsync(new string('-', 80));
            await writer.WriteLineAsync();
        }

        public async Task WriteScenarioName(string name, TextWriter writer)
        {
            await writer.WriteLineAsync(name.Underscore().Titleize());
            await writer.WriteLineAsync();
        }

        public Task WriteGiven(object given, TextWriter writer)
        {
            return WriteSection("Given", given, writer);
        }

        public Task WriteWhen(object when, TextWriter writer)
        {
            return WriteSection("When", when, writer);
        }

        public Task WriteExpect(object expect, TextWriter writer)
        {
            return WriteSection("Expect", expect, writer);
        }

        public Task WriteOcurredException(Exception occurredException, TextWriter writer)
        {
            return WriteSection("Exception", occurredException, writer);
        }

        private static async Task WriteSection(string sectionName, object section, TextWriter writer)
        {
            await writer.WriteLineAsync(sectionName + ":");
            await writer.WriteLineAsync(section.NicePrint());
            await writer.WriteLineAsync();
        }
    }
}