# Testing, benchmarks, and roadmap

**Status:** proposed validation plan for implementation.

## Source and test location

All source code belongs under `src/`, including production projects, test projects, and any optional benchmark project. Keep test source under `src/tests/`; do not create a repository-root `tests/` folder. Keep the solution file under `src/` as well. Documentation remains under `docs/`, and Copilot configuration remains under `.github/`.

The initial solution at `src/EngineDemo.slnx` contains the Core library, UCI-host scaffold, and Core test project. Add later projects under `src/` as their features are implemented; see [architecture.md](architecture.md).

## Test layers

### Core unit tests

Cover square indexing and notation, attack masks, FEN parsing/formatting, move encoding, special moves, occupancy/hash updates, and exact make/unmake restoration. Include malformed FEN and invalid move inputs; errors must be explicit rather than silently converted to a start position or a null move.

### Move-generation tests

Use perft fixtures with exact counts and divide output. Cover checks, pins, castling rights and attacked transit squares, en-passant discovered checks, all promotion types, promotion captures, and stalemate/checkmate positions. Test both generated move legality and that every legal input move can be parsed and replayed.

### Search tests

Use small tactical positions with known legal best-move sets rather than assuming one exact move where several are equal. Test checkmate precedence when halfmove/repetition thresholds are also met, automatic 75-move/fivefold draws when implemented, stalemate versus draw-threshold classification, claimable game-history draws separately from search-cycle repetition, transposition-table score cutoffs across safe versus near-threshold halfmove clocks, and ensure a zero score alone is not treated as proof of a rule draw. Also test mate-distance preference, transposition-table bound behavior, cancellation, and fallback to the last completed iteration or legal root move. Make test positions and limits deterministic.

### UCI integration tests

Run the executable as a child process with redirected stdin/stdout/stderr. Exercise handshake, readiness during search, startpos/FEN plus move lists, `go depth`, `go infinite` + `stop`, options, new-game reset, malformed input, and clean quit. Assert `stop` emits one best-so-far move, while replacement commands and `quit` suppress the abandoned search's stale `bestmove`; stdout contains only valid protocol lines and each completed search emits no more than one `bestmove`.

### End-to-end compatibility checks

Once core behavior is stable, run representative games or a small match against a known UCI engine under controlled conditions. Record engine versions, options, openings/positions, time controls, hardware, and result. Treat rating estimates as noisy measurements, not correctness proof.

## Required correctness gates

Do not advance a milestone while its gate fails:

1. **Position model:** valid FEN round-trips; malformed FEN fails explicitly; make/unmake restores all state and hash.
2. **Rules:** initial position perft matches 20, 400, 8,902, and 197,281 through depth 4; Kiwipete matches 48, 2,039, and 97,862 through depth 3; targeted special-move cases pass.
3. **UCI:** handshake is exact; stdout is protocol-clean; ready/stop/quit remain responsive; a search emits at most one legal best move.
4. **Search:** terminal positions, score perspective, draw-context handling, cancellation, depth/node/time limits, and fallback behavior match the search contract; no illegal root move is returned.
5. **Release:** documented build/test commands work from a clean checkout, and no benchmark is presented as an Elo guarantee.

## Development roadmap

The first implementation sprint is decomposed into ordered, independently testable feature specs in the [Sprint 1 mission](../features/00-mission-01-sprint.md). Use that mission's dependency table and each feature's acceptance tests as the execution source of truth.

### Milestone 0 — project foundation (complete)

- Completed: created `src/EngineDemo.slnx` with the Core library, UCI-host scaffold, and Core test project; all application and test projects remain under `src/`.
- Acceptance verified: the solution builds cleanly and `dotnet test` completes, building Core.Tests; no tests are defined yet because this feature introduces no behavior. No chess behavior is claimed yet; behavior tests begin with the next feature.

### Milestone 1 — board and notation

- Implement typed squares, pieces, moves, position state, FEN, attacks, and make/unmake.
- Acceptance: round-trip and state-invariant tests pass.

### Milestone 2 — legal moves

- Implement pseudo-legal generation, king-safety filtering, special moves, and draw-history tracking.
- Acceptance: required perft suites and focused rule tests pass.

### Milestone 3 — UCI baseline

- Implement line-oriented command parsing, handshake, position replay, limits, and serialized output.
- Acceptance: protocol integration tests pass; no non-UCI stdout.

### Milestone 4 — usable search

- Add material evaluation, iterative deepening, negamax alpha-beta, quiescence, move ordering, transposition table, and cancellation/time management.
- Acceptance: deterministic search tests pass and finite/infinite searches stop correctly.

### Milestone 5 — strength and performance iteration

- Add optional evaluation/search improvements only with a baseline benchmark and a correctness-preserving test.
- Acceptance: report repeatable node/time or match results, including environment and configuration; retain the simpler implementation if no measurable benefit appears.

## Benchmark practice

Separate correctness tests from performance measurements. Use a fixed set of positions and limits; warm up the .NET runtime consistently; repeat runs and report median/variation. Record build configuration, runtime version, CPU, commit, hash size, limit type, node count, elapsed time, allocation count/bytes, and whether the transposition table was cleared. Measure steady-state search allocations separately from one-time initialization and result/reporting allocations. Compare equivalent work (same positions and search depth/nodes) as well as time-limited throughput; retain a simpler implementation unless a proposed optimization demonstrates a repeatable benefit without weakening correctness.

Never use an unrepeatable GUI impression as a performance claim. Never loosen perft or protocol assertions to make a benchmark pass.

## Developer workflow

The exact command names depend on the solution structure selected in Milestone 0. Keep the eventual README/index commands aligned with the actual `.slnx`/project names. At minimum, document:

- A clean solution build.
- The full test suite and a focused perft/UCI test command, with all test projects under `src/tests/`.
- Launching the executable and sending a short UCI transcript.
- Running the optional benchmark harness, when one exists.

## Exit criteria for a first complete engine

- Correct standard-chess legal move generation and draw handling covered by tests.
- Reliable UCI handshake, position setup, search, stop, and quit.
- Search returns legal moves, handles terminal positions, and respects requested depth/time constraints.
- Documentation and commands match the implemented project; all known limitations are stated.
