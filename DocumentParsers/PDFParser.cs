using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace InMemoryVectorStore.DocumentParsers
{
    public class PDFParser : IDocumentParser
    {
        public async Task<string> GetContentAsync(string filePath)
        {
            // Note: PdfPig doesn't have async methods, so we're wrapping it in a Task
            return await Task.Run(() =>
            {
                var sb = new StringBuilder();

                using (PdfDocument document = PdfDocument.Open(filePath))
                {
                    foreach (Page page in document.GetPages())
                    {
                        string pageText = page.Text;
                        sb.AppendLine(pageText);
                    }
                }

                return sb.ToString();
            });
        }
    }
}
