using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI
{
    public class AIContent
    {
        /// <summary>
        /// The text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new empty content
        /// </summary>
        public AIContent() { }

        /// <summary>
        /// Creates a new content with the specified text
        /// </summary>
        public AIContent(string text)
        {
            Text = text;
        }
    }
}
