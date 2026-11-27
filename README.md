# RgChess documentation

RgChess is intended to become a standard-chess engine that communicates with chess GUIs and other clients through the Universal Chess Interface (UCI). These documents define a proposed design and implementation path; they do not describe functionality that is already implemented.

## Current baseline

The repository currently contains a minimal .NET 10 console scaffold at `src/EngineDemo`. Its entry point only prints a greeting. Production code, tests, and optional benchmark code are all intended to live under `src/`; the architecture and capabilities below are proposals for the next implementation stages.

## Scope and design defaults

- Language/runtime: C# on .NET 10.
- Game: standard chess only; no chess variants or graphical interface.
- Process interface: UCI over standard input and standard output.
- Board representation: 64-bit bitboards (`ulong`), with a single authoritative position state.
- Runtime dependencies: .NET platform libraries only unless a later decision establishes a clear need.
- Priority: correctness and protocol reliability before playing strength and low-level optimization.

## Reading order

1. [Architecture and module boundaries](docs/road-map/architecture.md)
2. [Chess rules and move generation](docs/road-map/chess-rules-and-move-generation.md)
3. [UCI protocol behavior](docs/road-map/uci-protocol.md)
4. [Search and evaluation](docs/road-map/search-and-evaluation.md)
5. [Testing, benchmarks, and roadmap](docs/road-map/testing-and-roadmap.md)

## Design documents

- [Architecture](docs/road-map/architecture.md) — proposed solution layout, responsibilities, core types, and data flow.
- [UCI protocol](docs/road-map/uci-protocol.md) — commands, options, output, search lifecycle, and error handling.
- [Chess rules and move generation](docs/road-map/chess-rules-and-move-generation.md) — position state, FEN, legal moves, special moves, and perft.
- [Search and evaluation](docs/road-map/search-and-evaluation.md) — search algorithms, evaluation, transposition table, and time management.
- [Testing and roadmap](docs/road-map/testing-and-roadmap.md) — test strategy, measurable gates, benchmarks, and delivery stages.

## Sprint plan

The initial implementation is broken into small, testable feature specs in the [Sprint 1 mission](docs/features/00-mission-01-sprint.md).

## External protocol reference

The engine should follow the UCI protocol conventions documented at [UCI protocol](https://backscattering.de/chess/uci/). This project’s supported command set and development priorities are described in [uci-protocol.md](docs/road-map/uci-protocol.md).

## Copilot agent resources

Repository instructions, custom agents, testing guidance, and reusable skills are maintained under `.github/`. See [Copilot agent customizations](docs/road-map/agent-customizations.md) for the resource map and usage.
