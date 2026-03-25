# Release Checklist

Use this checklist before shipping a backend release.

## Security

- [ ] Remove/rotate any real API keys from configuration files.
- [ ] Confirm secrets are injected through environment variables or secret manager.
- [ ] Ensure deployment is network-restricted if API auth is not yet enabled.
- [ ] Review admin and maintenance endpoints exposure.

## Build and Tests

- [ ] `dotnet build LocalChat.sln` succeeds.
- [ ] `dotnet test LocalChat.sln` succeeds.
- [ ] Validate migrations apply cleanly to a fresh SQLite DB.

## Configuration

- [ ] Validate provider URLs and timeouts (`Ollama`, `OpenRouter`, `HuggingFace`, `LlamaCpp`, TTS, `ComfyUi`).
- [ ] Verify `ConnectionStrings:DefaultConnection` for target environment.
- [ ] Confirm background worker toggles (`ConversationBackgroundWork`, `BackgroundMemoryProposals`).
- [ ] Confirm telemetry/log settings (`RequestFlowLogging`, `Inspection`).

## Operational Readiness

- [ ] Backup strategy documented and tested for DB and generated assets.
- [ ] Log retention strategy in place.
- [ ] Health endpoint (`/health`) wired into monitoring.
- [ ] Runbook includes maintenance endpoint usage and rollback plan.

## Documentation

- [ ] `README.md` reflects current setup, feature set, and general AI orchestration positioning.
- [ ] `docs/CONFIGURATION.md` updated for any new options.
- [ ] `docs/API_OVERVIEW.md` updated for endpoint changes and terminology mapping clarity.
- [ ] `docs/OPERATIONS.md` updated for new jobs or maintenance tasks.
- [ ] `SECURITY.md` contact/process current.

