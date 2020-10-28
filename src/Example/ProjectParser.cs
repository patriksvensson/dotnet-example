using Spectre.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Example
{
    public sealed class ProjectInformation
    {
        public string Name { get; set; }
        public FilePath Path { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }

        public DirectoryPath GetWorkingDirectory()
        {
            return Path.GetDirectory();
        }
    }

    public sealed class ProjectParser
    {
        private readonly IFileSystem _fileSystem;

        public ProjectParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ProjectInformation Parse(FilePath path)
        {
            // Load the project file
            var file = _fileSystem.GetFile(path);
            using var stream = file.OpenRead();
            var xml = XDocument.Load(stream);

            // Got a description?
            var description = Parse(xml, "//ExampleDescription", "//Description");

            // Got a title?
            var name = Parse(xml, "//ExampleTitle", "//Title");
            if (string.IsNullOrWhiteSpace(name))
            {
                name = path.GetFilenameWithoutExtension().FullPath;
            }

            // Got an order?
            var order = 1024;
            var orderString = Parse(xml, "//ExampleOrder");
            if (string.IsNullOrWhiteSpace(orderString) &&
                int.TryParse(orderString, out var orderInteger))
            {
                order = orderInteger;
            }

            return new ProjectInformation
            {
                Name = name,
                Path = path,
                Description = description,
                Order = order,
            };
        }

        private static string Parse(XDocument document, params string[] expressions)
        {
            foreach (var expression in expressions)
            {
                var element = document.Root.XPathSelectElement(expression);
                if (element != null)
                {
                    return element.Value;
                }
            }

            return string.Empty;
        }
    }
}
