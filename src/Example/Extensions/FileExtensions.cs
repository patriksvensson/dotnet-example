namespace Example;

public static class FileExtensions
{
    public static IEnumerable<string> ReadLines(this IFile file)
    {
        using var stream = file.OpenRead();
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}
