using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI.Messages
{
    public class UserAIMessage : AIMessage
    {
        public UserAIMessage()
        {
            Role = AIMessageRole.User;
        }

        public UserAIMessage(string content) : base(AIMessageRole.User, content)
        {
        }
    }
}
