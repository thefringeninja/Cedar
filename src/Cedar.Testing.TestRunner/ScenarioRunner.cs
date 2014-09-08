namespace Cedar.Testing.TestRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;

    public class ScenarioRunner
    {
        private readonly TestRunnerOptions _options;

        public ScenarioRunner(TestRunnerOptions options)
        {
            _options = options;
        }

        public async Task Run()
        {
            var assembly = await LoadTestAssembly();
            var results = await RunTests(assembly);

            await PrintResults(results.GroupBy(x => x.Key, x => x.Value));
        }

        private async Task<Assembly> LoadTestAssembly()
        {
            var assembly = _options.Assembly;
            
            if (false == Path.HasExtension(assembly))
                assembly = assembly + ".dll";
            
            using (var stream = File.OpenRead(assembly))
            {
                var buffer = new byte[stream.Length];
                
                await stream.ReadAsync(buffer, 0, buffer.Length);

                return Assembly.Load(buffer);
            }
        }

        private bool IsRunningUnderTeamCity
        {
            get { return _options.Teamcity; }
        }

        private TextWriter OutputFactory(string fileExtension)
        {
            if (String.IsNullOrWhiteSpace(_options.Output))
                return new NonClosingTextWriter(Console.Out);

            return File.CreateText(_options.GetOutputWithExtension(fileExtension));
        }

        private IEnumerable<IScenarioResultPrinter> GetPrinters()
        {
            var allPrinters = GetAllPrinters();

            if (IsRunningUnderTeamCity)
            {
                yield return new TeamCityTestServicePrinter(new NonClosingTextWriter(Console.Out));
            }

            foreach (var formatter in _options.Formatters)
            {
                Func<Func<string, TextWriter>, IScenarioResultPrinter> factory;
                if (allPrinters.TryGetValue(formatter + "Printer", out factory))
                {
                    yield return factory(OutputFactory);
                }
            }
        }

        private static IDictionary<string,Func<Func<string, TextWriter>, IScenarioResultPrinter>> GetAllPrinters()
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsClass && false == type.IsAbstract && typeof (IScenarioResultPrinter).IsAssignableFrom(type)
                let constructor = type.GetConstructor(new[] {typeof (Func<string, TextWriter>)})
                where constructor != null
                select new
                {
                    type,
                    constructor
                }).ToDictionary(
                    x => x.type.AssemblyQualifiedName,
                    x => new Func<Func<string, TextWriter>, IScenarioResultPrinter>(
                        factory => (IScenarioResultPrinter) x.constructor.Invoke(new object[] {factory})), PrinterTypeNameEqualityComparer.Instance
                );
        }

        private Task<KeyValuePair<string, ScenarioResult>[]> RunTests(Assembly assembly)
        {
            var scenarios = FindScenarios.InAssemblies(assembly);

            var results = Task.WhenAll(scenarios.Select(RunScenario));

            return results;
        }

        private static async Task<KeyValuePair<string, ScenarioResult>> RunScenario(Func<KeyValuePair<string, Task<ScenarioResult>>> runScenario)
        {
            var groupedScenarioResult = runScenario();

            return new KeyValuePair<string, ScenarioResult>(groupedScenarioResult.Key,
                await groupedScenarioResult.Value.ContinueWith<ScenarioResult>(HandleFailingScenario));
        }

        private static ScenarioResult HandleFailingScenario(Task<ScenarioResult> task)
        {
            if (false == task.IsFaulted)
            {
                return task.Result;
            }

            var exception = task.Exception.InnerException as Scenario.ScenarioException;

            // means something really wrong happened
            if (exception == null)
            {
                return new ScenarioResult(null, true, null, null, null, occurredException: task.Exception.InnerException);
            }

            return exception.ExpectedResult.WithScenarioException(exception);
        }

        private async Task PrintResults(IEnumerable<IGrouping<string, ScenarioResult>> results)
        {
            results = results.ToList();

            foreach (var printer in GetPrinters())
            {
                foreach (var category in results)
                {
                    await printer.PrintCategoryHeader(category.Key);

                    foreach (var result in category)
                    {
                        await printer.PrintResult(result);
                    }

                    await printer.PrintCategoryFooter(category.Key);
                }

                await printer.Flush();
            }
        }

        internal class PrinterTypeNameEqualityComparer : IEqualityComparer<string>
        {
            public static readonly IEqualityComparer<string> Instance = new PrinterTypeNameEqualityComparer();

            private PrinterTypeNameEqualityComparer()
            {
                
            }
            public bool Equals(string x, string y)
            {
                Type a = Type.GetType(x, true, true);

                return a.Name.Equals(y, StringComparison.InvariantCultureIgnoreCase)
                       || a.FullName.Equals(y, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return 0;
            }
        }

        internal class NonClosingTextWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get { return _inner.Encoding; }
            }

            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }

            private readonly TextWriter _inner;

            public NonClosingTextWriter(TextWriter inner)
            {
                _inner = inner;
            }

            public override void Write(bool value)
            {
                _inner.Write(value);
            }

            public override void Write(int value)
            {
                _inner.Write(value);
            }

            public override void Write(uint value)
            {
                _inner.Write(value);
            }

            public override void Write(long value)
            {
                _inner.Write(value);
            }

            public override void Write(ulong value)
            {
                _inner.Write(value);
            }

            public override void Write(float value)
            {
                _inner.Write(value);
            }

            public override void Write(double value)
            {
                _inner.Write(value);
            }

            public override void Write(decimal value)
            {
                _inner.Write(value);
            }

            public override void Write(string value)
            {
                _inner.Write(value);
            }

            public override void Write(object value)
            {
                _inner.Write(value);
            }

            public override void Write(string format, object arg0)
            {
                _inner.Write(format, arg0);
            }

            public override void Write(string format, object arg0, object arg1)
            {
                _inner.Write(format, arg0, arg1);
            }

            public override void Write(string format, object arg0, object arg1, object arg2)
            {
                _inner.Write(format, arg0, arg1, arg2);
            }

            public override void Write(string format, params object[] arg)
            {
                _inner.Write(format, arg);
            }

            public override void WriteLine()
            {
                _inner.WriteLine();
            }

            public override void WriteLine(char value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(char[] buffer)
            {
                _inner.WriteLine(buffer);
            }

            public override void WriteLine(char[] buffer, int index, int count)
            {
                _inner.WriteLine(buffer, index, count);
            }

            public override void WriteLine(bool value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(int value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(uint value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(long value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(ulong value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(float value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(double value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(decimal value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(string value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(object value)
            {
                _inner.WriteLine(value);
            }

            public override void WriteLine(string format, object arg0)
            {
                _inner.WriteLine(format, arg0);
            }

            public override void WriteLine(string format, object arg0, object arg1)
            {
                _inner.WriteLine(format, arg0, arg1);
            }

            public override void WriteLine(string format, object arg0, object arg1, object arg2)
            {
                _inner.WriteLine(format, arg0, arg1, arg2);
            }

            public override void WriteLine(string format, params object[] arg)
            {
                _inner.WriteLine(format, arg);
            }

            public override Task WriteAsync(char value)
            {
                return _inner.WriteAsync(value);
            }

            public override Task WriteAsync(string value)
            {
                return _inner.WriteAsync(value);
            }

            public override Task WriteAsync(char[] buffer, int index, int count)
            {
                return _inner.WriteAsync(buffer, index, count);
            }

            public override Task WriteLineAsync(char value)
            {
                return _inner.WriteLineAsync(value);
            }

            public override Task WriteLineAsync(string value)
            {
                return _inner.WriteLineAsync(value);
            }

            public override Task WriteLineAsync(char[] buffer, int index, int count)
            {
                return _inner.WriteLineAsync(buffer, index, count);
            }

            public override Task WriteLineAsync()
            {
                return _inner.WriteLineAsync();
            }

            public override Task FlushAsync()
            {
                return _inner.FlushAsync();
            }

            public override IFormatProvider FormatProvider
            {
                get { return _inner.FormatProvider; }
            }

            public override string NewLine
            {
                get { return _inner.NewLine; }
                set { _inner.NewLine = value; }
            }


            public override void Flush()
            {
                _inner.Flush();
            }

            public override void Write(char value)
            {
                _inner.Write(value);
            }

            public override void Write(char[] buffer)
            {
                _inner.Write(buffer);
            }

            public override void Write(char[] buffer, int index, int count)
            {
                _inner.Write(buffer, index, count);
            }

            public override string ToString()
            {
                return _inner.ToString();
            }
        }
    
    }
}