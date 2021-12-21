using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace Example;

public sealed class ExampleLister
{
    private readonly IAnsiConsole _console;
    private readonly ExampleFinder _finder;

    public ExampleLister(IAnsiConsole console, ExampleFinder finder)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
    }

    public void List()
    {
        var examples = _finder.FindExamples();
        if (examples.Count == 0)
        {
            _console.Markup("[yellow]No examples could be found.[/]");
            return;
        }

        _console.WriteLine();

        var rows = new Grid().Collapse();
        rows.AddColumn();
        foreach (var group in examples.GroupBy(ex => ex.Group))
        {
            rows.AddRow(CreateTable(group.Key, group));
            rows.AddEmptyRow();
        }

        _console.Write(rows);
        _console.MarkupLine("Type [blue]dotnet example --help[/] for help");
    }

    private static Table CreateTable(string group, IEnumerable<ProjectInformation> projects)
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
            grid.AddRow(
                $"[underline blue]{example.Name}[/]",
                example.Description ?? "[grey]N/A[/]");
        }

        return grid;
    }
}
