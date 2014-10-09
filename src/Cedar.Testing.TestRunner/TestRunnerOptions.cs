namespace Cedar.Testing.TestRunner
{
    using System;
    using System.IO;
    using PowerArgs;

    public class TestRunnerOptions
    {
        private string[] _formatters;

        [ArgDescription("Show help."), ArgShortcut("?")]
        public bool Help { get; set; }

        [ArgDescription("The assembly to test.")]
        [ArgPosition(0)]
        public string Assembly { get; set; }

        [ArgDescription("Force teamcity output.")]
        public bool Teamcity { get; set; }

        [ArgDescription("A list of formaters.")]
        public string[] Formatters
        {
            get { return _formatters; }
            set
            {
                if(value == null || value.Length == 0)
                {
                    return;
                }
                _formatters = value;
            }
        }

        [ArgDescription("Output folder.")]
        public string Output { get; set; }

        public TestRunnerOptions()
        {
            _formatters = new[]
            {
                "PlainText"
            };
        }
    }
}