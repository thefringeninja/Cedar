namespace Cedar.Testing.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing;
    using Cedar.Testing.Printing.TeamCity;

    public class ScenarioRunner : MarshalByRefObject, IScenarioRunner
    {
        private readonly string _assembly;
        private readonly bool _isRunningUnderTeamCity;
        private readonly string _output;
        private readonly string[] _formatters;

        public ScenarioRunner(string assembly, bool isRunningUnderTeamCity, string output, params string[] formatters)
        {
            _assembly = assembly;
            _isRunningUnderTeamCity = isRunningUnderTeamCity;
            _output = output;
            _formatters = formatters;
        }

        public void Run()
        {
            RunInternal().Wait();
        }

        private async Task RunInternal()
        {
            var assembly = await LoadTestAssembly();
            var results = await RunTests(assembly);
            await PrintResults(results.GroupBy(x => x.Key, x => x.Value));
        }

        private async Task<Assembly> LoadTestAssembly()
        {
            var assembly = _assembly;

            if(false == Path.HasExtension(assembly))
            {
                assembly = assembly + ".dll";
            }
            
            using (var stream = File.OpenRead(assembly))
            {
                var buffer = new byte[stream.Length];
                
                await stream.ReadAsync(buffer, 0, buffer.Length);

                return Assembly.Load(buffer);
            }
        }

        private bool IsRunningUnderTeamCity
        {
            get { return _isRunningUnderTeamCity; }
        }

        private TextWriter OutputFactory(string fileExtension)
        {
            if (String.IsNullOrWhiteSpace(_output))
                return new NonClosingTextWriter(Console.Out);

            var outputFile = GetOutputWithExtension(fileExtension);

            return File.CreateText(outputFile);
        }

        private string GetOutputWithExtension(string fileExtension)
        {
            if(String.IsNullOrWhiteSpace(_output))
            {
                throw new InvalidOperationException();
            }

            return Path.ChangeExtension(Path.Combine(_output, Path.GetFileName(_assembly)), fileExtension);
        }

        private IEnumerable<IScenarioResultPrinter> GetPrinters()
        {
            var printerFactories = GetAllPrinterFactories();

            if (IsRunningUnderTeamCity)
            {
                yield return new TeamCityPrinter(new NonClosingTextWriter(Console.Out));
            }

            foreach (var formatter in _formatters)
            {
                Func<Func<string, TextWriter>, IScenarioResultPrinter> factory;
                if (printerFactories.TryGetValue(formatter + "Printer", out factory))
                {
                    yield return factory(OutputFactory);
                }
            }
        }

        private static IDictionary<string,Func<Func<string, TextWriter>, IScenarioResultPrinter>> GetAllPrinterFactories()
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
                await groupedScenarioResult.Value);
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