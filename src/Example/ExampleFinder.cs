using Spectre.Console;
using Spectre.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
    public sealed class ExampleFinder
    {
        private readonly IEnvironment _environment;
        private readonly IGlobber _globber;
        private readonly ProjectParser _parser;

        public ExampleFinder(IFileSystem fileSystem, IEnvironment environment, IGlobber globber)
        {
            _environment = environment;
            _globber = globber;
            _parser = new ProjectParser(fileSystem);
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
                AnsiConsole.Markup("[red]Error:[/] The example [underline]{0}[/] could not be found.", name);
                return null;
            }

            if (result.Count > 1)
            {
                AnsiConsole.Markup("[red]Error:[/] Found multiple examples called [underline]{0}[/].", name);
                return null;
            }

            return result.First();
        }

        public IReadOnlyList<ProjectInformation> FindExamples()
        {
            var result = new List<ProjectInformation>();

            var examples = FindProjects("examples").Concat(FindProjects("samples"));
            foreach (var example in examples)
            {
                result.Add(_parser.Parse(example));
            }

            return result.OrderBy(x => x.Order).ToList();
        }

        private IEnumerable<FilePath> FindProjects(string folder)
        {
            var root = new DirectoryPath(folder).MakeAbsolute(_environment);
            var globberSettings = new GlobberSettings { Comparer = new PathComparer(false), Root = root };
            return _globber.Match($"**/*.(c|f)sproj", globberSettings).OfType<FilePath>();
        }
    }
}
