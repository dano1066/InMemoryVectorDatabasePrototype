using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI.Messages
{
    public class AssistantAIMessage : AIMessage
    {
        public AssistantAIMessage()
        {
            Role = AIMessageRole.Assistant;
        }

        public AssistantAIMessage(string content) : base(AIMessageRole.Assistant, content)
        {
        }
    }
}
