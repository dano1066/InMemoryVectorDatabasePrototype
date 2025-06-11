using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InMemoryVectorStore
{
    public static class Highlighter
    {
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "is", "are", "in", "on", "at", "a", "an", "and", "or", "of", "to", "from", "by", "with", "for", "as", "that", "this"
    };

        public static string HighlightMatches(string query, string content)
        {
            var keywords = query
                .Split(new[] { ' ', '.', ',', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !StopWords.Contains(word.ToLower()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var word in keywords)
            {
                var pattern = $@"\b({Regex.Escape(word)})\b";
                content = Regex.Replace(content, pattern, "\x1b[1;33m$1\x1b[0m", RegexOptions.IgnoreCase); // Highlight yellow
            }

            return content;
        }
    }
}
