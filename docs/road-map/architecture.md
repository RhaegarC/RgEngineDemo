# Architecture

**Status:** proposed design for future implementation.

## Goals and boundaries

Build a correct, maintainable standard-chess engine in C#/.NET 10. The executable is a UCI-speaking console process. Keep chess rules and search independent of console I/O so that they can be tested without launching the protocol loop.

The initial design favors explicit state and well-tested algorithms over premature micro-optimization. Start single-threaded. Do not include a GUI, network service, variants, opening book, tablebases, or external runtime packages in the first implementation.

## Proposed solution layout

The current solution provides the Core library, UCI-host scaffold, and Core test project from this proposed structure; UCI tests and benchmarks remain future additions. Keep **all source-bearing projects and their source files under `src/`**, including production code, tests, and optional benchmarks. The solution file also lives in `src/`. Do not create root-level `tests/`, `benchmarks/`, or other source-code directories. Documentation, repository configuration, and non-code assets may remain in their appropriate top-level folders.

```text
src/
  EngineDemo.slnx
  EngineDemo.Core/
    Board/
    Notation/
    Search/
  EngineDemo.Uci/
  tests/
    EngineDemo.Core.Tests/
    EngineDemo.Uci.Tests/
  benchmarks/
    EngineDemo.Benchmarks/    # optional; add when there are stable hot paths to measure
docs/
  road-map/
.github/
```

`EngineDemo.Core` is a class library containing the chess model, legal move generation, position parsing, search, and evaluation. `EngineDemo.Uci` is the console executable and depends on Core. Core must not depend on UCI or write to `Console`. Test projects live under `src/tests/` and remain separate from production assemblies. Add benchmark code under `src/benchmarks/` only when repeatable performance measurements are useful; correctness tests must not depend on it.

`src/EngineDemo.slnx` contains `EngineDemo.Core`, `EngineDemo.Uci`, and `EngineDemo.Core.Tests`. The UCI project is only a host scaffold; later features add chess behavior and UCI commands. Add further test or benchmark projects under `src/` only when their features require them.

## C# namespace conventions

Use `EngineDemo` as the C# root namespace and as the prefix for project and assembly names. Keep `RgChess` for the product name and UCI identity. Set `<RootNamespace>EngineDemo</RootNamespace>` explicitly in each project file rather than relying on a project name to determine the namespace.

For example, code in the `EngineDemo.Core` project should declare `namespace EngineDemo.Core;` (or a sub-namespace such as `EngineDemo.Core.Board`).

Use descriptive sub-namespaces for project boundaries and modules:

| Project | Namespace examples |
|---|---|
| `EngineDemo.Core` | `EngineDemo.Core`, `EngineDemo.Core.Board`, `EngineDemo.Core.Notation`, `EngineDemo.Core.Search` |
| `EngineDemo.Uci` | `EngineDemo.Uci` |
| `EngineDemo.Core.Tests` | `EngineDemo.Core.Tests` |
| `EngineDemo.Uci.Tests` | `EngineDemo.Uci.Tests` |
| `EngineDemo.Benchmarks` | `EngineDemo.Benchmarks` |

Keep the product name **RgChess** in user-facing identity, product documentation, and UCI identification where appropriate.

Within Core, organize by domain responsibility rather than creating a project for every subsystem:

| Area | Responsibility |
|---|---|
| `Board` | Piece/square/move types, position state, attacks, move generation, make/unmake, draw state |
| `Notation` | FEN parsing/serialization and UCI move parsing/formatting |
| `Search` | Search limits/results, iterative deepening, alpha-beta, move ordering, transposition table, evaluation, time management |
| `Uci` | Input loop, command parsing/dispatch, options, output formatting, and process lifetime |

Split these areas further only when implementation or test boundaries make the benefit clear.

## Core model

Use `ulong` bitboards, where bit `0` is `a1`, files advance within each rank, and ranks advance from rank 1 to rank 8. Document and test this mapping once; all attack tables, parsing, and notation must use it consistently.

The `Position` is the authoritative mutable search state. It should contain:

- Twelve piece bitboards (one per color and piece kind), plus cached white, black, and total occupancy bitboards.
- Side to move.
- Castling rights for each side.
- En-passant target square, or an explicit no-square value.
- Halfmove clock and fullmove number.
- Incrementally maintained Zobrist hash.

The position API should own state transitions. A `Move` is a compact value describing source, destination, promotion (if any), and the flags needed to distinguish special moves. `MakeMove` returns or fills an `UndoState` containing every prior value needed by `UnmakeMove`. Avoid duplicating mutable board state in the searcher.

Suggested types (names are illustrative, not requirements):

| Type | Contract |
|---|---|
| `Position` | Holds the complete current position and applies/reverts moves |
| `Move` | Value type for a chess move; does not encode mutable position data |
| `UndoState` | Exact snapshot of changed metadata and captured-piece information |
| `MoveGenerator` | Generates pseudo-legal candidates and/or legal moves from a position |
| `AttackGenerator` | Answers whether a square is attacked and supplies attack masks |
| `Fen` | Parses and formats a position without silently accepting malformed input |
| `SearchLimits` | Immutable description of depth, node, clock, and cancellation constraints |
| `SearchResult` | Best move, score, completed depth, principal variation, and statistics |

Avoid adding interfaces solely for hypothetical extensibility. Use value types and direct calls for the performance-sensitive core; isolate console and clock dependencies where tests need deterministic control.

## Module and dependency rules

1. Board/rules code owns legality and position transitions; search must not implement an alternate rules model.
2. Search consumes a `Position`, legal move generation, and limits; it returns a result and never prints protocol text.
3. UCI owns tokenization, command semantics, and serialization. It converts protocol moves to validated `Move` values before passing them into Core.
4. Only the UCI host writes protocol output. Diagnostics go to standard error, never standard output.
5. FEN and UCI move formats are boundary representations; internal code uses typed squares, pieces, moves, and positions.
6. Cancellation and time limits flow from the UCI host into the active search. Do not share one mutable search instance across concurrent searches.

## Runtime flow

1. The UCI process reads one complete line at a time and parses the command.
2. `position` builds a start or FEN position, then replays each supplied move through legal-move validation.
3. `go` creates `SearchLimits` and starts one search. Search reports completed-iteration information to a callback/channel owned by the UCI layer and eventually returns a `SearchResult`.
4. The UCI layer serializes `info` and exactly one terminating `bestmove` for a completed or stopped search.
5. `stop`, replacement of the current position, new-game reset, or process shutdown cancels and joins the active search before mutating or discarding state.

The command loop must remain responsive to `isready` and `stop` while search runs. Serialize output writes so that protocol lines cannot interleave.

## State and invariants

- Exactly one king of each color exists in a valid position.
- Piece bitboards are disjoint; cached occupancies equal their unions.
- Side to move, castling rights, en-passant square, clocks, and hash remain consistent through every make/unmake pair.
- Every generated legal move leaves the moving side’s king unattacked.
- A position restored after `MakeMove`/`UnmakeMove` is bit-for-bit equivalent in all meaningful fields, including hash and clocks.
- No command, parse failure, or engine diagnostic is allowed to produce non-UCI text on stdout.

These are architectural contracts and should become tests before search optimization begins. See [chess rules and move generation](chess-rules-and-move-generation.md) and [testing and roadmap](testing-and-roadmap.md).
