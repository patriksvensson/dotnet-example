namespace Example;

public class ExampleRunner
{
    private readonly IAnsiConsole _console;
    private readonly ExampleFinder _finder;

    public ExampleRunner(IAnsiConsole console, ExampleFinder finder)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
    }

    public async Task<int> Run(string name, IRemainingArguments remaining)
    {
        var example = _finder.FindExample(name);
        if (example == null)
        {
            return -1;
        }

        if (!await Build(example).ConfigureAwait(false))
        {
            return -1;
        }

        var arguments = "run";
        if (remaining.Raw.Count > 0)
        {
            arguments += $"--no-build --no-restore -- {string.Join(" ", remaining.Raw)}";
        }

        // Run the example using "dotnet run"
        var info = new ProcessStartInfo("dotnet")
        {
            Arguments = arguments,
            WorkingDirectory = example.GetWorkingDirectory().FullPath,
        };

        var process = Process.Start(info);
        if (process == null)
        {
            throw new InvalidOperationException("An error occured when starting the 'dotnet' process");
        }

        process.WaitForExit();
        return process.ExitCode;
    }

    public async Task<int> RunAll(IRemainingArguments remaining)
    {
        var examples = _finder.FindExamples();
        foreach (var (_, first, _, example) in examples.Enumerate())
        {
            if (!first)
            {
                _console.WriteLine();
            }

            _console.Write(new Rule($"Example: [silver]{example.Name}[/]").LeftJustified().RuleStyle("grey"));

            var exitCode = await Run(example.Name, remaining).ConfigureAwait(false);
            if (exitCode != 0)
            {
                _console.MarkupLine($"[red]Error:[/] Example [u]{example.Name}[/] did not return a successful exit code.");
                return exitCode;
            }
        }

        return 0;
    }

    private async Task<bool> Build(ProjectInformation example)
    {
        var exitCode = await _console.Status().StartAsync($"Building example [yellow]{example.Name}[/]...", async ctx =>
        {
            var cmd = Cli.Wrap("dotnet").WithArguments("build")
                .WithWorkingDirectory(example.GetWorkingDirectory().FullPath)
                .WithValidation(CommandResultValidation.None);

            await foreach (var cmdEvent in cmd.ListenAsync())
            {
                switch (cmdEvent)
                {
                    case StandardErrorCommandEvent stdErr:
                        _console.MarkupLine($"[red]ERR>[/] {stdErr.Text.EscapeMarkup()}");
                        break;
                    case ExitedCommandEvent exited:
                        return exited.ExitCode;
                }
            }

            // Should never occur
            return -1;
        }).ConfigureAwait(false);

        if (exitCode != 0)
        {
            _console.MarkupLine($"[red]Error:[/] Could not build example [u]{example.Name}[/]");
        }

        return exitCode == 0;
    }
}
