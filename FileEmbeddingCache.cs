class FileEmbeddingCache
{
    private readonly string _indexPath;
    private readonly string _vectorFolder;
    private readonly Dictionary<string, string> _index;

    public FileEmbeddingCache(string cacheDir)
    {
        _vectorFolder = Path.Combine(cacheDir, "chunks");
        _indexPath = Path.Combine(cacheDir, "embeddings-index.json");

        Directory.CreateDirectory(_vectorFolder);
        _index = File.Exists(_indexPath)
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_indexPath)) ?? new()
            : new();
    }

    public bool TryGet(string hash, out float[]? embedding)
    {
        embedding = null;
        if (_index.TryGetValue(hash, out var fileName))
        {
            var fullPath = Path.Combine(_vectorFolder, fileName);
            if (File.Exists(fullPath))
            {
                var bytes = File.ReadAllBytes(fullPath);
                embedding = new float[bytes.Length / sizeof(float)];
                Buffer.BlockCopy(bytes, 0, embedding, 0, bytes.Length);
                return true;
            }
            else
            {
                // Cache file is missing even though index exists
                _index.Remove(hash);
            }
        }
        return false;
    }

    public void Add(string hash, float[] embedding)
    {
        var fileName = $"{hash}.vec";
        var fullPath = Path.Combine(_vectorFolder, fileName);

        var bytes = new byte[embedding.Length * sizeof(float)];
        Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
        File.WriteAllBytes(fullPath, bytes);

        _index[hash] = fileName;
    }

    public void Save()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_index, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_indexPath, json);
    }
}
