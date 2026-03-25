# Contributing

Thanks for contributing to LocalChat.

## Development Setup

1. Install .NET 9 SDK.
2. Configure local provider settings in `src/LocalChat.Api/appsettings.Development.json`.
3. Run:

```powershell
dotnet restore LocalChat.sln
dotnet build LocalChat.sln
dotnet test LocalChat.sln
```

## Branch and PR Expectations

- Keep changes focused and reviewable.
- Include tests for behavior changes.
- Update docs when adding or changing endpoints/configuration.
- Avoid unrelated refactors in feature PRs.

## Commit Guidelines

Preferred commit slices:

- `security/config`
- `ci`
- `docs`
- `polish`

Example message style:

- `docs: add quickstart provider key guidance`
- `ci: add build and test workflow`

## Security

- Never commit real secrets.
- Use `appsettings.Development.json` (gitignored) or environment variables for provider keys.
- If you suspect a leak in tracked files, open a security issue privately per `SECURITY.md`.
