using System.Text.RegularExpressions;

namespace Tests.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceWhitespace(this string input)
        {
            return Regex.Replace(input, @"\s+", " ");
        }
    }
}
