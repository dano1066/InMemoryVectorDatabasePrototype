using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI
{
    public class AIEmbedding
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();

        public AIEmbedding() { }

        public AIEmbedding(float[] embedding)
        {
            Embedding = embedding;
        }

        public AIEmbedding(OpenAI.Embeddings.OpenAIEmbedding openAIEmbedding)
        {
            Embedding = openAIEmbedding.ToFloats().ToArray();
        }
    }
}
