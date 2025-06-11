using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Build5Nines.SharpVector.Data;
using Build5Nines.SharpVector.OpenAI;
using InMemoryVectorStore;
using InMemoryVectorStore.DocumentParsers;
using InMemoryVectorStore.Models.AI.Messages;
using InMemoryVectorStore.Models.VectorStores;
using InMemoryVectorStore.ServiceWrappers;
using InMemoryVectorStore.VectorStores;
using OpenAI;
using OpenAI.Chat;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env if present
        Env.Load();

        // Initialize services through factories
        IAIService aiService = AIServiceFactory.CreateAzureOpenAIService();
        IVectorDB vectorDb = VectorDBFactory.CreateInMemoryVectorDB();

        Console.WriteLine("Select mode:");
        Console.WriteLine("1. RAG Mode (Retrieval Augmented Generation)");
        Console.WriteLine("2. Context Mode (Full document context)");
        Console.WriteLine("3. Train Mode (Process documents and create vector database)");
        Console.WriteLine("4. List Databases (Show available vector databases)");
        Console.WriteLine("Enter choice (1, 2, 3, or 4): ");

        string choice = Console.ReadLine()?.Trim();
        string mode = choice switch
        {
            "2" => "context",
            "3" => "train",
            "4" => "list",
            _ => "rag"
        };

        if (mode == "train")
        {
            Console.WriteLine("Enter folder path containing documents to train:");
            var folderPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine("Invalid folder path.");
                return;
            }

            Console.WriteLine("Enter a database identifier for this document set:");
            var dbIdentifier = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(dbIdentifier))
            {
                Console.WriteLine("Database identifier cannot be empty.");
                return;
            }

            await TrainMode(vectorDb, folderPath, dbIdentifier);
        }
        else if (mode == "rag")
        {
            Console.WriteLine("Enter database identifier to use:");
            var dbIdentifier = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(dbIdentifier))
            {
                Console.WriteLine("Database identifier cannot be empty.");
                return;
            }

            await RagMode(aiService, vectorDb, dbIdentifier);
        }
        else if (mode == "context")
        {
            Console.WriteLine("Enter folder path containing documents:");
            var folderPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Console.WriteLine("Invalid folder path.");
                return;
            }

            var files = Directory.GetFiles(folderPath);
            await ContextMode(aiService, files);
        }
        else if (mode == "list")
        {
            await ListDatabasesMode(vectorDb);
        }
    }

    public static async Task TrainMode(IVectorDB vectorDb, string folderPath, string dbIdentifier)
    {
        Console.WriteLine("\nInitializing vector database...");

        try
        {
            await vectorDb.InitializeAsync(new VectorDBOptions());
        }
        catch (Exception)
        {
            // For training mode, it's okay if the database doesn't exist yet
            // We'll create it when we add documents
            Console.WriteLine("Creating new vector database...");
        }

        var files = Directory.GetFiles(folderPath);
        Console.WriteLine($"Found {files.Length} files to process.");

        var documents = new List<DocumentToProcess>();
        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileName(file);
                Console.WriteLine("Reading: " + fileName);

                var parser = DocumentParserFactory.GetParser(file);
                var content = await parser.GetContentAsync(file);

                documents.Add(new DocumentToProcess
                {
                    FileName = fileName,
                    Content = content,
                    Metadata = new Dictionary<string, string>
                    {
                        { "filename", fileName }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read {file}: {ex.Message}");
            }
        }

        if (documents.Count == 0)
        {
            Console.WriteLine("No valid documents found to process.");
            return;
        }

        Console.WriteLine("\nProcessing documents and creating vector embeddings...");
        var result = await vectorDb.BuildDocumentIndex(documents, new ChunkingOptions
        {
            Method = ChunkingMethod.FixedLength,
            ChunkSize = 5000,
            OverlapSize = 400
        }, dbIdentifier);

        Console.WriteLine($"\nTraining completed:");
        Console.WriteLine($"Documents added: {result.DocumentsAdded}");
        Console.WriteLine($"Total chunks created: {result.TotalChunksAdded}");

        if (!result.Success)
        {
            Console.WriteLine("Errors occurred during processing:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.FileName}: {error.ErrorMessage}");
            }
        }

        Console.WriteLine($"\nVector database '{dbIdentifier}' is now ready for use.");
    }

    public static async Task RagMode(IAIService aiService, IVectorDB vectorDb, string dbIdentifier)
    {
        Console.WriteLine("\nInitializing vector database...");

        try
        {
            var success = await vectorDb.InitializeAsync(new VectorDBOptions()
            {
                IndexName = dbIdentifier
            });

            Console.WriteLine("Vector database loaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Please make sure you have trained the database with this identifier first.");
            return;
        }

        Console.WriteLine("\nReady to search! Type a query (or 'exit' to quit):");

        while (true)
        {
            Console.Write("\nQuery: ");
            var query = Console.ReadLine();

            if (query?.Trim().ToLower() == "exit")
                break;

            Console.WriteLine("Searching for results");
            var results = await vectorDb.SearchAsync(query, threshold: 0.2f, pageCount: 5);

            if (results.TextResults.Count == 0)
            {
                Console.WriteLine("No matching results.");
                continue;
            }

            Console.WriteLine($"\nResults: {results.TextResults.Count} chunks found");

            var chunks = results.TextResults.Select(r => r.Text).ToList();

            Console.WriteLine("\nGenerating Answer...");
            var answer = aiService.AnswerQuestion(chunks, query);
            Console.WriteLine(answer);
            OutputCosts(aiService);
        }

        Console.WriteLine("Done.");
    }

    public static async Task ContextMode(IAIService aiService, string[] files)
    {
        Console.WriteLine("\nLoading documents for Context Mode...");
        var messages = new List<AIMessage>();
        // Add a system message to explain the task
        messages.Add(new AIMessage(AIMessageRole.System, @"You are a helpful assistant that answers questions based on the provided document context. If the users question is vague or unclear, you can 
attempt to answer the question and at the end clarify if this is what they meant. "));

        foreach (var file in files)
        {
            try
            {
                var parser = DocumentParserFactory.GetParser(file);
                var content = await parser.GetContentAsync(file);
                var fileName = Path.GetFileName(file);
                Console.WriteLine($"Reading: {fileName}");

                const int maxChunkSize = 10000;
                for (int i = 0; i < content.Length; i += maxChunkSize)
                {
                    int length = Math.Min(maxChunkSize, content.Length - i);
                    string chunk = content.Substring(i, length);
                    messages.Add(new AIMessage(AIMessageRole.User, chunk));
                }

                Console.WriteLine($"Loaded: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read {file}: {ex.Message}");
            }
        }
        Console.WriteLine($"Loaded {messages.Count} chunks from {files.Length} files.");
        Console.WriteLine("\nContext Mode - Ready to ask questions! Type a query (or 'exit' to quit):");

        while (true)
        {
            Console.Write("\nQuery: ");
            var query = Console.ReadLine();

            if (query?.Trim().ToLower() == "exit")
                break;

            Console.WriteLine("\nGenerating Answer with full context...");

            messages.Add(new AIMessage(AIMessageRole.User, query));

            try
            {
                var response = aiService.GetChatCompletion(messages);

                Console.WriteLine(response.Content?.FirstOrDefault()?.Text);

                OutputCosts(aiService);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating response: {ex.Message}");

                // If context is too large, we might need to reduce it
                if (ex.Message.Contains("maximum context length"))
                {
                    Console.WriteLine("The context is too large. Try reducing the number of documents or chunk size.");
                }
            }
        }
    }

    public static async Task ListDatabasesMode(IVectorDB vectorDb)
    {
        Console.WriteLine("\nListing available vector databases...");

        var databases = await vectorDb.ListDatabasesAsync();

        if (databases.Count == 0)
        {
            Console.WriteLine("No vector databases found.");
            Console.WriteLine("Use Train Mode (option 3) to create a new database.");
            return;
        }

        Console.WriteLine($"Found {databases.Count} vector databases:");
        Console.WriteLine("------------------------------------");

        foreach (var db in databases)
        {
            Console.WriteLine($"- {db.Name}");
        }

        Console.WriteLine("------------------------------------");
        Console.WriteLine("Use these identifiers with RAG Mode (option 1) to query a database.");
    }


    public static void OutputCosts(IAIService aiService)
    {
        Console.WriteLine($"\nTotal API cost: ${Math.Round(aiService.CalculateTotalCost(), 4)}");
        var tokenUsage = aiService.GetTokenUsage();

        // Calculate total input and output tokens across all models
        int totalInputTokens = tokenUsage.Values.Sum(usage => usage.PromptTokens);
        int totalOutputTokens = tokenUsage.Values.Sum(usage => usage.CompletionTokens);

        // Output the combined totals
        Console.WriteLine($"Total Input Tokens: {totalInputTokens}");
        Console.WriteLine($"Total Output Tokens: {totalOutputTokens}");
        Console.WriteLine($"Total Tokens: {totalInputTokens + totalOutputTokens}");
    }

}