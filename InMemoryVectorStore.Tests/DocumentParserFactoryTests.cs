using InMemoryVectorStore.DocumentParsers;
using Xunit;

namespace InMemoryVectorStore.Tests
{
    public class DocumentParserFactoryTests
    {
        [Theory]
        [InlineData("test.txt", typeof(PlaintextParser))]
        [InlineData("test.json", typeof(JsonParser))]
        [InlineData("test.pdf", typeof(PDFParser))]
        [InlineData("test.doc", typeof(WordParser))]
        [InlineData("test.docx", typeof(WordParser))]
        [InlineData("test.unknown", typeof(PlaintextParser))]
        public void GetParser_ReturnsExpectedParserType(string fileName, Type expectedType)
        {
            var parser = DocumentParserFactory.GetParser(fileName);
            Assert.IsType(expectedType, parser);
        }
    }
}
