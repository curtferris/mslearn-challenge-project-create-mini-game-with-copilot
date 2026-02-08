# Rock–Paper–Scissors: Alex Kidd-Inspired Edition

Modern take on Alex Kidd’s Janken battles, focused on personality-driven opponents, a coin economy, and rich stats to guide strategy.

## Implementation Decisions

- Shared contracts live in a dedicated `src/Game.Shared` class library referenced by both API and Blazor projects from day one.
- Frontend targets **Blazor WebAssembly** (hosted API + WASM client) to keep the UI fully decoupled from server rendering.
- MVP mechanics stick to the base 50-coin loop; min/max bet enforcement, streak bonuses, and other modifiers are deferred to Phase 4.
- Testing stack is xUnit; AI personas use a seeded RNG (random number generator service registered via DI) so outcomes stay deterministic in tests.
- Persistence remains in-memory for this demo; abstraction layers for databases are optional unless priorities change.
- All projects target .NET 10 Preview; check in a `global.json` so local devs and CI agents use the same SDK.

## Tooling, CI, and Packages

### Runtime & SDK
- Install the latest .NET 10 Preview SDK locally and on CI runners so `Game.Api`, `Game.Client`, and `Game.Shared` stay in lockstep.
- Add `global.json` to pin the SDK version and avoid mismatched CLI behavior.

### GitHub Actions
- Base workflow: `dotnet restore`, `dotnet build`, and `dotnet test` on `ubuntu-latest` + `windows-latest` runners using the shared solution.
- Cache `~/.nuget/packages` via `actions/cache` keyed on `global.json` + project files for faster CI iterations.
- Keep a placeholder `dotnet publish` step commented/unconfigured until a hosting target (Static Web Apps, App Service, etc.) is chosen.

### NuGet Recommendations
- `xunit`, `xunit.runner.visualstudio` for unit testing infrastructure.
- `FluentAssertions` to keep domain assertions readable.
- `coverlet.collector` for coverage reporting during `dotnet test`.
- `Swashbuckle.AspNetCore` to expose OpenAPI docs for gameplay endpoints.
- Optional UI helpers: `Blazored.Toast` or `Blazorise` when we tackle retro feedback/polish.

## Delivery Phases & Tasks

### Phase 0 – Repo Scaffolding & Shared Contracts
- [x] Create `src/` structure with `dotnet new webapi -n Game.Api`, `dotnet new blazorwasm -n Game.Client`, and `dotnet new classlib -n Game.Shared`, plus a solution file tying all projects together.
- [x] Add `.editorconfig`, launch profiles, and developer experience notes to README so new contributors can run both apps via `dotnet watch`.
- [x] Introduce shared DTOs (`PlayerState`, `RoundResult`, `OpponentProfile`, `OpponentStats`) inside `Game.Shared` referenced by both API and Blazor client.
- [ ] Capture any deviations from this plan directly in [Plan.md](Plan.md) so the document remains the single source of truth.

### Phase 1 – Backend Core Gameplay Loop
- [x] Define enums/models for moves, winner states, bets, and round payloads inside `Game.Api` (or shared library).
- [x] Implement deterministic resolver and bet validator enforcing starting 50 coins and balance checks (min/max rules deferred to Phase 4).
- [x] Build an in-memory `PlayerStateService` that tracks aggregate stats, streaks, and per-opponent dictionaries with reset capability.
- [x] Expose `POST /play`, `POST /reset`, and `GET /stats` endpoints; serialize shared contracts for client reuse.

### Phase 2 – AI Roster & Metadata
- [x] Create `IOpponentStrategy` plus baseline Marty strategy; add additional behaviors (random, weighted, patterned, “cheater”) registered via DI.
- [x] Implement `/opponents` (and optional `/opponents/{id}`) endpoints returning metadata (name, behavior type, difficulty) for all strategies.
- [x] Extend per-opponent stats (record, net coins, last played) and ensure state updates per round feed both `/stats` and `/opponents` responses.
- [x] Add xUnit tests (e.g., `tests/Game.Domain.Tests`) validating resolver outcomes, bet validation edge cases, and deterministic AI behaviors using seeded RNG.

### Phase 3 – Blazor UI & Interaction Flow
- [x] Scaffold shared HTTP client/service layer for calling `/opponents`, `/play`, `/stats`, `/reset`, reusing shared DTOs.
- [x] Build components: opponent picker, move selector, bet form (with validation + quick chips), results panel with retro effects, stats dashboard.
- [x] Wire interaction flow (select opponent → place bet → pick move → show result) with loading feedback and error states; sync UI state to API responses.
- [x] Optionally add local-storage hook for session persistence if the gameplay loop benefits from resume support.

> Update: Added the gameplay hub (`Pages/Index.razor`) wired to `GameApiClient`, plus a bespoke retro-inspired theme and a local-storage backed session cache so the UI components share a cohesive layout and survive refreshes.

### Phase 4 – Polish, QA, and Enhancements
 - [x] Fold in more Alex Kidd-inspired art/graphics from Miracle World (custom recreations for this learning task) so the experience feels anchored in that universe.
	- [x] Add a mini “Janken countdown” animation (tap rock three times before revealing the move) so rounds mimic the original reveal cadence. The reveal now displays the round outcome for a full four seconds so players can digest the moves and winner before the overlay closes.
 - [x] Add images for all of the opponents so that we can visualise then. 
- [x] Inject a short background story that elevates Marty into an evil overlord the player must overthrow, setting narrative stakes for beating every opponent.

