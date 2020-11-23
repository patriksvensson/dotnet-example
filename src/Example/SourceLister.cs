using System.Diagnostics;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.IO;

namespace Example
{
    public sealed class SourceLister
    {
        private readonly IFileSystem _fileSystem;
        private readonly IGlobber _globber;

        public SourceLister(IFileSystem fileSystem, IGlobber globber)
        {
            _fileSystem = fileSystem;
            _globber = globber;
        }

        public int List(ProjectInformation project)
        {
            var result = FindProgram(project);
            if (result == null)
            {
                return -1;
            }

            using (var stream = _fileSystem.File.OpenRead(result.FullPath))
            using (var reader = new StreamReader(stream))
            {
                var lines = Colorizer.Colorize(reader.ReadToEnd());

                var table = new Table { ShowHeaders = false, Border = TableBorder.Rounded };
                table.AddColumn(new TableColumn("") { NoWrap = true });
                table.AddColumn("");

                var lineNumber = 1;
                foreach (var line in lines)
                {
                    table.AddRow($"[grey]{lineNumber}[/]", line);
                    lineNumber++;
                }

                AnsiConsole.WriteLine();
                AnsiConsole.Render(table);
            }

            return 0;
        }

        private FilePath FindProgram(ProjectInformation project)
        {
            var directory = project.Path.GetDirectory();
            var result = _globber.Match("**/Program.(c|f)s", new GlobberSettings { Root = directory }).OfType<FilePath>().ToList();

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
}
