using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.DocumentParsers
{
    public class PlaintextParser : IDocumentParser
    {
        public async Task<string> GetContentAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
