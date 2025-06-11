using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Constants
{
    public static class GPTConstants
    {
        public static Dictionary<string, ModelCost> ModelCosts { get; set; } = new Dictionary<string, ModelCost>
        {
            // GPT-3.5 Models
            { "gpt-3.5-turbo", new ModelCost { InputTokenCost = 0.0005m, OutputTokenCost = 0.0015m } },
            { "gpt-3.5-turbo-instruct", new ModelCost { InputTokenCost = 0.0015m, OutputTokenCost = 0.0020m } },
            { "gpt-3.5-turbo-16k", new ModelCost { InputTokenCost = 0.0030m, OutputTokenCost = 0.0040m } },

            // GPT-4 Models
            { "gpt-4", new ModelCost { InputTokenCost = 0.03m, OutputTokenCost = 0.06m } },
            { "gpt-4-32k", new ModelCost { InputTokenCost = 0.06m, OutputTokenCost = 0.12m } },
            { "gpt-4-turbo", new ModelCost { InputTokenCost = 0.01m, OutputTokenCost = 0.03m } },

            // GPT-4.1 Models
            { "gpt-4.1", new ModelCost { InputTokenCost = 0.0020m, OutputTokenCost = 0.0080m } },
            { "gpt-4.1-mini", new ModelCost { InputTokenCost = 0.0004m, OutputTokenCost = 0.0016m } },
            { "gpt-4.1-nano", new ModelCost { InputTokenCost = 0.0001m, OutputTokenCost = 0.0004m } },

            // GPT-4o Models
            { "gpt-4o", new ModelCost { InputTokenCost = 0.0050m, OutputTokenCost = 0.0200m } },
            { "gpt-4o-mini", new ModelCost { InputTokenCost = 0.0006m, OutputTokenCost = 0.0024m } },

            // o1 and o3 Models
            { "o1", new ModelCost { InputTokenCost = 0.0150m, OutputTokenCost = 0.0600m } },
            { "o1-pro", new ModelCost { InputTokenCost = 0.1500m, OutputTokenCost = 0.6000m } },
            { "o1-mini", new ModelCost { InputTokenCost = 0.0011m, OutputTokenCost = 0.0044m } },
            { "o3-mini", new ModelCost { InputTokenCost = 0.0011m, OutputTokenCost = 0.0044m } },

            // Embedding Models
            { "text-embedding-ada-002", new ModelCost { InputTokenCost = 0.0001m, OutputTokenCost = 0.0001m } },
            { "text-embedding-3-small", new ModelCost { InputTokenCost = 0.00002m, OutputTokenCost = 0.00002m } },
            { "text-embedding-3-large", new ModelCost { InputTokenCost = 0.00013m, OutputTokenCost = 0.00013m } }
        };
    }
}
