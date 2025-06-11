using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI
{
    public class AIUsage
    {
        public int PromptTokens { get; set; }

        public int CompletionTokens { get; set; }

        public int TotalTokens { get; set; }


        public AIUsage() { }

        public AIUsage(int promptTokens, int completionTokens)
        {
            PromptTokens = promptTokens;
            CompletionTokens = completionTokens;
            TotalTokens = promptTokens + completionTokens;
        }

        public AIUsage(ChatTokenUsage openAIUsage) //openAI model
        {
            PromptTokens = openAIUsage.InputTokenCount;
            CompletionTokens = openAIUsage.OutputTokenCount;
            TotalTokens = openAIUsage.TotalTokenCount;
        }
    }
}
