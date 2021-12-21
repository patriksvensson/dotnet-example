using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.IO;

namespace Example;

public sealed class ExampleSourceLister
{
    private readonly IFileSystem _fileSystem;
    private readonly IGlobber _globber;

    public ExampleSourceLister(IFileSystem fileSystem, IGlobber globber)
    {
        _fileSystem = fileSystem;
        _globber = globber;
    }

    public bool List(ProjectInformation project)
    {
        var result = FindProgram(project);
        if (result == null)
        {
            return false;
        }

        var lines = GetLines(result);

        var table = new Table { ShowHeaders = false, Border = TableBorder.Rounded };
        table.AddColumn(new TableColumn(string.Empty) { NoWrap = true });
        table.AddColumn(string.Empty);

        var lineNumber = 1;
        foreach (var line in lines)
        {
            table.AddRow($"[grey]{lineNumber}[/]", line);
            lineNumber++;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);

        return true;
    }

    private List<string> GetLines(FilePath programFile)
    {
        using (var stream = _fileSystem.File.OpenRead(programFile.FullPath))
        using (var reader = new StreamReader(stream))
        {
            // F# doesn't have a SyntaxWalker, so just
            // return the lines.
            if (programFile.GetExtension() == ".fs")
            {
                var result = new List<string>();
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    var escaped = line.EscapeMarkup();
                    if (escaped != null)
                    {
                        result.Add(escaped);
                    }
                }

                return result;
            }

            // Return colorized lines for C#
            return CSharpColorizer.Colorize(reader.ReadToEnd());
        }
    }

    private FilePath? FindProgram(ProjectInformation project)
    {
        var directory = project.Path.GetDirectory();
        var result = _globber.Match("**/Program.{f|c}s", new GlobberSettings { Root = directory }).OfType<FilePath>().ToList();

        if (result.Count == 0)
        {
            AnsiConsole.Markup("[red]Error:[/] Could not find Program.cs for example [underline]{0}[/].", project.Name);
            return null;
        }

        if (result.Count > 1)
        {
            AnsiConsole.Markup("[red]Error:[/] Found multiple Program.cs for example [underline]{0}[/].", project.Name);
            return null;
        }

        return result.First();
    }
}
