using System;
using System.Threading.Tasks;
using CliWrap;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Example
{
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

            if (!await Build(example))
            {
                return -1;
            }

            var result = await Cli.Wrap("dotnet")
                .WithWorkingDirectory(example.GetWorkingDirectory().FullPath)
                .WithValidation(CommandResultValidation.None)
                .WithArguments(args =>
                {
                    args.Add("run");

                    if (remaining.Raw.Count > 0)
                    {
                        args
                            .Add("--no-build")
                            .Add("--no-restore")
                            .Add("--")
                            .Add(remaining.Raw);
                    }
                })
                .WithStandardOutputPipe(PipeTarget.ToDelegate(_console.WriteLine))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(_console.WriteLine))
                .ExecuteAsync();

            return result.ExitCode;
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

                _console.Write(new Rule($"Example: [silver]{example.Name}[/]").LeftAligned().RuleStyle("grey"));

                var exitCode = await Run(example.Name, remaining);
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
                var result = await Cli.Wrap("dotnet")
                    .WithArguments("build")
                    .WithWorkingDirectory(example.GetWorkingDirectory().FullPath)
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(
                        line => _console.MarkupLine($"[red]{line.EscapeMarkup()}[/]")
                    ))
                    .ExecuteAsync();

                return result.ExitCode;
            });

            return exitCode == 0;
        }
    }
}