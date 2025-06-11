using InMemoryVectorStore.Models.VectorStores;
using InMemoryVectorStore.ServiceWrappers;
using InMemoryVectorStore.VectorStores;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace InMemoryVectorStore.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task TrainMode_CallsBuildDocumentIndexWithParsedDocuments()
        {
            using var tempDir = new TempDirectory();
            var file1 = tempDir.CreateFile("doc1.txt", "Hello world");

            var vectorDbMock = new Mock<IVectorDB>();
            vectorDbMock.Setup(v => v.InitializeAsync(It.IsAny<VectorDBOptions>())).ReturnsAsync(true);
            DocumentsAddResult result = new() { Success = true };
            IEnumerable<DocumentToProcess>? capturedDocs = null;
            vectorDbMock
                .Setup(v => v.BuildDocumentIndex(It.IsAny<IEnumerable<DocumentToProcess>>(), It.IsAny<ChunkingOptions>(), "mydb"))
                .Callback<IEnumerable<DocumentToProcess>, ChunkingOptions, string>((docs, opts, name) => capturedDocs = docs)
                .ReturnsAsync(result);

            await Program.TrainMode(vectorDbMock.Object, tempDir.Path, "mydb");

            vectorDbMock.Verify(v => v.InitializeAsync(It.IsAny<VectorDBOptions>()), Times.Once);
            vectorDbMock.Verify(v => v.BuildDocumentIndex(It.IsAny<IEnumerable<DocumentToProcess>>(), It.IsAny<ChunkingOptions>(), "mydb"), Times.Once);
            Assert.NotNull(capturedDocs);
            var doc = Assert.Single(capturedDocs!);
            Assert.Equal("doc1.txt", doc.FileName);
            Assert.Equal("Hello world", doc.Content.Trim());
        }

        [Fact]
        public async Task ListDatabasesMode_CallsListDatabases()
        {
            var vectorDbMock = new Mock<IVectorDB>();
            vectorDbMock.Setup(v => v.ListDatabasesAsync()).ReturnsAsync(new List<VectorDBInfo>());

            await Program.ListDatabasesMode(vectorDbMock.Object);

            vectorDbMock.Verify(v => v.ListDatabasesAsync(), Times.Once);
        }
    }
}
