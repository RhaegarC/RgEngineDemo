# Mission: Sprint 1 — Core foundation and position notation

**Status:** In progress

**Sprint:** 1

**Design references:** [Architecture](../road-map/architecture.md), [Chess rules and move generation](../road-map/chess-rules-and-move-generation.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Establish the .NET solution and Core test harness, then implement the smallest reliable chess-position foundation: typed squares and pieces, bitboard operations, validated position state, the standard initial position, and strict FEN parsing/serialization.

At the end of this sprint, callers can construct and inspect a standard-chess position from the start position or FEN, and round-trip it deterministically. This is a data-model milestone, not a playable engine.

## Repository constraints

- All application, test, and optional benchmark source code and project files stay under `src/`.
- The solution lives at `src/EngineDemo.slnx`; Core tests live under `src/tests/`.
- Use `EngineDemo` as the explicit C# root namespace. Keep `RgChess` for product and project naming.
- Keep design and feature documents in `docs/road-map/` and `docs/features/`.
- Do not add root-level `tests/`, `benchmarks/`, or source-code directories.

## Scope

- Create the Core, UCI host, and Core test project structure.
- Define square indexing and coordinate notation.
- Define color/piece types and their FEN-symbol mapping.
- Provide tested 64-bit bitboard primitives.
- Represent validated positions with piece bitboards, derived occupancy, side to move, castling rights, en-passant target, and move counters.
- Provide a standard initial-position factory and deterministic position hash.
- Parse and serialize all six standard FEN fields with explicit invalid-input errors.

## Out of scope

- Attack generation, legal or pseudo-legal move generation, move application/undo, perft, repetition history, and draw adjudication.
- UCI command handling or search/evaluation.
- GUI, chess variants, opening book, tablebases, and performance tuning.

## Feature sequence

Implement in numeric order. Each feature is independently testable and should be completed before its dependents.

| Feature | Scope | Depends on | Status |
|---|---|---|---|
| [01 — Solution and Core test foundation](archive/01-solution-and-core-test-foundation.md) | Create the solution and the Core, UCI-host, and Core-test projects under `src/`. | None | Archived |
| [02 — Square and coordinate notation](archive/02-square-and-coordinate-notation.md) | Add the 64-square type, indexing contract, and coordinate conversion. | 01 | Archived |
| [03 — Piece and color model](archive/03-piece-and-color-model.md) | Add typed colors/pieces and a reversible FEN-symbol mapping. | 01 | Archived |
| [04 — Bitboard operations](archive/04-bitboard-operations.md) | Add safe, tested bitboard membership and iteration helpers. | 01, 02 | Archived |
| [05 — Position state and validation](archive/05-position-state-and-validation.md) | Store position metadata and piece masks; validate invariants and derive occupancy. | 02, 03, 04 | Archived |
| [06 — Position hash](06-position-hash.md) | Add a deterministic Zobrist key for a complete position. | 05 | Planned |
| [07 — Standard initial position](07-standard-initial-position.md) | Construct and verify the standard start position. | 05, 06 | Planned |
| [08 — FEN parsing](08-fen-parsing.md) | Parse and validate all six FEN fields into a Position. | 02, 03, 05, 06 | Planned |
| [09 — FEN serialization](09-fen-serialization.md) | Serialize valid positions deterministically and prove round trips. | 07, 08 | Planned |

## Definition of done

- Every feature's acceptance criteria are met and its focused tests pass.
- `dotnet build src/EngineDemo.slnx` and `dotnet test src/EngineDemo.slnx` succeed.
- Tests cover malformed and boundary inputs as well as valid positions; no test substitutes a fake for behavior it claims to prove.
- A standard start position and valid FEN serialize deterministically; parse/serialize/parse preserves all position fields and the position key.
- No production or test source/project file has been added outside `src/`.
- Documentation accurately states that legal moves, UCI behavior, and search are not yet implemented.

## Progress record

Update the status of each feature file and this mission as work is completed. A feature is done only when its tests pass and its acceptance criteria are verified; merging or archiving is tracked separately.
