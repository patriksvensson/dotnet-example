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
            var file = _fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            {
                var xml = XDocument.Load(stream);

                var name = path.GetFilenameWithoutExtension().FullPath;
                var description = string.Empty;
                var order = 1000;

                // Got a description?
                var descriptionElement = xml.Root.XPathSelectElement("//Description");
                if (descriptionElement != null)
                {
                    description = descriptionElement.Value;
                }

                // Got a title?
                var titleElement = xml.Root.XPathSelectElement("//Title");
                if (titleElement != null)
                {
                    name = titleElement.Value;
                }

                // Got a order?
                var orderElement = xml.Root.XPathSelectElement("//ExampleOrder");
                if (orderElement != null)
                {
                    int.TryParse(orderElement.Value, out order);
                }

                return new ProjectInformation
                {
                    Name = name,
                    Path = path,
                    Description = description,
                    Order = order,
                };
            }
        }
    }
}
