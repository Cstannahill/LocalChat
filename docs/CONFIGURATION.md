# Configuration Guide

LocalChat uses standard ASP.NET Core configuration binding.

Load order (typical):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. environment variables
4. command-line args

Use environment variables with `__` for nested keys, for example:

- `ConnectionStrings__DefaultConnection`
- `Ollama__BaseUrl`
- `OpenRouter__ApiKey`

## Required vs Optional

- Required for startup: `ConnectionStrings:DefaultConnection` (defaults to local SQLite file if omitted)
- Required for specific features:
  - chat provider integrations (`Ollama`/`OpenRouter`/`HuggingFace`/`LlamaCpp`)
  - TTS (`Speech`, `KokoroTts`, `QwenTts`)
  - images (`ComfyUi`)

If a feature is called without a reachable provider, API endpoints return errors (for example HTTP `503` for unreachable TTS provider).

## Core Sections

## `ConnectionStrings`

- `DefaultConnection`: SQLite connection string. Default data path is `App_Data/localchat.db` under API content root.

## `Ollama`

- `BaseUrl`
- `Model`
- `EmbeddingModel`
- `TimeoutSeconds`
- `KeepAlive`
- `ReservedOutputTokens`
- `SafetyMarginTokens`
- `MaxContextFallback`

## `OpenRouter`

- `BaseUrl`
- `ApiKey`
- `DefaultModel`
- `HttpReferer`
- `AppTitle`
- `EnableStreaming`
- `DefaultContextWindow`

## `HuggingFace`

- `BaseUrl`
- `ApiKey`
- `DefaultModel`
- `EnableStreaming`
- `DefaultContextWindow`

## `LlamaCpp`

- `BaseUrl`
- `ApiKey`
- `DefaultModel`
- `DefaultContextWindow`
- `UsePropsForContext`
- `ReservedOutputTokens`
- `SafetyMarginTokens`

## `Summaries`

- `Enabled`
- `KeepRecentMessageCount`
- `MinMessagesToSummarize`
- `MaxMessagesPerPass`
- `MaxSummaryCharacters`

## `Retrieval`

- `Enabled`
- ranking and threshold controls such as `CandidatePoolSize`, `MaxSelectedMemoryItems`, `SemanticWeight`, `LexicalWeight`, `MinFinalScore`, `RecencyHalfLifeDays`

## `MemoryProposals`

- confidence and extraction limits (`MinConfidenceScore`, `MaxCandidatesPerRun`, etc.)
- auto-accept and session-state TTL policy values

## `SessionStateCleanup`

- `Enabled`
- `PreservePinned`
- per-family max ages (`OutfitMaxAgeHours`, `LocationMaxAgeHours`, etc.)

## `Speech`

- `Provider`: `"Kokoro"` or `"Qwen"` (any non-`Kokoro` value currently resolves to Qwen provider)

## `KokoroTts`

- `BaseUrl`
- `ApiKey`
- `Model`
- `DefaultVoice`
- `DefaultSpeed`
- `ResponseFormat`
- `PublicBasePath`
- `TimeoutSeconds`

## `QwenTts`

- `BaseUrl`
- `ApiKey`
- `Model`
- `DefaultVoice`
- `DefaultSpeed`
- `ResponseFormat`
- `TimeoutSeconds`

## `ComfyUi`

- `BaseUrl`
- `WorkflowTemplatePath`
- `PollIntervalMs`
- `TimeoutSeconds`
- workflow node IDs (`PositivePromptNodeId`, `NegativePromptNodeId`, `LatentNodeId`, `SamplerNodeId`, `SaveImageNodeId`)
- `FilenamePrefix`

## `RetrievalAdmin`

- `BaseUrl`
- `EmbeddingModel`
- `BatchSize`
- `TimeoutSeconds`

## `ConversationBackgroundWork`

- `Enabled`
- polling/debounce parameters
- summary refresh thresholds

## `BackgroundMemoryProposals`

- `Enabled`
- scan cadence and conversation selection thresholds

## `Inspection`

- `EnableRetrievalTelemetry`
- `EnablePromptTelemetry`
- `EnableFlowTimingTelemetry`

## `RequestFlowLogging`

- `Enabled`
- `LogFilePath` (defaults to `App_Data/Logs/request-flow.ndjson`)

## Production Hardening Recommendations

1. Set secrets (`OpenRouter__ApiKey`, `HuggingFace__ApiKey`, TTS keys) outside source control.
2. Use absolute paths for critical logs if container/pod working directories vary.
3. Keep provider-specific timeouts explicit for your network.
4. Disable optional telemetry in high-throughput environments unless needed.
5. Restrict network access to this API if running without auth middleware.
