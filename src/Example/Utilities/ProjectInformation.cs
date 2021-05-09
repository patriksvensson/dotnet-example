using Spectre.IO;

namespace Example
{
    public sealed class ProjectInformation
    {
        public string Name { get; set; }
        public FilePath Path { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; }
        public string Group { get; set; }

        public DirectoryPath GetWorkingDirectory()
        {
            return Path.GetDirectory();
        }
    }
}
