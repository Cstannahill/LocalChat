# LocalChat AI Orchestration Backend

LocalChat is a .NET 9 backend for local-first AI orchestration with:

- multi-provider LLM routing (`Ollama`, `OpenRouter`, `HuggingFace`, `LlamaCpp`)
- conversation memory and session-state lifecycle tooling
- retrieval-enhanced prompt composition
- streaming chat responses over Server-Sent Events (SSE)
- text-to-speech (Kokoro or Qwen provider integration)
- contextual image prompting and image generation job orchestration (ComfyUI)
- import/export, inspection, and maintenance/admin endpoints

This repository contains the API plus application/domain/infrastructure layers and automated tests.

## Terminology Note

Some API/domain names are intentionally retained from the current implementation (for example `chat`, `conversations`, `agents`, and `userProfiles`). Functionally, these map to reusable AI orchestration concepts:

- `agent` -> assistant/agent profile
- `userProfile` -> end-user profile/context
- `conversation` -> stateful interaction session

Naming alignment is planned toward more generic agent platform terminology while preserving current behavior and data model intent.

## Tech Stack

- .NET `9.0`
- ASP.NET Core Minimal APIs
- Entity Framework Core + SQLite
- Swagger/OpenAPI (enabled in Development)
- xUnit test suite (domain, application, infrastructure)

## Solution Layout

- `src/LocalChat.Api` - web host, minimal API endpoints, middleware, static assets
- `src/LocalChat.Application` - orchestration and use-case logic
- `src/LocalChat.Domain` - entities, value objects, enums
- `src/LocalChat.Infrastructure` - persistence, provider adapters, background workers
- `src/LocalChat.Contracts` - request/response contracts
- `tests/*` - test projects

## Quick Start

1. Install .NET 9 SDK.
2. Configure `src/LocalChat.Api/appsettings.json` (or environment variables).
3. Start dependencies you intend to use (for example Ollama, TTS server, ComfyUI).
4. Run the API:

```powershell
dotnet run --project src/LocalChat.Api/LocalChat.Api.csproj
```

Default dev URL is `http://localhost:5170` (see `launchSettings.json`).

Detailed setup and provider key placement: [QUICKSTART.md](QUICKSTART.md)

### Health Check

```text
GET /health
```

### Swagger (Development Only)

- `http://localhost:5170/swagger`

## Configuration

Configuration is loaded from ASP.NET configuration sources (`appsettings*.json`, environment variables, etc.).

High-impact sections:

- `ConnectionStrings:DefaultConnection`
- `Ollama`, `OpenRouter`, `HuggingFace`, `LlamaCpp`
- `Speech`, `KokoroTts`, `QwenTts`
- `ComfyUi`
- `Summaries`, `Retrieval`, `MemoryProposals`, `SessionStateCleanup`
- `ConversationBackgroundWork`, `BackgroundMemoryProposals`
- `Inspection`, `RequestFlowLogging`

Detailed guidance: [docs/CONFIGURATION.md](docs/CONFIGURATION.md)

## Professional Use Cases

The platform architecture is adaptable to multiple production AI scenarios:

- Customer service AI systems (multi-provider LLM routing with memory)
- Educational tutoring systems (retrieval-enhanced prompt composition)
- Healthcare information assistants (with additional compliance, privacy, and access controls)
- Enterprise knowledge assistants (RAG over internal memory/lore/knowledge stores)
- Productivity copilots and agents (stateful orchestration plus provider routing)
- Technical support assistants (domain-specific retrieval over product/code documentation)
- Multilingual business support experiences (provider/model flexibility for global workloads)

## Demo / What This Proves

This project demonstrates production-relevant backend engineering for agent systems:

- clean layered architecture in .NET (`Api`, `Application`, `Domain`, `Infrastructure`, `Contracts`)
- provider-agnostic model routing with per-turn overrides
- stateful conversation orchestration with memory and retrieval pipelines
- background work execution, maintenance endpoints, and operational runbook support
- streaming token delivery over SSE for real-time UX
- test-backed behavior across domain/application/infrastructure layers

As a portfolio artifact, it shows practical delivery capability beyond prototypes: system design, implementation discipline, documentation, and operational thinking.

## API Surface

Primary route groups for AI orchestration:

- `/api/chat` - streaming send, continue, regenerate, suggested user message
- `/api/conversations` - conversation CRUD and message mutation
- `/api/agents`, `/api/user-profiles`
- `/api/model-profiles`, `/api/generation-presets`, `/api/app-defaults`
- `/api/memory`, `/api/knowledge-bases`, `/api/inspection`
- `/api/tts`, `/api/images`
- `/api/import-export`
- `/api/admin`, `/api/admin/maintenance`, `/api/admin/background-work`

Endpoint catalog and examples: [docs/API_OVERVIEW.md](docs/API_OVERVIEW.md)

## Data and Generated Assets

- SQLite database: `src/LocalChat.Api/App_Data/localchat.db` (default)
- Request flow telemetry: `src/LocalChat.Api/App_Data/Logs/request-flow.ndjson` (when enabled)
- Generated speech files: `src/LocalChat.Api/wwwroot/generated/audio/...`
- Generated images: `src/LocalChat.Api/wwwroot/generated/images/...`
- Agent uploads: `src/LocalChat.Api/wwwroot/uploads/agents/...`

On startup the app:

1. ensures migration history compatibility for legacy databases
2. applies pending EF Core migrations
3. seeds default model profile, generation preset, and agent records

## Development Commands

Build:

```powershell
dotnet build LocalChat.sln
```

Test:

```powershell
dotnet test LocalChat.sln
```

## Security and Release Notes

- This backend currently exposes admin and maintenance endpoints without built-in auth.
- Treat deployments as trusted-network only until authn/authz is introduced.
- Never commit real API keys or tokens.

Release-oriented documentation:

- [docs/CONFIGURATION.md](docs/CONFIGURATION.md)
- [docs/API_OVERVIEW.md](docs/API_OVERVIEW.md)
- [docs/OPERATIONS.md](docs/OPERATIONS.md)
- [docs/RELEASE_CHECKLIST.md](docs/RELEASE_CHECKLIST.md)
- [SECURITY.md](SECURITY.md)
