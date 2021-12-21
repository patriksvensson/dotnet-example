using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using Spectre.IO;

namespace Example;

public sealed class ExampleFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly IGlobber _globber;
    private readonly string[] _skip;
    private readonly ProjectParser _parser;

    public ExampleFinder(IFileSystem fileSystem, IEnvironment environment, IGlobber globber, string[]? skip)
    {
        _fileSystem = fileSystem;
        _environment = environment;
        _globber = globber;
        _skip = skip ?? Array.Empty<string>();
        _parser = new ProjectParser(fileSystem);
    }

    public ProjectInformation? FindExample(string name)
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

        return result[0];
    }

    public IReadOnlyList<ProjectInformation> FindExamples()
    {
        var result = new List<ProjectInformation>();

        var folders = GetExampleFolders();
        var examples = folders.Select(FindProjects).Aggregate((acc, xs) => acc.Concat(xs));
        foreach (var example in examples)
        {
            result.Add(_parser.Parse(example));
        }

        return result
            .Where(x => x.Visible && !_skip.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x.Order)
            .ToList();
    }

    private string[] GetExampleFolders()
    {
        var dotExamplesFilePath = new FilePath(".examples").MakeAbsolute(_environment);
        var folders = _fileSystem.Exist(dotExamplesFilePath)
                    ? _fileSystem.GetFile(dotExamplesFilePath)
                                 .ReadLines()
                                 .Where(s => !string.IsNullOrWhiteSpace(s)
                                          && !s.StartsWith('#')) // skip comments
                                 .Select(s => s.Trim())
                                 .ToArray()
                    : Array.Empty<string>();

        if (folders.Length == 0)
        {
            folders = new[] { "examples", "samples" };
        }

        return folders;
    }

    private IEnumerable<FilePath> FindProjects(string folder)
    {
        var root = new DirectoryPath(folder).MakeAbsolute(_environment);
        var globberSettings = new GlobberSettings { Comparer = new PathComparer(false), Root = root };
        return _globber.Match(@"**/*.{c|f}sproj", globberSettings).OfType<FilePath>();
    }
}
