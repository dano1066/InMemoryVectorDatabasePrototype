using InMemoryVectorStore.Models.AI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.ServiceWrappers
{
    public class AICompletion
    {
        public List<AIContent> Content { get; set; } = new List<AIContent>();

        public AIUsage? Usage { get; set; }
        public AICompletion() { }

        public AICompletion(string text, AIUsage? usage = null)
        {
            Content.Add(new AIContent(text));
            Usage = usage;
        }

        public AICompletion(ChatCompletion openAICompletion) //open AI model
        {
            if (openAICompletion.Content != null)
            {
                foreach (var content in openAICompletion.Content)
                {
                    Content.Add(new AIContent(content.Text ?? string.Empty));
                }
            }

            if (openAICompletion.Usage != null)
            {
                Usage = new AIUsage(openAICompletion.Usage);
            }
        }
    }
}
