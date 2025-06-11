using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public enum ChunkingMethod
    {
        FixedLength,
        Document,
        Paragraph
    }
}
