# InMemoryVectorStore

## Introduction

**InMemoryVectorStore** is a minimal command‑line tool written in C#/.NET that demonstrates how to build and query a vector database entirely in memory. The application allows you to process local documents, create vector embeddings for them and then ask questions using your choice of AI provider. Everything runs locally without requiring a paid search service such as Azure Cognitive Search or AWS Kendra.

## Features

- Works with **Azure OpenAI**, **OpenAI** or **DeepSeek** APIs
- Builds an in‑memory vector database from your own documents
- Supports *Retrieval Augmented Generation (RAG)* mode or *Full Context* mode
- Stores vector data on disk as a small cache file so it can be reloaded quickly
- Simple `.env` configuration for API keys

## Requirements

Before running the application, set the following environment variables (or populate a `.env` file):

- `OPENAI_API_KEY`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_ENDPOINT`
- `DEEPSEEK_API_KEY`

Copy `.env.example` to `.env` and fill in the values for the providers you want to use. The `.env` file will be loaded automatically when the app starts.

## Usage

Run the project using the .NET CLI:

```bash
# restore dependencies and build
 dotnet build

# start the program
 dotnet run --project InMemoryVectorStore.csproj
```

When launched, you will be asked which AI provider to use followed by a menu to select the desired mode:

1. **RAG Mode** – query a trained vector database and get answers based on the most relevant chunks
2. **Context Mode** – load whole documents into the chat context and ask questions against the full text
3. **Train Mode** – process a folder of documents and create a new vector database
4. **List Databases** – show all cached databases available on your machine

### Train Mode

Provide a folder path containing your source files (PDF, DOCX, TXT, etc.) and a database identifier. The tool will parse the files, create embeddings and store them in a local cache file. This cache is later loaded by RAG mode.

### RAG Mode

After training, choose *RAG Mode* and specify the same database identifier. The application searches the in‑memory vectors for the best matching chunks and passes them to your selected AI service to generate an answer.

### Context Mode

Context mode reads the contents of the provided files directly into the chat messages without using the vector database. This is useful for small sets of documents that fit within the model’s context window.

## Why In-Memory?

By storing vectors in memory and caching them locally, you can experiment with retrieval based workflows without the cost of hosted search solutions. It’s ideal for prototypes, demos or learning how RAG systems work.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

