using Spectre.Console;
using Spectre.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Spectre.IO.Environment;

namespace Example
{
    public sealed class ExampleFinder
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironment _environment;
        private readonly IGlobber _globber;
        private readonly ProjectParser _parser;

        public ExampleFinder()
        {
            _fileSystem = new FileSystem();
            _environment = new Environment();
            _globber = new Globber(_fileSystem, _environment);
            _parser = new ProjectParser();
        }

        public ProjectInformation FindExample(string name)
        {
            var result = new List<ProjectInformation>();
            foreach (var example in FindExamples())
            {
                if (example.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(example);
                }
            }

            if (result.Count == 0)
            {
                AnsiConsole.Markup("[red]Error:[/] The example [blue]{0}[/] could not be found.", name);
                return null;
            }

            if (result.Count > 1)
            {
                AnsiConsole.Markup("[red]Error:[/] Found multiple examples called [blue]{0}[/].", name);
                return null;
            }

            return result.First();
        }

        public IReadOnlyList<ProjectInformation> FindExamples()
        {
            var result = new List<ProjectInformation>();
            foreach (var example in FindProjects())
            {
                result.Add(_parser.Parse(example));
            }
            return result;
        }

        private IReadOnlyList<FilePath> FindProjects()
        {
            var root = new DirectoryPath($"examples").MakeAbsolute(_environment);
            var globberSettings = new GlobberSettings { Comparer = new PathComparer(false), Root = root };
            return _globber.Match($"**/*.csproj", globberSettings).OfType<FilePath>().ToList();
        }
    }
}
