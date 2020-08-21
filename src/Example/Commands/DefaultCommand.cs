using Spectre.Cli;
using Spectre.Console;
using Spectre.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Example.Commands
{
    public sealed class DefaultCommand : Command<DefaultCommand.Settings>
    {
        private readonly ExampleFinder _finder;
        private readonly SourceLister _lister;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[EXAMPLE]")]
            [Description("The example to run.\nIf none is specified, all examples will be listed")]
            public string Name { get; set; }

            [CommandOption("-l|--list")]
            [Description("Lists all available examples")]
            public bool List { get; set; }

            [CommandOption("-s|--source")]
            [Description("Show example source code")]
            public bool Source { get; set; }
        }

        private readonly IEnvironment _environment;

        public DefaultCommand()
        {
            var fileSystem = new FileSystem();
            var environment = new Environment();
            var globber = new Globber(fileSystem, environment);

            _environment = environment;
            _finder = new ExampleFinder(fileSystem, environment, globber);
            _lister = new SourceLister(fileSystem, globber);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (settings.List || string.IsNullOrWhiteSpace(settings.Name))
            {
                return List();
            }

            if (settings.Source)
            {
                return ViewSource(settings.Name);
            }

            return Run(settings.Name, context);
        }

        private int List()
        {
            var examples = _finder.FindExamples();
            if (examples.Count == 0)
            {
                AnsiConsole.Markup("[yellow]No examples could be found.[/]");
                return 0;
            }

            var grid = new Table { Border = BorderKind.Square };
            grid.AddColumn(new TableColumn("Name") { NoWrap = true, });
            grid.AddColumn(new TableColumn("Path") { NoWrap = true, });
            grid.AddColumn(new TableColumn("Description"));

            foreach (var example in examples.OrderBy(e => e.Order))
            {
                var path = _environment.WorkingDirectory.GetRelativePath(example.Path);

                grid.AddRow(
                    $"[underline blue]{example.Name}[/]",
                    $"[grey]{path.FullPath}[/]",
                    !string.IsNullOrEmpty(example.Description)
                        ? $"{example.Description}"
                        : "[grey]N/A[/]");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(grid);

            return 0;
        }

        private int ViewSource(string name)
        {
            var example = _finder.FindExample(name);
            if (example == null)
            {
                return -1;
            }

            _lister.List(example);

            return 0;
        }

        private int Run(string name, CommandContext context)
        {
            var example = _finder.FindExample(name);
            if (example == null)
            {
                return -1;
            }

            var arguments = "run";
            if (context.Remaining.Raw.Count > 0)
            {
                var remaining = string.Join(" ", context.Remaining.Raw);
                arguments += $" -- {remaining}";
            }

            // Run the example using "dotnet run"
            var info = new ProcessStartInfo("dotnet")
            {
                Arguments = arguments,
                WorkingDirectory = example.GetWorkingDirectory().FullPath
            };

            var process = Process.Start(info);
            process.WaitForExit();

            // Return the example's exit code.
            return process.ExitCode;
        }
    }
}
