using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class ChunkingOptions
    {
        public ChunkingMethod Method { get; set; }
        public int ChunkSize { get; set; }
        public int OverlapSize { get; set; }
        public Func<string, string> RetrieveMetadata { get; set; }
    }
}
