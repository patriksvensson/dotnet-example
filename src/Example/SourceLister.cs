using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public void List(FilePath project)
        {
            var directory = project.GetDirectory();
            var result = _globber.Match("**/Program.cs", new GlobberSettings { Root = directory }).OfType<FilePath>().FirstOrDefault();
            if (result == null)
            {
                throw new InvalidOperationException("Could not find source.");
            }

            using (var stream = _fileSystem.File.OpenRead(result.FullPath))
            using (var reader = new StreamReader(stream))
            {
                var lines = Colorizer.Colorize(reader.ReadToEnd());

                var table = new Table { ShowHeaders = false, Border = BorderKind.Rounded };
                table.AddColumn(new TableColumn("") { NoWrap = true });
                table.AddColumn("");

                var lineNumber = 1;
                foreach (var line in lines)
                {
                    table.AddRow($"[grey]{lineNumber}[/]", line);
                    lineNumber++;
                }

                AnsiConsole.Render(table);
            }
        }
    }
}
