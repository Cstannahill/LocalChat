# Operations Guide

This document focuses on running and operating the LocalChat AI orchestration backend in non-trivial environments.

## Runtime Behavior on Startup

On application startup, `Program.cs` performs:

1. SQLite DB initialization and migration history compatibility check
2. `Database.MigrateAsync()` to apply pending EF Core migrations
3. default seeding for model profiles, generation presets, and assistant profiles (stored as agents in the current schema)

## Run Modes

Development:

```powershell
dotnet run --project src/LocalChat.Api/LocalChat.Api.csproj
```

Production-like:

```powershell
dotnet publish src/LocalChat.Api/LocalChat.Api.csproj -c Release -o publish
./publish/LocalChat.Api
```

## Data and File Locations

Relative to API content root (`src/LocalChat.Api` when run via `dotnet run --project`):

- DB: `App_Data/localchat.db`
- request flow log: `App_Data/Logs/request-flow.ndjson`
- generated audio: `wwwroot/generated/audio/YYYY/MM/...`
- generated images: `wwwroot/generated/images/YYYY/MM/...`
- uploaded agent images: `wwwroot/uploads/agents/<agentId>/...`

## Backups

Minimum backup set:

1. `App_Data/localchat.db`
2. generated assets in `wwwroot/generated/`
3. uploaded agent images in `wwwroot/uploads/agents/`

If you can only back up one item, prioritize the DB.

## Background Work

Two background systems are wired:

- conversation background work queue (`ConversationBackgroundWork` settings)
- background memory proposal worker (`BackgroundMemoryProposals` settings)

Operational endpoints:

- `/api/admin/background-work/status`
- `/api/admin/background-work/conversations/{conversationId}/refresh-summary`
- `/api/admin/background-work/conversations/{conversationId}/extract-memory`
- `/api/admin/background-work/conversations/{conversationId}/reindex-retrieval`
- `/api/admin/memory-proposals/background/status`
- `/api/admin/memory-proposals/background/run/{conversationId}`

## Maintenance Endpoints

Use carefully in controlled operations windows:

- `/api/admin/maintenance/memory/rebuild-keys`
- `/api/admin/maintenance/retrieval/reindex-all`
- `/api/admin/maintenance/memory/prune-stale-session-state`
- `/api/admin/maintenance/audit/memory-extraction/prune?olderThanDays=30`
- `/api/admin/maintenance/audit/memory-extraction/export/conversations/{conversationId}`

## Logging and Telemetry

- Standard ASP.NET logging via configured log providers
- Request flow middleware logs API request timing and writes NDJSON file when enabled
- Request-flow logs are append-only; ensure filesystem has enough space and retention policy

## Common Failure Cases

1. Provider unavailable (LLM/TTS/ComfyUI unreachable)
   - verify configured base URLs and network
   - verify required API keys for remote providers
2. Slow/failed generation due to provider timeout
   - increase provider `TimeoutSeconds`
3. Missing generated media URLs
   - verify static file hosting and `wwwroot` write permissions

## Recommended Production Controls

1. Place API behind auth-aware gateway/reverse proxy.
2. Restrict admin/maintenance endpoints by network policy.
3. Store secrets in environment/secret manager only.
4. Add log retention/rotation for `App_Data/Logs`.
5. Schedule database backups and restore drills.

