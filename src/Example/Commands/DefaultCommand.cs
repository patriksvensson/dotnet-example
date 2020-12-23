using Spectre.Cli;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Example.Commands
{
    public sealed class DefaultCommand : Command<DefaultCommand.Settings>
    {
        private readonly IEnvironment _environment;
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
                return List(settings);
            }

            if (settings.Source)
            {
                return ViewSource(settings.Name);
            }

            return Run(settings.Name, context);
        }

        private int List(Settings settings)
        {
            var examples = _finder.FindExamples();
            if (examples.Count == 0)
            {
                AnsiConsole.Markup("[yellow]No examples could be found.[/]");
                return 0;
            }

            AnsiConsole.WriteLine();

            var rows = new Grid().Collapse();
            rows.AddColumn();
            foreach (var group in examples.GroupBy(ex => ex.Group))
            {
                rows.AddRow(CreateTable(settings, group.Key, group));
                rows.AddEmptyRow();
            }

            AnsiConsole.Render(rows);

            AnsiConsole.MarkupLine("Type [blue]dotnet example --help[/] for help");

            return 0;
        }

        private Table CreateTable(Settings settings, string group, IEnumerable<ProjectInformation> projects)
        {
            var grid = new Table { Border = TableBorder.Rounded }.Expand();
            grid.AddColumn(new TableColumn("[grey]Example[/]") { NoWrap = true, });
            grid.AddColumn(new TableColumn("[grey]Description[/]"));

            if (!string.IsNullOrWhiteSpace(group))
            {
                grid.Title = new TableTitle(group);
            }

            foreach (var example in projects.OrderBy(e => e.Order))
            {
                var path = _environment.WorkingDirectory.GetRelativePath(example.Path);

                grid.AddRow(
                    $"[underline blue]{example.Name}[/]",
                    !string.IsNullOrEmpty(example.Description)
                        ? $"{example.Description}"
                        : "[grey]N/A[/]");
            }

            return grid;
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
            foreach (var (_, first, _, example) in examples.Enumerate())
            {
                if (!first)
                {
                    AnsiConsole.WriteLine();
                }

                AnsiConsole.Render(new Rule($"Example: [silver]{example.Name}[/]").LeftAligned().RuleStyle("grey"));

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
