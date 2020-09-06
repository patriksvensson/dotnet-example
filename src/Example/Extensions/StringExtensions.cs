namespace Example
{
    public static class StringExtensions
    {
        public static string EscapeMarkup(this string markup)
        {
            return markup?.Replace("[", "[[")?.Replace("]", "]]");
        }
    }
}
