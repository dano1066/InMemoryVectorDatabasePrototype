using InMemoryVectorStore.VectorStores;
using Xunit;

namespace InMemoryVectorStore.Tests
{
    public class InMemoryVectorDBTests
    {
        [Fact]
        public void GenerateCacheIdentifierFromFiles_ReturnsSameHash_ForSameFiles()
        {
            using var tempDir = new TempDirectory();
            var file1 = tempDir.CreateFile("a.txt", "hello");
            var file2 = tempDir.CreateFile("b.txt", "world");

            var hash1 = InMemoryVectorDB.GenerateCacheIdentifierFromFiles(new[] { file1, file2 });
            var hash2 = InMemoryVectorDB.GenerateCacheIdentifierFromFiles(new[] { file2, file1 });

            Assert.Equal(hash1, hash2);
        }
    }

}
