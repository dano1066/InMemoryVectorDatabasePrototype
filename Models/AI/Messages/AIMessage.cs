using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.AI.Messages
{
    public class AIMessage
    {
        public AIMessageRole Role { get; set; }

        public string Content { get; set; } = string.Empty;


        public AIMessage() { }

        public AIMessage(AIMessageRole role, string content)
        {
            Role = role;
            Content = content;
        }

        public AIMessage(ChatMessage openAIMessage)
        {
            Content = openAIMessage.Content[0].Text; ;

            if (openAIMessage is SystemChatMessage)
                Role = AIMessageRole.System;
            else if (openAIMessage is UserChatMessage)
                Role = AIMessageRole.User;
            else if (openAIMessage is AssistantChatMessage)
                Role = AIMessageRole.Assistant;
        }

        public ChatMessage ToOpenAIChatMessage()
        {
            return Role switch
            {
                AIMessageRole.System => new SystemChatMessage(Content),
                AIMessageRole.User => new UserChatMessage(Content),
                AIMessageRole.Assistant => new AssistantChatMessage(Content),
                _ => throw new NotSupportedException($"Unsupported message role: {Role}")
            };
        }
    }
}
