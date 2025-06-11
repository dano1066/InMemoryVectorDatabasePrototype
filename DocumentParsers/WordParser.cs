using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.DocumentParsers
{
    public class WordParser : IDocumentParser
    {
        public async Task<string> GetContentAsync(string filePath)
        {
            // OpenXml doesn't have async methods, so we're wrapping it in a Task
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();

                using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                {
                    Body body = doc.MainDocumentPart.Document.Body;

                    if (body != null)
                    {
                        sb.Append(body.InnerText);
                    }
                }

                return sb.ToString();
            });
        }
    }
}
