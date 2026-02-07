## Repo Purpose
- Build the Alex Kidd-inspired Rock–Paper–Scissors minigame described in [Plan.md](Plan.md), targeting a playful yet stats-driven experience.
- Treat the repo as the learning path deliverable from Microsoft Learn; solution code lives on the `Solution` branch per [README.md](README.md#L1-L3).

## Current State
- Main branch currently hosts only planning docs; no .NET projects exist yet, so agents should scaffold both backend and Blazor frontend from scratch.
- Keep documentation-first changes atomic so reviewers can compare against the empty baseline more easily.

## Architecture North Star
- Backend: .NET 10 Web API that exposes gameplay endpoints, keeps deterministic RPS logic, and stores prototype state in-memory.
- Frontend: Blazor (Server or WASM) consuming the API, handling UI state locally while mirroring authoritative state returned by the backend.

## Gameplay Mechanics
- Player starts with 50 coins, places a bet each round, and outcomes adjust the coin balance; enforce optional min/max bet rules when introduced.
- Track wins, losses, ties, and coin deltas per opponent; surfaced stats fuel both UI feedback and AI behaviour tweaks.

## Opponents & AI Profiles
- Marty is mandatory; design additional personalities (random, weighted, patterned, “cheater”) as strategy classes so they can be swapped via DI.
- AI logic must expose metadata (name, behaviour type, difficulty) for the `/opponents` API and Blazor selector.

## API Contract
- `POST /play` accepts player move + bet + opponent id, executes resolution, and returns round result + updated player state.
- `GET /opponents`, `GET /stats`, and `POST /reset` round out the minimum surface; keep payload contracts centralized for reuse between API + UI models.

## Data & State Models
- `PlayerState`: coins, streaks, aggregate W/L/T, and per-opponent stats; consider a dictionary keyed by opponent id for extensibility.
- `RoundResult`: player move, opponent move, winner enum, coin delta, timestamp to enable history views in the UI.

## Frontend Expectations
- Components needed: opponent picker, move selector, bet form, results panel with retro effects, stats dashboard; keep each component lean and reusable.
- Prefer partial classes or view models for complex logic so markup files stay declarative; use local storage only if session persistence becomes a requirement.

## Dev Workflow
- Scaffold backend via `dotnet new webapi -n Game.Api` and Blazor via `dotnet new blazorserver -n Game.Client` (adjust names to match repo); keep them under a `src/` folder.
- During development run `dotnet watch` for each project or configure a solution file for multi-project debugging; document any deviations inside [Plan.md](Plan.md).

## Testing & Quality
- Prioritize unit tests for the RPS resolver, bet validation, AI strategy selection, and coin economy edge cases before adding UI tests.
- Keep tests colocated (e.g., `tests/Game.Domain.Tests`) and target deterministic seeds for AI personalities so snapshots remain stable.

## Documentation & Communication
- Use `Plan.md` as the single source of truth for implementation decisions, task tracking, and any deviations from the original plan.
- Update the README with developer experience notes, tooling requirements, and local development instructions to onboard new contributors smoothly.
- Prepare release notes or a summary in the `Solution` branch to highlight the final implementation against the empty baseline for reviewers.
- Update the documentation after each step to ensure we have a clear record of where we are up to. I would like for tasks to be documented and tracked so that we can easily see what has been completed and what is still outstanding. Also keep small incremental changes to the codebase so that we can easily review and understand the changes being made.