> Update: Added a CRT scanline overlay, glitchy sweeps, and a streak-aware taunt marquee fed by live player stats to kick off Phase 4 polish.
> Update: Added the gameplay hub (`Pages/Index.razor`) wired to `GameApiClient`, plus a bespoke retro-inspired theme, local-storage backed session cache, a lore panel with Marty’s overlord storyline, and pruned the Blazor template scaffold so only the Alex Kidd experience remains.
> Update: Implemented the Miracle World-style Janken countdown overlay that taps thrice, then flashes the round result (moves, winner, and coin delta) before returning to the arena.
> Update: Extended the reveal phase to linger for four seconds so the player can read both moves, the winner, and the coin delta before the overlay fades out.
> Update: Swapped the textual move labels in the reveal with rock/paper/scissors glyphs so the overlay instantly communicates what each side picked.
> Update: Added illustrated SVG portraits for Marty, Lucky Lena, Rhythm Rex, and Sneaky Sid plus UI support so the opponent picker cards showcase each challenger visually.
> Update: Refined those portraits into full cartoon villains (including a Marty rendition based on the provided photo reference) to better match the Miracle World vibe.
> Update: Dropped a new MiracleBackdrop component into the hero panel with layered suns, mountains, palace silhouettes, palm islands, and animated waves so the UI immediately evokes Miracle World.

## Reference Briefs

### 🎮 Gameplay Design Brief

#### Core Concept
- Classic Rock–Paper–Scissors with stylized opponents and escalating stakes.
- Player move vs. AI move resolves via standard rules.

#### Gameplay Features
- Three-move selection (Rock, Paper, Scissors) surfaced every round.
- Bet submission per round; outcome adjusts coin balance immediately.
- Results highlight win/loss/tie plus coin delta to reinforce stakes.

#### AI Opponents
- Marty is mandatory and establishes baseline behavior.
- Additional personalities should exist as discrete strategy classes (random, weighted, patterned, “cheater”).
- Each opponent publishes metadata: display name, behavior type, difficulty badge.

#### Coin Economy
- Player begins at 50 coins; zero coins ends the session until reset.
- Bets required for each round; wins pay out, losses deduct, ties return stake.
- Optional rules: enforce min/max bets and offer streak bonuses for consecutive wins.

#### Scoring & Stats
- Maintain aggregate wins, losses, ties, and total coin delta.
- Track per-opponent history (record, net coins, last faced timestamp) to drive UI insights.
- Enable streak tracking for both wins and losses to inform AI taunts or UI flair.

#### Optional Enhancements
- Retro-inspired animations, audio cues, or CRT shaders during reveal.
- Unlockable opponents, themes, or wagers as coin milestones are hit.
- Lightweight session persistence (browser storage or serialized state) for longer play.

### 🧩 Technical Design Brief

#### Tech Stack
- Backend: .NET 10 Web API hosting gameplay logic and authoritative state.
- Frontend: Blazor (Server or WASM) consuming API responses and rendering UI state locally.
- Language: C# end-to-end; prototype data kept in-memory with optional SQLite/LiteDB later.

#### API Surface

| Method | Route            | Payload Highlights                     | Purpose                                  |
|--------|------------------|----------------------------------------|------------------------------------------|
| POST   | /play            | player move, bet amount, opponent id   | Resolve round, return `RoundResult` + `PlayerState`. |
| GET    | /opponents       | —                                      | List opponent metadata for selectors.    |
| GET    | /stats           | —                                      | Fetch aggregate and per-opponent stats.  |
| POST   | /reset           | optional opponent selection or seed    | Reset coins, streaks, and history.       |
| GET    | /opponents/{id}* | —                                      | (*Optional) Detailed dossier per AI.     |

#### Game Engine Logic
- Deterministic resolver translates move pair into `Winner` enum and coin delta.
- Bet validator enforces balance, min/max rules, and rejects invalid stakes.
- AI strategy interface (e.g., `IOpponentStrategy`) drives move generation.
- State service manages `PlayerState`, per-opponent metrics, and resets.

#### Shared Models
- `PlayerState`: `Coins`, `Wins`, `Losses`, `Ties`, `CurrentStreak`, `BestStreak`, `PerOpponentStats` dictionary.
- `RoundResult`: `PlayerMove`, `OpponentMove`, `Winner`, `CoinDelta`, `Timestamp`.
- `OpponentProfile`: `Id`, `Name`, `BehaviorType`, `Difficulty`, optional flavor text.
- `OpponentStats`: `Wins`, `Losses`, `Ties`, `NetCoins`, `LastPlayed`.

### 🖥️ Frontend (Blazor) Requirements

#### UI Components
- Opponent picker with metadata cards and difficulty indicators.
- Move selector with retro-styled buttons plus quick shortcuts.
- Bet form supporting validation, quick chips, and current coin preview.
- Results panel showing last round outcome, coin delta, and animations.
- Stats dashboard summarizing overall record, streaks, and per-opponent performance.

#### State Management
- Keep UI state in Blazor component state or dedicated view models; synchronize with API responses to avoid drift.
- Optional local storage hook to rehydrate player state between sessions if needed.

#### Interaction Flow
1. Player chooses opponent and bet size.
2. Player selects move; UI posts to `/play` and displays loading feedback.
3. Response drives results animation, updates stats, and re-renders coin balance.
4. Provide reset button wired to `/reset` plus confirmation modal.

### ⚙️ Non-Functional Requirements
- Favor clean, modular architecture so AI agents can extend strategies easily.
- Unit tests target resolver logic, bet validation, AI personality selection, and state transitions before UI testing.
- Document deviations from this plan inside README or commit history for reviewers.