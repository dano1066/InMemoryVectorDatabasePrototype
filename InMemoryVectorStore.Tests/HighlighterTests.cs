using Xunit;

namespace InMemoryVectorStore.Tests
{
    public class HighlighterTests
    {
        [Fact]
        public void HighlightMatches_HighlightsKeywords()
        {
            string query = "quick brown fox";
            string content = "The quick brown fox jumps over the lazy dog.";

            string highlighted = Highlighter.HighlightMatches(query, content);

            Assert.Contains("\x1b[1;33mquick\x1b[0m", highlighted);
            Assert.Contains("\x1b[1;33mbrown\x1b[0m", highlighted);
            Assert.Contains("\x1b[1;33mfox\x1b[0m", highlighted);
        }
    }
}
