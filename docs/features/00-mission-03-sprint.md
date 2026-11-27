# Mission: Sprint 3 — UCI baseline and usable search

**Status:** Planned

**Sprint:** 3

**Design references:** [Architecture](../road-map/architecture.md), [UCI protocol](../road-map/uci-protocol.md), [Search and evaluation](../road-map/search-and-evaluation.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Deliver the first usable single-threaded chess engine. At the end of this sprint, the UCI host accepts positions and commands reliably, runs deterministic legal search, and supports depth, movetime, and infinite searches with responsive cancellation.

## Subtasks

- [ ] Implement line-oriented UCI input, whitespace tokenization, variable-length position/go arguments, and atomic output writes.
- [ ] Implement `uci`, identity output, `isready`, and supported-option declarations without non-protocol stdout.
- [ ] Implement `ucinewgame`, `position startpos`, and six-field FEN position setup with legal move replay and transactional replacement.
- [ ] Implement `setoption` syntax validation and stderr diagnostics for unknown or unsupported options.
- [ ] Implement clean `quit` handling and a child-process integration-test harness with redirected standard streams.
- [ ] Define immutable `SearchLimits` and `SearchResult`; keep Core search independent of console and UCI formatting.
- [ ] Add side-to-move-relative material evaluation, a legal root fallback, and deterministic one-ply root search.
- [ ] Implement iterative-deepening negamax alpha-beta, with bounded reusable per-ply storage and surfaced maximum-ply exhaustion.
- [ ] Support `go depth`, `go movetime`, and `go infinite` using monotonic time, bounded cancellation checks, and last-completed-iteration fallback.
- [ ] Integrate a single active search with responsive `isready`, `stop`, replacement-command cancellation, and exactly one terminal `bestmove`.

## Out of scope

- Node/clock limits, `searchmoves`, ponder, transposition tables, quiescence, and advanced move ordering.
- Multi-threading, GUI, Chess960, and claims about playing strength.

## Acceptance criteria

- Malformed FEN or illegal moves are diagnosed on stderr and never silently replaced by the initial position or partially applied.
- Stdout contains only complete UCI protocol lines.
- Search results are deterministic for fixed position, limits, and table state; every returned root move is legal.
- `stop` is idempotent and emits one best-so-far move; replacement commands and `quit` suppress stale best moves.
- Root terminal positions emit `bestmove 0000`; a single legal root move is still searched through its continuation.
- Tests cover terminal precedence, fallback, cancellation, depth/time limits, readiness during search, and repeated search/stop/quit cycles.
- The solution builds and the full test suite passes.
