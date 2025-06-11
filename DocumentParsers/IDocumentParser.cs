using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.DocumentParsers
{
    public interface IDocumentParser
    {
        Task<string> GetContentAsync(string filePath);
    }
}
