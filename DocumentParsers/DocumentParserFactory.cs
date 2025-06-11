using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.DocumentParsers
{
    public static class DocumentParserFactory
    {
        public static IDocumentParser GetParser(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".txt" => new PlaintextParser(),
                ".json" => new JsonParser(),
                ".pdf" => new PDFParser(),
                ".doc" => new WordParser(),
                ".docx" => new WordParser(),
                // Default to plaintext parser for unknown file types
                _ => new PlaintextParser()
            };
        }
    }
}
