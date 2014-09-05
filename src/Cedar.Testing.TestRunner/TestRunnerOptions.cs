namespace Cedar.Testing.TestRunner
{
    using System;
    using System.IO;
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

        [ArgDescription("A list of formaters.")]
        public string[] Formatters { get; set; }
        
        [ArgDescription("Output folder.")]
        public string Output { get; set; }

        public TestRunnerOptions()
        {
            Formatters = new[]
            {
                "PlainText"
            };
        }

        public string GetOutputWithExtension(string fileExtension)
        {
            if (String.IsNullOrWhiteSpace(Output)) throw new InvalidOperationException();

            return Path.ChangeExtension(Path.Combine(Output, Assembly), fileExtension);
        }
    }
}