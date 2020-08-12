using Spectre.Cli;
using Spectre.Console;
using System.ComponentModel;

namespace Example.Commands
{
    [Description("Lists all available examples")]
    public sealed class ListCommand : Command
    {
        private readonly ExampleFinder _finder;

        public ListCommand()
        {
            _finder = new ExampleFinder();
        }

        public override int Execute(CommandContext context)
        {
            var examples = _finder.FindExamples();
            if (examples.Count == 0)
            {
                return 0;
            }

            var grid = new Grid();
            grid.AddColumn(new GridColumn() { NoWrap = true, Padding = new Padding(0, 4) });
            grid.AddColumn();

            foreach (var example in examples)
            {
                grid.AddRow($"[blue]{example.Name}[/]", $"{example.Description}");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(grid);

            return 0;
        }
    }
}
