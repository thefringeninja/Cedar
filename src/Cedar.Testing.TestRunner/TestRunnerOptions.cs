namespace Cedar.Testing.TestRunner
{
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using PowerArgs;

    public class TestRunnerOptions
    {
        [ArgDescription("Show help."), ArgShortcut("?")]
        public bool Help { get; set; }

        [ArgDescription("The assembly to test.")]
        [ArgPosition(0)]
        public string Assembly { get; set; }

        [ArgDescription("Force teamcity output.")]
        public bool Teamcity { get; set; }

        public async Task<Assembly> LoadAssembly()
        {
            var assembly = Assembly;
            
            if (false == Path.HasExtension(assembly))
                assembly = assembly + ".dll";
            
            using (var stream = File.OpenRead(assembly))
            {
                var buffer = new byte[stream.Length];
                
                await stream.ReadAsync(buffer, 0, buffer.Length);

                return System.Reflection.Assembly.Load(buffer);
            }
        }
    }
}