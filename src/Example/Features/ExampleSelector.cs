using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace Example;

public sealed class ExampleSelector
{
    private readonly IAnsiConsole _console;
    private readonly ExampleFinder _finder;

    public ExampleSelector(IAnsiConsole console, ExampleFinder finder)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
    }

    public ProjectInformation? Select()
    {
        var examples = _finder.FindExamples();
        if (examples.Count == 0)
        {
            _console.Markup("[yellow]No examples could be found.[/]");
            return null;
        }

        var prompt = new SelectionPrompt<string>();
        var groups = examples.GroupBy(ex => ex.Group);

        if (groups.Count() == 1)
        {
            prompt.AddChoices(examples.Select(x => x.Name));
        }
        else
        {
            var noGroupExamples = new List<string>();

            foreach (var group in groups)
            {
                if (string.IsNullOrEmpty(group.Key))
                {
                    noGroupExamples.AddRange(group.Select(x => x.Name));
                }
                else
                {
                    prompt.AddChoiceGroup(
                        group.Key,
                        group.Select(x => x.Name));
                }
            }

            if (noGroupExamples.Count > 0)
            {
                prompt.AddChoices(noGroupExamples);
            }
        }

        var example = AnsiConsole.Prompt(prompt
            .Title("[yellow]Choose example to run[/]")
            .MoreChoicesText("[grey](Move up and down to reveal more examples)[/]")
            .Mode(SelectionMode.Leaf));

        return examples.FirstOrDefault(x => x.Name == example);
    }
}
