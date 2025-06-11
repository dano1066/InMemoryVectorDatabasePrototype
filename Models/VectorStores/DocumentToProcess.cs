using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class DocumentToProcess
    {
        public string Content { get; set; }
        public string FileName { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
