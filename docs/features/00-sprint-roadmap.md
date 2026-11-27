# RgChess sprint roadmap

**Status:** Sprint 1 complete; Sprints 2–6 planned.

This index organizes the requirements in [`docs/road-map/`](../road-map/) into six delivery-sized sprints. Each planned sprint has about ten implementation subtasks, a clear mission boundary, and measurable acceptance gates. The subtasks are planning-level work items; implementation details can be refined when a sprint starts.

## Sprint sequence

| Sprint | Mission | Main outcome | Depends on |
|---|---|---|---|
| 1 | [Core foundation and position notation](archive/00-mission-01-sprint.md) | .NET solution, position primitives, initial position, and strict FEN support. | Complete |
| 2 | [Complete chess rules core](00-mission-02-sprint.md) | Attack detection, reversible moves, legal move generation, perft, and draw rules. | Sprint 1 |
| 3 | [UCI baseline and usable search](00-mission-03-sprint.md) | Reliable protocol host with deterministic search for depth, movetime, and infinite modes. | Sprint 2 |
| 4 | [Tactical search and transposition table](00-mission-04-sprint.md) | Quiescence, move ordering, mate scores, and safe bounded hash reuse. | Sprint 3 |
| 5 | [Complete UCI search controls](00-mission-05-sprint.md) | Node/clock limits, search restrictions, pondering, and lifecycle hardening. | Sprint 4 |
| 6 | [Benchmarking and evidence-based improvements](00-mission-06-sprint.md) | Reproducible performance baseline, compatibility checks, and measured improvements. | Sprint 5 |

This sequence consolidates the earlier roadmap into broader outcomes: Sprint 2 completes the rules core before search; Sprint 3 delivers a usable single-threaded UCI engine; Sprint 4 completes baseline tactical-search and table behavior; Sprint 5 completes the planned UCI control surface; Sprint 6 handles measurement and optional tuning.

## Shared constraints

- Follow the module boundaries, namespace, runtime, and source-location rules in [architecture](../road-map/architecture.md). Production and test source stays under `src/`; tests stay under `src/tests/`; the solution stays under `src/EngineDemo.slnx`.
- Core owns chess state, rules, and search. It does not read or write protocol text. The UCI host owns parsing, lifecycle, and protocol output.
- Implement standard chess only. Do not add a GUI, variants, network service, opening book, or tablebases.
- Treat the [chess rules](../road-map/chess-rules-and-move-generation.md), [UCI contract](../road-map/uci-protocol.md), and [search design](../road-map/search-and-evaluation.md) as behavioral sources of truth. Do not claim support for behavior before its acceptance gate passes.
- The [Copilot agent customizations guide](../road-map/agent-customizations.md) describes repository tooling and is not an engine feature or sprint deliverable.
- Validate in-scope behavior using the test layers and measurable gates in [testing and roadmap](../road-map/testing-and-roadmap.md). Keep performance claims separate from correctness claims.

## Scope policy

Subtasks are grouped by shippable outcomes rather than individual methods or test cases. Keep each sprint focused on its mission and acceptance gates; if implementation evidence shows a sprint cannot fit, revise the sprint boundary instead of weakening correctness requirements.
