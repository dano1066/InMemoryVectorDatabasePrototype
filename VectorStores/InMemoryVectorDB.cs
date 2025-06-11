using Build5Nines.SharpVector;
using Build5Nines.SharpVector.Data;
using Build5Nines.SharpVector.OpenAI;
using InMemoryVectorStore.Models.VectorStores;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.VectorStores
{
    public class InMemoryVectorDB : IVectorDB
    {
        private readonly BasicOpenAIMemoryVectorDatabase _vectorDb;
        private readonly TextDataLoader<int, string> _loader;
        private VectorDBOptions _options;
        private bool _isInitialized = false;
        private static readonly string DefaultCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InMemoryVectorStore", "Cache");

        //public InMemoryVectorDB()
        //{
        //    _vectorDb = new BasicMemoryVectorDatabase();
        //    _loader = new TextDataLoader<int, string>(_vectorDb);
        //}

        public InMemoryVectorDB(EmbeddingClient embeddingClient)
        {
            _vectorDb = new BasicOpenAIMemoryVectorDatabase(embeddingClient);
            _loader = new TextDataLoader<int, string>(_vectorDb);
            Directory.CreateDirectory(DefaultCachePath);
        }

        public async Task<bool> InitializeAsync(VectorDBOptions options)
        {
            _options = options ?? new VectorDBOptions();

            if (string.IsNullOrEmpty(_options.IndexName))
            {
                throw new ArgumentException("Database identifier cannot be empty or null.");
            }

            var cacheFilePath = Path.Combine(DefaultCachePath, $"cache-{_options.IndexName}.zip");

            if (File.Exists(cacheFilePath))
            {
                try
                {
                    Directory.CreateDirectory(DefaultCachePath);
                    using var cacheStream = File.OpenRead(cacheFilePath);
                    await _vectorDb.DeserializeFromJsonStreamAsync(cacheStream);
                    _isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to load vector database from cache: {ex.Message}", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"No database found with identifier '{_options.IndexName}'. Please train the database first.", cacheFilePath);
            }
        }

        public async Task<DocumentsAddResult> BuildDocumentIndex(IEnumerable<DocumentToProcess> documents, ChunkingOptions chunkingOptions, string indexName)
        {
            var result = new DocumentsAddResult
            {
                Success = true
            };

            foreach (var document in documents)
            {
                try
                {
                    var textChunkingOptions = MapToTextChunkingOptions(chunkingOptions, document);

                    var loadResult = _loader.AddDocument(document.Content, textChunkingOptions);

                    result.DocumentsAdded++;
                    result.TotalChunksAdded += loadResult.Count();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add(new DocumentAddError
                    {
                        FileName = document.FileName,
                        ErrorMessage = ex.Message
                    });
                }
            }

            try
            {
                Directory.CreateDirectory(DefaultCachePath);
                var cacheFilePath = Path.Combine(DefaultCachePath, $"cache-{indexName}.zip");
                using var cacheStream = File.Create(cacheFilePath);
                await _vectorDb.SerializeToJsonStreamAsync(cacheStream);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the operation
                result.ErrorMessage = $"Documents were added but caching failed: {ex.Message}";
            }
            

            return result;
        }

        public async Task<SearchResult> SearchAsync(string query, float threshold = 0.2f, int pageCount = 5)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Vector database has not been initialized. Call InitializeAsync first.");
            }

            var results = await _vectorDb.SearchAsync(query, threshold: threshold, pageCount: pageCount);

            return new SearchResult
            {
                TextResults = results.Texts.Select(r => new TextSearchResult
                {
                    Text = r.Text,
                    Metadata = r.Metadata,
                    Score = r.VectorComparison
                }).ToList()
            };
        }

        public async Task<List<VectorDBInfo>> ListDatabasesAsync()
        {
            var result = new List<VectorDBInfo>();

            var cacheFiles = Directory.GetFiles(DefaultCachePath, "cache-*.zip");

            foreach (var cacheFile in cacheFiles)
            {
                // Extract the database identifier from the filename
                var fileName = Path.GetFileName(cacheFile);
                if (fileName.StartsWith("cache-") && fileName.EndsWith(".zip"))
                {
                    var dbIdentifier = fileName.Substring(6, fileName.Length - 10); // Remove "cache-" prefix and ".zip" suffix

                    result.Add(new VectorDBInfo
                    {
                        Name = dbIdentifier
                    });
                }
            }

            return result;
        }


        private TextChunkingOptions<string> MapToTextChunkingOptions(ChunkingOptions options, DocumentToProcess document)
        {
            return new TextChunkingOptions<string>
            {
                Method = MapChunkingMethod(options.Method),
                ChunkSize = options.ChunkSize,
                OverlapSize = options.OverlapSize,
                RetrieveMetadata = options.RetrieveMetadata ?? (chunk =>
                {
                    var metadata = new Dictionary<string, string>(document.Metadata);
                    if (!metadata.ContainsKey("filename") && !string.IsNullOrEmpty(document.FileName))
                    {
                        metadata["filename"] = document.FileName;
                    }
                    return System.Text.Json.JsonSerializer.Serialize(metadata);
                })
            };
        }

        private TextChunkingMethod MapChunkingMethod(ChunkingMethod method)
        {
            return method switch
            {
                ChunkingMethod.FixedLength => TextChunkingMethod.FixedLength,
                ChunkingMethod.Paragraph => TextChunkingMethod.Paragraph,
                _ => TextChunkingMethod.FixedLength
            };
        }

        public static string GenerateCacheIdentifierFromFiles(IEnumerable<string> filePaths)
        {
            using var sha = SHA256.Create();
            var builder = new StringBuilder();

            foreach (var file in filePaths.OrderBy(f => f))
            {
                var info = new FileInfo(file);
                builder.Append(file);
                builder.Append(info.Length);
                builder.Append(info.LastWriteTimeUtc.Ticks);
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
