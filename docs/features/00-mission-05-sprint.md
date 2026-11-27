# Mission: Sprint 5 — Complete UCI search controls

**Status:** Planned

**Sprint:** 5

**Design references:** [UCI protocol](../road-map/uci-protocol.md), [Search and evaluation](../road-map/search-and-evaluation.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Complete the planned UCI search-control surface and verify that all limits and lifecycle commands remain bounded and coherent. At the end of this sprint, supported `go` parameters are validated explicitly and the engine behaves predictably under clocks, stops, and command replacement.

## Subtasks

- [ ] Define and test accepted `go` grammar, including rejection of unknown, malformed, and conflicting limit tokens.
- [ ] Implement hard `nodes` limits at node boundaries without intentional overshoot, counting quiescence nodes consistently.
- [ ] Implement side-to-move `wtime`/`btime`, increments, and `movestogo` allocation with a documented reserve.
- [ ] Make monotonic clock access injectable for deterministic allocation and deadline tests.
- [ ] Bound deadline polling delay and measure response behavior, including quiescence and scheduler/protocol overhead.
- [ ] Implement `searchmoves` validation and restrict root search to the supplied legal moves.
- [ ] Implement `ponder`/`ponderhit` only with a correct lifecycle and valid ponder output; otherwise do not advertise support.
- [ ] Verify `infinite`, one-legal-move positions, and explicit depth/node/movetime limits do not interfere with each other.
- [ ] Exercise stop, replacement `position`, `ucinewgame`, replacement `go`, Hash resize, and quit while a search is active.
- [ ] Extend process-level UCI tests for exact terminal output, malformed limits, clock boundaries, and repeated command cycles; document supported behavior.

## Out of scope

- Multiple search threads, Chess960, and unimplemented UCI commands/options.
- Promising exact wall-clock cutoff or weakening hard depth/node limits.

## Acceptance criteria

- Depth and node limits are hard; time limits return within the documented polling and safety margin.
- Clock allocation respects side-to-move time, increments, moves-to-go, and reserve; one legal move does not skip continuation search.
- `infinite` continues until cancellation, and invalid limits never start an unbounded fallback search.
- `searchmoves` returns only an allowed legal move; ponder behavior and output are valid for the implemented protocol lifecycle.
- Every started search reaches one terminal path, with no stale/duplicate `bestmove`, leaked task, or hung quit.
- The documented command/option table exactly matches tested behavior; the full solution and test suite pass.
