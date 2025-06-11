using DeepSeek.Core.Models;
using DeepSeek.Core;
using InMemoryVectorStore.Models.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InMemoryVectorStore.Constants;
using InMemoryVectorStore.Models.AI.Messages;

namespace InMemoryVectorStore.ServiceWrappers
{
    public class DeepSeekAIServiceOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultEmbeddingModel { get; set; } = "deepseek-embed";
        public string DefaultChatModel { get; set; } = Constant.Model.ChatModel;
        public Dictionary<string, ModelCost> ModelCosts { get; set; } = new Dictionary<string, ModelCost>
        {
            // DeepSeek Chat Models
            { Constant.Model.ChatModel, new ModelCost { InputTokenCost = 0.0005m, OutputTokenCost = 0.0015m } },
            { "deepseek-chat", new ModelCost { InputTokenCost = 0.0005m, OutputTokenCost = 0.0015m } },
            { "deepseek-coder", new ModelCost { InputTokenCost = 0.0005m, OutputTokenCost = 0.0015m } },
            
            // DeepSeek Embedding Models
            { "deepseek-embed", new ModelCost { InputTokenCost = 0.0001m, OutputTokenCost = 0.0001m } },
        };
    }

    public class DeepSeekAIService : IAIService
    {
        private readonly DeepSeekClient _client;
        private readonly DeepSeekAIServiceOptions _options;
        private readonly ConcurrentDictionary<string, AIUsage> _tokenUsageByModel;

        public DeepSeekAIService(DeepSeekAIServiceOptions options)
        {
            _options = options;
            _client = new DeepSeekClient(_options.ApiKey);
            _tokenUsageByModel = new ConcurrentDictionary<string, AIUsage>();
        }

        public AIEmbedding CreateEmbeddings(string text, string? model = null)
        {
            throw new NotImplementedException("Deepseek does not support embedding functionality.");
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
            var chatModel = model ?? _options.DefaultChatModel;

            // Convert our AIMessages to DeepSeek Messages
            var deepSeekMessages = messages.Select(m =>
            {
                switch (m.Role)
                {
                    case AIMessageRole.System:
                        return Message.NewSystemMessage(m.Content);
                    case AIMessageRole.User:
                        return Message.NewUserMessage(m.Content);
                    case AIMessageRole.Assistant:
                        return Message.NewAssistantMessage(m.Content);
                    default:
                        throw new NotSupportedException($"Unsupported message role: {m.Role}");
                }
            }).ToList();

            // Create the chat request
            var request = new ChatRequest
            {
                Messages = deepSeekMessages,
                Model = chatModel,
                Temperature = (float)temperature
            };

            if (maxTokens.HasValue)
            {
                request.MaxTokens = maxTokens.Value;
            }

            // Make the API call
            var response = _client.ChatAsync(request, CancellationToken.None).GetAwaiter().GetResult();

            if (response == null)
            {
                throw new Exception($"Failed to get chat completion: {_client.ErrorMsg}");
            }

            // Track token usage
            if (response.Usage != null)
            {
                TrackTokenUsage(chatModel, response.Usage.PromptTokens, response.Usage.CompletionTokens);
            }

            // Convert to our AICompletion format
            var content = response.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;
            var usage = response.Usage != null
                ? new AIUsage(response.Usage.PromptTokens, response.Usage.CompletionTokens)
                : null;

            return new AICompletion(content, usage);
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
                if (_options.ModelCosts.TryGetValue(usage.Key, out var modelCost))
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
                if (_options.ModelCosts.TryGetValue(usage.Key, out var modelCost))
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
