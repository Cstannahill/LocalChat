# Quickstart

This project is a local-first AI orchestration backend for agent-style systems.

## 1. Prerequisites

- .NET 9 SDK
- Optional providers you plan to use:
- `Ollama`
- `OpenRouter`
- `HuggingFace`
- `LlamaCpp`
- `Kokoro` or `Qwen` TTS provider
- `ComfyUI` (for image workflows)

## 2. Configure Provider Keys

Local secret-bearing configuration should stay in `src/LocalChat.Api/appsettings.Development.json` (gitignored).

Set keys when using remote or authenticated providers:

- `OpenRouter:ApiKey`
- `HuggingFace:ApiKey`
- `KokoroTts:ApiKey` (if your deployment requires it)
- `QwenTts:ApiKey` (if your deployment requires it)
- `LlamaCpp:ApiKey` (only if your server requires auth)

`src/LocalChat.Api/appsettings.json` is the public-safe baseline and should not contain real keys.

You can also set keys via environment variables:

- `OpenRouter__ApiKey`
- `HuggingFace__ApiKey`
- `KokoroTts__ApiKey`
- `QwenTts__ApiKey`
- `LlamaCpp__ApiKey`

## 3. Run the API

```powershell
dotnet run --project src/LocalChat.Api/LocalChat.Api.csproj
```

Default local URL:

- `http://localhost:5170`

## 4. Validate

- Health: `GET /health`
- Swagger (Development): `http://localhost:5170/swagger`