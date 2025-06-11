using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Collections;
using System.Collections.Concurrent;
using InMemoryVectorStore.Constants;
using InMemoryVectorStore.Models.AI.Messages;
using InMemoryVectorStore.Models.AI;
using InMemoryVectorStore.ServiceWrappers;

namespace InMemoryVectorStore
{
    public class OpenAIServiceOptions
    {
        // API key is loaded from the environment to avoid hard coding secrets
        public string ApiKey { get; set; } =
            Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        public string DefaultEmbeddingModel { get; set; } = "text-embedding-3-small"; //text-embedding-3-large
        public string DefaultChatModel { get; set; } = "gpt-4.1-nano"; //"gpt-4o-mini"; //"gpt-4.1-mini";
    }

    public class OpenAIService : IAIService
    {
        private readonly OpenAIClient _client;
        private readonly OpenAIServiceOptions _options;
        private readonly ConcurrentDictionary<string, AIUsage> _tokenUsageByModel;

        #region Constructors
        public OpenAIService(OpenAIServiceOptions options)
        {
            _options = options;
            _client = new OpenAIClient(_options.ApiKey);
            _tokenUsageByModel = new ConcurrentDictionary<string, AIUsage>();
        }

        public OpenAIService(OpenAIServiceOptions options, OpenAIClient client)
        {
            _options = options;
            _client = client;
            _tokenUsageByModel = new ConcurrentDictionary<string, AIUsage>();
        }
        #endregion

        #region Embeddings
        public AIEmbedding CreateEmbeddings(string text, string? model = null)
        {
            var embedModel = model ?? _options.DefaultEmbeddingModel;

            OpenAIEmbedding response = _client.GetEmbeddingClient(embedModel).GenerateEmbedding(text);

            return new AIEmbedding(response);
        }
        #endregion

        #region Completions

        public string AnswerQuestion(List<string> contextChunks, string question)
        {
            var messages = new List<AIMessage>()
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
            var chatModel = model ?? _options.DefaultChatModel;

            var completionOptions = new ChatCompletionOptions
            {
                Temperature = (float)temperature
            };

            if (maxTokens.HasValue)
            {
                completionOptions.MaxOutputTokenCount = maxTokens.Value;
            }

            var chatMessages = messages.Select(m => m.ToOpenAIChatMessage()).ToList();

            var response = _client.GetChatClient(chatModel).CompleteChat(
                chatMessages,
                completionOptions).Value;

            if (response.Usage != null)
            {
                TrackTokenUsage(chatModel, response.Usage.InputTokenCount, response.Usage.OutputTokenCount);
            }

            return new AICompletion(response);
        }
        #endregion

        #region Helpers
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
                    costBreakdown[usage.Key] = 0; // Model cost not found
                }
            }

            return costBreakdown;
        }
        #endregion

        #region Private

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
        #endregion
    }

    public class ModelCost
    {
        public decimal InputTokenCost { get; set; }
        public decimal OutputTokenCost { get; set; }
    }
}
