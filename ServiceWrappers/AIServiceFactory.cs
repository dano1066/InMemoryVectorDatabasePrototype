using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.ServiceWrappers
{
    public static class AIServiceFactory
    {
        public static IAIService CreateOpenAIService() //todo: handle the service options better to maybe load from config file or somewhere more secure
        {
            return new OpenAIService(new OpenAIServiceOptions());
        }
        public static IAIService CreateAzureOpenAIService()
        {
            return new AzureOpenAIService(new AzureOpenAIServiceOptions());
        }

        //Note: Azure supports deepseek and many other AI models
        public static IAIService CreateDeepSeekAIService()
        {
            var options = new DeepSeekAIServiceOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty,
                DefaultEmbeddingModel = "deepseek-embed",
                DefaultChatModel = "deepseek-chat"
            };
            return new DeepSeekAIService(options);
        }
    }
}
