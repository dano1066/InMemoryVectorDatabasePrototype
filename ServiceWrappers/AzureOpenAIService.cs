using Azure;
using Azure.AI.OpenAI;
using InMemoryVectorStore.Constants;
using InMemoryVectorStore.Models.AI;
using InMemoryVectorStore.Models.AI.Messages;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.ServiceWrappers
{
    public class AzureOpenAIServiceOptions
    {
        // Secrets and endpoints are loaded from environment variables
        public string ApiKey { get; set; } =
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
        public string Endpoint { get; set; } =
            Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
        public string DefaultEmbeddingDeployment { get; set; } = "text-embedding-3-small";
        public string DefaultChatDeployment { get; set; } = "gpt-4o-mini";
    }

    public class AzureOpenAIService : IAIService
    {
        private readonly AzureOpenAIClient _client;
        private readonly AzureOpenAIServiceOptions _options;
        private readonly ConcurrentDictionary<string, AIUsage> _tokenUsageByModel;

        public AzureOpenAIService(AzureOpenAIServiceOptions options)
        {
            _options = options;
            _client = new AzureOpenAIClient(new Uri(_options.Endpoint),
                new AzureKeyCredential(_options.ApiKey));
            _tokenUsageByModel = new ConcurrentDictionary<string, AIUsage>();
        }

        public AIEmbedding CreateEmbeddings(string text, string? model = null)
        {
            var deploymentName = model ?? _options.DefaultEmbeddingDeployment;

            //var options = new EmbeddingGenerationOptions()
            //{
            //    Dimensions = 1536
            //};

            var response = _client.GetEmbeddingClient(_options.DefaultEmbeddingDeployment).GenerateEmbedding(text);

            return new AIEmbedding(response);
        }

        public string AnswerQuestion(List<string> contextChunks, string question)
        {
            var messages = new List<AIMessage>
            {
                new AIMessage(AIMessageRole.System, "You are a helpful assistant that answers user questions based on the previous context messages."),
            };

            foreach (var item in contextChunks)
            {
                messages.Add(new AIMessage(AIMessageRole.User, item));
            }

            messages.Add(new AIMessage(AIMessageRole.User, "Answer This Question: " + question));

            var result = GetChatCompletion(messages);

            if (result.Content.Count > 0 && !string.IsNullOrEmpty(result.Content[0].Text))
            {
                return result.Content[0].Text;
            }
            return "";
        }

        public string OptimiseUserQuery(string query)
        {
            var messages = new List<AIMessage>
            {
                new AIMessage(AIMessageRole.System, SystemPrompts.QueryOptimizer),
                new AIMessage(AIMessageRole.User, query)
            };

            var result = GetChatCompletion(messages);

            if (result.Content.Count > 0 && !string.IsNullOrEmpty(result.Content[0].Text))
            {
                return result.Content[0].Text;
            }
            return "";
        }

        public AICompletion GetChatCompletion(List<AIMessage> messages, string? model = null, double temperature = 0.7, int? maxTokens = null)
        {
            var deploymentName = model ?? _options.DefaultChatDeployment;

            var chatMessages = messages.Select(m => m.ToOpenAIChatMessage()).ToList();

            var response = _client.GetChatClient(_options.DefaultChatDeployment).CompleteChat(chatMessages);

            if (response.Value.Usage != null)
            {
                TrackTokenUsage(deploymentName, response.Value.Usage.InputTokenCount, response.Value.Usage.OutputTokenCount);
            }

            return new AICompletion(response.Value);
        }

        public Dictionary<string, AIUsage> GetTokenUsage()
        {
            return _tokenUsageByModel.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public decimal CalculateTotalCost()
        {
            decimal totalCost = 0;

            foreach (var usage in _tokenUsageByModel)
            {
                if (GPTConstants.ModelCosts.TryGetValue(usage.Key, out var modelCost))
                {
                    decimal inputCost = (usage.Value.PromptTokens / 1000.0m) * modelCost.InputTokenCost;
                    decimal outputCost = (usage.Value.CompletionTokens / 1000.0m) * modelCost.OutputTokenCost;
                    totalCost += inputCost + outputCost;
                }
            }

            return totalCost;
        }

        public Dictionary<string, decimal> GetCostBreakdownByModel()
        {
            var costBreakdown = new Dictionary<string, decimal>();

            foreach (var usage in _tokenUsageByModel)
            {
                if (GPTConstants.ModelCosts.TryGetValue(usage.Key, out var modelCost))
                {
                    decimal inputCost = (usage.Value.PromptTokens / 1000.0m) * modelCost.InputTokenCost;
                    decimal outputCost = (usage.Value.CompletionTokens / 1000.0m) * modelCost.OutputTokenCost;
                    costBreakdown[usage.Key] = inputCost + outputCost;
                }
                else
                {
                    costBreakdown[usage.Key] = 0;
                }
            }

            return costBreakdown;
        }

        private void TrackTokenUsage(string model, int promptTokens, int completionTokens)
        {
            _tokenUsageByModel.AddOrUpdate(
                model,
                new AIUsage
                {
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = promptTokens + completionTokens
                },
                (key, existingUsage) =>
                {
                    existingUsage.PromptTokens += promptTokens;
                    existingUsage.CompletionTokens += completionTokens;
                    existingUsage.TotalTokens += (promptTokens + completionTokens);
                    return existingUsage;
                });
        }
    }
}
