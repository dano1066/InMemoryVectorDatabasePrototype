using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI.Messages
{
    public class SystemAIMessage : AIMessage
    {
        public SystemAIMessage()
        {
            Role = AIMessageRole.System;
        }

        public SystemAIMessage(string content) : base(AIMessageRole.System, content)
        {
        }
    }
}
