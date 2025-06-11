using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class DocumentsAddResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int DocumentsAdded { get; set; }
        public int TotalChunksAdded { get; set; }
        public List<DocumentAddError> Errors { get; set; } = new List<DocumentAddError>();
    }
}
