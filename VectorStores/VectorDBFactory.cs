using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.VectorStores
{
    public static class VectorDBFactory
    {
        public static IVectorDB CreateInMemoryVectorDB()
        {
            var model = "text-embedding-3-small";

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
            var openAIClient = new OpenAI.OpenAIClient(apiKey);
            var embeddingClient = openAIClient.GetEmbeddingClient(model);

            return new InMemoryVectorDB(embeddingClient);
        }
        public static IVectorDB CreateInMemoryVectorDB(EmbeddingClient embeddingClient)
        {
            return new InMemoryVectorDB(embeddingClient);
        }
    }
}
