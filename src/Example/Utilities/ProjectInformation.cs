using Spectre.IO;

namespace Example;

public sealed class ProjectInformation
{
    public string Name { get; }
    public FilePath Path { get; }
    public string Description { get; }
    public int Order { get; }
    public bool Visible { get; }
    public string Group { get; }

    public ProjectInformation(
        string name, FilePath path, string description,
        int order, bool visible, string group)
    {
        Name = name;
        Path = path;
        Description = description;
        Order = order;
        Visible = visible;
        Group = group;
    }

    public DirectoryPath GetWorkingDirectory()
    {
        return Path.GetDirectory();
    }
}
