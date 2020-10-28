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

            [CommandOption("-a|--all")]
            [Description("Runs all available examples")]
            public bool All { get; set; }

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
            if (settings.All)
            {
                return RunAll(settings, context);
            }

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

            var grid = new Table { Border = TableBorder.Rounded };
            grid.AddColumn(new TableColumn("[grey]Name[/]") { NoWrap = true, });
            grid.AddColumn(new TableColumn("[grey]Path[/]") { NoWrap = true, });
            grid.AddColumn(new TableColumn("[grey]Description[/]"));

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
            AnsiConsole.MarkupLine("[u]Available examples:[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Render(grid);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Type [blue]dotnet example --help[/] for help");

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

        private int RunAll(Settings settings, CommandContext context)
        {
            var examples = _finder.FindExamples();
            foreach (var example in examples)
            {
                var exitCode = Run(example.Name, context);
                if (exitCode != 0)
                {
                    AnsiConsole.MarkupLine($"Example [u]{example.Name}[/] did not return a successful exit code.");
                    return exitCode;
                }
            }

            return 0;
        }
    }
}
