using System.Text.RegularExpressions;
using System.Text;

namespace PrimeGames.Extensions
{
    public static class StringExtensions
    {
        public static string ToSlug(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLowerInvariant();

            text = Regex.Replace(text, @"[^a-z0-9\s-ก-๙]", "");

            text = Regex.Replace(text, @"\s+", "-");

            text = Regex.Replace(text, @"-{2,}", "-");

            text = text.Trim('-');

            if (text.Length > 100)
                text = text.Substring(0, 100).Trim('-');

            return text;
        }

        public static string TruncateAtWord(this string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            int lastSpace = text.LastIndexOf(' ', maxLength);
            if (lastSpace > 0)
                return text.Substring(0, lastSpace) + "...";

            return text.Substring(0, maxLength) + "...";
        }

        public static string StripHtml(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            return Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}