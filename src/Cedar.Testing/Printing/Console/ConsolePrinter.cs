namespace Cedar.Testing.Printing.Console
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing.PlainText;

    public class ConsolePrinter : IScenarioResultPrinter
    {
        private readonly IScenarioResultPrinter _inner;
        private bool _disposed;

        public ConsolePrinter(Func<string, TextWriter> _)
        {
            _inner = new PlainTextPrinter(_ => Console.Out);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Flush().Wait();
        }

        public Task PrintCategoryFooter(string category)
        {
            return _inner.PrintCategoryFooter(category);
        }

        public Task PrintCategoryHeader(string category)
        {
            return _inner.PrintCategoryHeader(category);
        }

        public Task PrintResult(ScenarioResult result)
        {
            return _inner.PrintResult(result);
        }

        public Task Flush()
        {
            return _inner.Flush();
        }

        public string FileExtension
        {
            get { return String.Empty; }
        }
    }
}