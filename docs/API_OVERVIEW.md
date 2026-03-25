# API Overview

This API is implemented with ASP.NET Core Minimal APIs.

Base URL (default dev): `http://localhost:5170`

The route surface is currently named around chat/domain semantics, but it is usable as a general AI orchestration API (session management, profile management, memory/retrieval, media generation, and admin workflows).

Global utility endpoints:

- `GET /health`
- `GET /swagger` (Development only)

## Route Groups

## Interaction and Session Orchestration

- `/api/chat`
  - `POST /send/stream` (SSE token stream)
  - `POST /continue/stream`
  - `POST /regenerate`
  - `POST /suggest-user-message`
- `/api/conversations`
  - create/list/get/update message content
  - delete/branch/select message variants
  - update persona and runtime settings on conversation

## Assistant/Agent and User Profile Management (Current Route Names)

- `/api/characters`
  - list/detail/create/update/delete
  - upload/delete character image
- `/api/personas`
  - list/detail/create/update/delete/default persona handling

## Model and Runtime Defaults

- `/api/model-profiles`
- `/api/generation-presets`
- `/api/app-defaults`

These define provider+model routing and generation defaults at app/profile/session scopes.

## Memory, Retrieval, and Inspection

- `/api/memory`
  - memory CRUD/review, conflict workflows, merge/promote/demote, import/export
- `/api/lorebooks`
- `/api/inspection`
  - prompt and retrieval inspection
- `/api/inspection/scene-state`
- `/api/inspection/memory-extraction`

## Media

- `/api/tts`
  - synthesize speech for message content
  - list speech clips by message
- `/api/images`
  - generate contextual prompts
  - trigger image generation jobs
  - list generated image jobs by conversation

## Admin and Maintenance

- `/api/admin`
  - retrieval stats/reindex
  - background memory proposal status/trigger
- `/api/admin/maintenance`
  - memory key rebuild
  - full retrieval reindex
  - prune/export extraction audits
  - prune stale scene-state memory
- `/api/admin/background-work`
  - status and manual trigger of conversation background work tasks

## Import/Export

- `/api/import-export`
  - character import/export
  - persona import/export

## Terminology Mapping

- `character` corresponds to an assistant/agent profile
- `persona` corresponds to an end-user profile
- `conversation` corresponds to an interaction session

## Streaming Chat Example

```bash
curl -N -X POST "http://localhost:5170/api/chat/send/stream" \
  -H "Content-Type: application/json" \
  -d '{
    "characterId":"00000000-0000-0000-0000-000000000000",
    "conversationId":null,
    "userPersonaId":null,
    "message":"Hello there"
  }'
```

The response is `text/event-stream` with events including stream start, token deltas, completion, and error.

## One-Turn Provider/Model Override

`/api/chat/send/stream` and `/api/chat/regenerate` accept optional query parameters:

- `overrideProvider` (for example `Ollama`, `OpenRouter`, `HuggingFace`, `LlamaCpp`)
- `overrideModelIdentifier`

Use these for one-turn routing overrides without changing stored defaults.

## TTS Example

```bash
curl -X POST "http://localhost:5170/api/tts/messages/{messageId}/synthesize" \
  -H "Content-Type: application/json" \
  -d '{
    "voice":"Camila",
    "modelIdentifier":"qwen3-tts-1.7b",
    "speed":1.0
  }'
```

## Notes

- Endpoint auth is not enabled by default. Deploy behind network controls.
- Use Swagger in Development for contract-level payload details.
