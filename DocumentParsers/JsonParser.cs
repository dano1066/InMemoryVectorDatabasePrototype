using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.DocumentParsers
{
    public class JsonParser : IDocumentParser
    {
        public async Task<string> GetContentAsync(string filePath)
        {
            var jsonText = await File.ReadAllTextAsync(filePath);
            //TODO: do we put some intelligence in here to parse out the important parts and get rid of the junk?
            return jsonText;
        }
    }
}
