using System;

namespace InMemoryVectorStore.Tests
{
    internal sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(Path);
        }

        public string CreateFile(string name, string contents)
        {
            var full = System.IO.Path.Combine(Path, name);
            System.IO.File.WriteAllText(full, contents);
            return full;
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Path, true);
            }
            catch
            {
            }
        }
    }
}
