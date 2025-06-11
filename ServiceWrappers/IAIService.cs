using InMemoryVectorStore.Models.AI;
using InMemoryVectorStore.Models.AI.Messages;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.ServiceWrappers
{
    public interface IAIService
    {

        AIEmbedding CreateEmbeddings(string text, string? model = null);


        string AnswerQuestion(List<string> contextChunks, string question);


        string OptimiseUserQuery(string query);

        AICompletion GetChatCompletion(List<AIMessage> messages, string? model = null, double temperature = 0.7, int? maxTokens = null);

        Dictionary<string, AIUsage> GetTokenUsage();


        decimal CalculateTotalCost();

        Dictionary<string, decimal> GetCostBreakdownByModel();
    }
}
