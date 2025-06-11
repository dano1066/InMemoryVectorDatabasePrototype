using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class DocumentAddError
    {
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
