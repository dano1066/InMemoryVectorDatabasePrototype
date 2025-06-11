# InMemoryVectorStore

This project provides a simple vector store example using various AI services. API keys are no longer embedded in the source code. Configure the following environment variables before running:

- `OPENAI_API_KEY`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_ENDPOINT`
- `DEEPSEEK_API_KEY`

You can copy `.env.example` to `.env` and fill in your keys.

When you run the application, it will automatically load variables from a `.env`
file if present using the `DotNetEnv` package. This allows you to keep your API
keys in a local file without exporting environment variables each time.
