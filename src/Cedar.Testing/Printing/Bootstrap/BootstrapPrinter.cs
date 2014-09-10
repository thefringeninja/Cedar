namespace Cedar.Testing.Printing.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Inflector;

    public class BootstrapPrinter : IScenarioResultPrinter
    {
        private readonly TextWriter _output;
        private bool _disposed;
        private readonly IList<Tuple<string, List<ScenarioResult>>> _tableOfContents;

        public BootstrapPrinter(Func<string, TextWriter> factory)
        {
            _output = factory(FileExtension);
            _tableOfContents = new List<Tuple<string, List<ScenarioResult>>>();
        }

        public async Task PrintResult(ScenarioResult result)
        {
            _tableOfContents.Last().Item2.Add(result);
        }

        public async Task PrintCategoryHeader(string category)
        {
            _tableOfContents.Add(Tuple.Create(category.Replace('.', ' ').Underscore().Titleize(), new List<ScenarioResult>()));
        }

        public async Task PrintCategoryFooter(string category)
        {}

        public async Task Flush()
        {
            await _output.WriteLineAsync("<!DOCTYPE html>");
            await _output.WriteLineAsync("<html>");

            await WriteHead();
            await WriteBody();

            await _output.WriteLineAsync("</html>");
        }

        public string FileExtension
        {
            get { return "html"; }
        }

        private async Task WriteHead()
        {
            await _output.WriteLineAsync("<head><style>");
            
            var stylesheet = GetType().Assembly.GetManifestResourceStream(GetType(), "bootstrap.css");
            
            using (var reader = new StreamReader(stylesheet))
            {
                await _output.WriteLineAsync(await reader.ReadToEndAsync());
            }
            
            await _output.WriteLineAsync("</style></head>");
        }

        private async Task WriteBody()
        {
            await _output.WriteLineAsync("<body><section class='container'>");
            await WriteNavigation();
            await WriteResults();
            await _output.WriteLineAsync("</section></body>");
        }

        private async Task WriteNavigation()
        {
            await _output.WriteLineAsync("<nav><ul>");
            foreach (var item in _tableOfContents)
            {
                var category = item.Item1;
                var results = item.Item2;
                await _output.WriteLineAsync(
                        String.Format(
                            "<li><a href='#{0}'>{1}</a> ({2})</li>",
                            category.Underscore(), category, results.All(result => result.Passed) ? "PASSED" : "FAILED"));
            }
            await _output.WriteLineAsync("</ul></nav>");
            await _output.WriteLineAsync();
        }

        private async Task WriteResults()
        {
            foreach (var item in _tableOfContents)
            {
                var category = item.Item1;
                var results = item.Item2;

                await WriteCategoryHeader(category);

                foreach (var result in results)
                {
                    await WriteResult(result);
                }

                await WriteCategoryFooter();
            }
        }

        private async Task WriteResult(ScenarioResult result)
        {

            await _output.WriteLineAsync(String.Format("<div class='alert alert-{0}'>", result.Passed ? "success" : "danger"));
            await _output.WriteLineAsync("<details id='{0}'>");
            await _output.WriteLineAsync("<summary>" + (result.Name ?? "???").Underscore().Titleize() + " - " + (result.Passed ? "Passed" : "Failed") + "</summary>");
            await _output.WriteLineAsync("<pre>");
            await WriteGiven(result.Given);
            await WriteWhen(result.When);
            await WriteExpect(result.Expect);
            if (result.OccurredException != null)
            {
                await WriteOcurredException(result.OccurredException);
            }
            await _output.WriteLineAsync("</pre>");
            await _output.WriteLineAsync("</details>");
            await _output.WriteLineAsync("</div>");
        }

        private async Task WriteCategoryHeader(string category)
        {
            await _output.WriteLineAsync(String.Format("<section id='{0}'>", category.Underscore()));
            await _output.WriteLineAsync(String.Format("<h1>{0}</h1>", category));
        }

        private async Task WriteCategoryFooter()
        {
            await _output.WriteLineAsync("</section>");
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

        private async Task WriteOcurredException(Exception occurredException)
        {
            await WriteSection("Exception", occurredException, "");
        }

        private async Task WriteSection(string sectionName, object section, string prefix = "\t")
        {
            await _output.WriteLineAsync(sectionName + ":");
            foreach (var line in section.NicePrint(prefix))
            {
                await _output.WriteLineAsync(line);
            }
            await _output.WriteLineAsync();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _output.Dispose();
        }
    }
}