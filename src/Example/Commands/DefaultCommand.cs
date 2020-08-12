using Spectre.Cli;
using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics;

namespace Example.Commands
{
    public sealed class DefaultCommand : Command<DefaultCommand.Settings>
    {
        private readonly ExampleFinder _finder;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[EXAMPLE]")]
            [Description("The example to run.\nIf none is specified, all examples will be listed")]
            public string Name { get; set; }

            [CommandOption("-l|--list")]
            [Description("Lists all available examples")]
            public bool List { get; set; }
        }

        public DefaultCommand()
        {
            _finder = new ExampleFinder();
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.List || string.IsNullOrWhiteSpace(settings.Name))
            {
                return List();
            }

            return Run(settings.Name);
        }

        private int List()
        {
            var examples = _finder.FindExamples();
            if (examples.Count == 0)
            {
                AnsiConsole.Markup("[yellow]No examples could be found.[/]");
                return 0;
            }

            var grid = new Grid();
            grid.AddColumn(new GridColumn() { NoWrap = true, Padding = new Padding(2, 4) });
            grid.AddColumn();

            AnsiConsole.MarkupLine("[underline]Examples[/]");

            foreach (var example in examples)
            {
                grid.AddRow($"[blue]{example.Name}[/]", $"{example.Description}");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(grid);

            return 0;
        }

        private int Run(string name)
        {
            var example = _finder.FindExample(name);
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
