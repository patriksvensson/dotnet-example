using Spectre.Cli;
using System.ComponentModel;
using System.Diagnostics;

namespace Example.Commands
{
    public sealed class RunCommand : Command<RunCommand.Settings>
    {
        private readonly ExampleFinder _finder;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "<EXAMPLE>")]
            [Description("The example to run")]
            public string Name { get; set; }
        }

        public RunCommand()
        {
            _finder = new ExampleFinder();
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var example = _finder.FindExample(settings.Name);
            if (example == null)
            {
                return -1;
            }

            // Run the example using "dotnet run"
            ProcessStartInfo info = new ProcessStartInfo("dotnet");
            info.Arguments = "run";
            info.WorkingDirectory = example.GetWorkingDirectory().FullPath;
            var process = Process.Start(info);
            process.WaitForExit();

            // Return the example's exit code.
            return process.ExitCode;
        }
    }
}
