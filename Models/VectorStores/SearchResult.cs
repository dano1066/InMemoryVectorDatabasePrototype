using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class SearchResult
    {
        public bool IsEmpty => TextResults.Count == 0;
        public List<TextSearchResult> TextResults { get; set; } = new List<TextSearchResult>();
    }
}
