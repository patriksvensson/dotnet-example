using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.IO;

namespace Example;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<DefaultCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("dotnet example");
        });

        return await app.RunAsync(args).ConfigureAwait(false);
    }
}

public sealed class DefaultCommand : AsyncCommand<DefaultCommand.Settings>
{
    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly IGlobber _globber;

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[EXAMPLE]")]
        [Description("The example to run.\nIf none is specified, all examples will be listed")]
        public string? Name { get; set; }

        [CommandOption("-l|--list")]
        [Description("Lists all available examples")]
        public bool List { get; set; }

        [CommandOption("-a|--all")]
        [Description("Runs all available examples")]
        public bool All { get; set; }

        [CommandOption("--skip")]
        [Description("Skips example when combined with [grey]--all[/]")]
        public string[]? Skip { get; set; }

        [CommandOption("-s|--source")]
        [Description("Show example source code")]
        public bool Source { get; set; }

        [CommandOption("--select")]
        [Description("Show example source code")]
        public bool Select { get; set; }
    }

    public DefaultCommand(IAnsiConsole console)
    {
        _console = console;
        _fileSystem = new FileSystem();
        _environment = new Environment();
        _globber = new Globber(_fileSystem, _environment);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (settings.All)
        {
            var finder = new ExampleFinder(_fileSystem, _environment, _globber, settings.Skip);
            var runner = new ExampleRunner(_console, finder);
            return await runner.RunAll(context.Remaining).ConfigureAwait(false);
        }
        else if (settings.Select)
        {
            var finder = new ExampleFinder(_fileSystem, _environment, _globber, settings.Skip);
            var selector = new ExampleSelector(_console, finder);

            var example = selector.Select();
            if (example == null)
            {
                return -1;
            }

            if (settings.Source)
            {
                var sourceLister = new ExampleSourceLister(_fileSystem, _globber);
                if (!sourceLister.List(example))
                {
                    return -1;
                }

                return 0;
            }
            else
            {
                var runner = new ExampleRunner(_console, finder);
                return await runner.Run(example.Name, context.Remaining).ConfigureAwait(false);
            }
        }
        else if (settings.List || string.IsNullOrWhiteSpace(settings.Name))
        {
            var finder = new ExampleFinder(_fileSystem, _environment, _globber, settings.Skip);

            // Only one example?
            var examples = finder.FindExamples();
            if (examples.Count == 1)
            {
                // Execute it
                var runner = new ExampleRunner(_console, finder);
                return await runner.Run(examples[0].Name, context.Remaining).ConfigureAwait(false);
            }

            var lister = new ExampleLister(_console, finder);
            lister.List();
            return 0;
        }
        else if (settings.Source)
        {
            var finder = new ExampleFinder(_fileSystem, _environment, _globber, settings.Skip);
            var example = finder.FindExample(settings.Name);
            if (example == null)
            {
                return -1;
            }

            var lister = new ExampleSourceLister(_fileSystem, _globber);
            if (!lister.List(example))
            {
                return -1;
            }

            return 0;
        }
        else
        {
            var finder = new ExampleFinder(_fileSystem, _environment, _globber, settings.Skip);
            var runner = new ExampleRunner(_console, finder);
            return await runner.Run(settings.Name, context.Remaining).ConfigureAwait(false);
        }
    }
}
