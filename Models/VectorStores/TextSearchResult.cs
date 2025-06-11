using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class TextSearchResult
    {
        public string Text { get; set; }
        public string Metadata { get; set; }
        public float Score { get; set; }
    }
}
