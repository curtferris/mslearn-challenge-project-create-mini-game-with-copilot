# Rock–Paper–Scissors: Alex Kidd-Inspired Edition

Planning workspace for the Microsoft Learn challenge project. The `main` branch now hosts the shared solution plus documentation-first scaffolding for the upcoming .NET implementation.

## Repository Layout

| Path | Purpose |
| --- | --- |
| `Plan.md` | Single source of truth for gameplay brief, implementation decisions, phases, and tasks. |
| `src/Game.Api` | ASP.NET Core Web API (net10.0) that will host gameplay endpoints. |
| `src/Game.Client` | Blazor WebAssembly client (net10.0) that consumes the API. |
| `src/Game.Shared` | Class library for DTOs and shared domain types referenced by API + client. |
| `Game.slnx` | Solution tying the three projects together. |

## Tooling Requirements

- .NET 10.0.102 SDK (pinned via `global.json`).
- VS Code + C# Dev Kit or Visual Studio 2022 Preview for best experience.
- Node.js is **not** required; the Blazor client runs via `dotnet` tooling.

## Local Development

From the repo root you can spin up both apps with a single command (logs are prefixed per service):

```bash
pwsh scripts/run-dev.ps1
```

If you prefer manual control, run the API and client separately:

```bash
# Terminal 1 – API
dotnet watch --project src/Game.Api/Game.Api.csproj

# Terminal 2 – Blazor WASM client
dotnet watch --project src/Game.Client/Game.Client.csproj
```

VS Code users can also pick the "Launch API + Client" compound configuration provided in `.vscode/launch.json` to debug both processes at once.

Both apps target http://localhost-based ports selected by the ASP.NET dev cert tooling. Update `appsettings.Development.json` / client `appsettings` once we wire the actual endpoints.

### API Surface (current)

| Method | Route   | Notes |
| --- | --- | --- |
| GET  | `/opponents` | Lists available opponent metadata (id, name, behavior, difficulty). |
| GET  | `/opponents/{id}` | Returns metadata for a single opponent id. |
| POST | `/play`  | Accepts `PlayRequest`; returns `PlayResponse` with round summary + updated `PlayerState`. |
| GET  | `/stats` | Returns `PlayerStatsResponse` (aggregates + recent history). |
| POST | `/reset` | Resets coins/stats; returns fresh `PlayerState`. |

## Continuous Integration

- GitHub Actions workflow (coming soon) will run `dotnet restore`, `dotnet build`, and `dotnet test` on Ubuntu + Windows runners.
- NuGet caching will key off `global.json` plus project files for faster iterations.
- Publishing/deployment steps stay disabled until we choose a hosting target.

For the finished learning-path solution, switch to the `Solution` branch once development completes.