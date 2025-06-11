using InMemoryVectorStore.Models.VectorStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.VectorStores
{
    public interface IVectorDB
    {
        Task<bool> InitializeAsync(VectorDBOptions options);

        Task<DocumentsAddResult> BuildDocumentIndex(IEnumerable<DocumentToProcess> documents, ChunkingOptions chunkingOptions, string indexName);

        Task<SearchResult> SearchAsync(string query, float threshold = 0.2f, int pageCount = 5);

        Task<List<VectorDBInfo>> ListDatabasesAsync();
    }
}
