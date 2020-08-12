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

        public DirectoryPath GetWorkingDirectory()
        {
            return Path.GetDirectory();
        }
    }

    public sealed class ProjectParser
    {
        private readonly IFileSystem _fileSystem;

        public ProjectParser()
        {
            _fileSystem = new FileSystem();
        }

        public ProjectInformation Parse(FilePath path)
        {
            var file = _fileSystem.GetFile(path);
            using (var stream = file.OpenRead())
            {
                var xml = XDocument.Load(stream);

                var name = path.GetFilenameWithoutExtension().FullPath;
                var description = string.Empty;

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
                else
                {
                    // Got a package ID?
                    var packageIdElement = xml.Root.XPathSelectElement("//PackageId");
                    if (packageIdElement != null)
                    {
                        name = packageIdElement.Value;
                    }
                }

                return new ProjectInformation
                {
                    Name = name,
                    Path = path,
                    Description = description
                };
            }
        }
    }
}
