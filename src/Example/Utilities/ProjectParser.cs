using System;
using System.Xml.Linq;
using System.Xml.XPath;
using Spectre.IO;

namespace Example;

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

        // Visible?
        var visible = true;
        var visibleString = Parse(xml, "//ExampleVisible");
        if (visibleString?.Equals("false", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            visible = false;
        }

        // Got a description?
        var description = Parse(xml, "//ExampleDescription", "//Description");

        // Belongs to a group?
        var group = Parse(xml, "//ExampleGroup");

        // Got a title?
        var name = Parse(xml, "//ExampleTitle", "//Title");
        if (string.IsNullOrWhiteSpace(name))
        {
            name = path.GetFilenameWithoutExtension().FullPath;
        }

        // Got an order?
        var order = 1024;
        var orderString = Parse(xml, "//ExampleOrder");
        if (int.TryParse(orderString, out var orderInteger))
        {
            order = orderInteger;
        }

        return new ProjectInformation(name, path, description, order, visible, group);
    }

    private static string Parse(XDocument document, params string[] expressions)
    {
        foreach (var expression in expressions)
        {
            var element = document.Root?.XPathSelectElement(expression);
            if (element != null)
            {
                return element.Value;
            }
        }

        return string.Empty;
    }
}
