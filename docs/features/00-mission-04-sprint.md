# Mission: Sprint 4 — Tactical search and transposition table

**Status:** Planned

**Sprint:** 4

**Design references:** [Chess rules and move generation](../road-map/chess-rules-and-move-generation.md), [Search and evaluation](../road-map/search-and-evaluation.md), [UCI protocol](../road-map/uci-protocol.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Improve search quality and reuse without weakening legality, score correctness, or stop responsiveness. At the end of this sprint, the engine searches tactical continuations, reports mate/PV information consistently, and uses a bounded transposition table only when its bounds and draw context are safe.

## Subtasks

- [ ] Add staged ordering for the previous principal variation, captures/promotions, killer moves, and history-scored quiet moves.
- [ ] Implement quiescence search for captures/promotions and all legal evasions when in check.
- [ ] Bound quiescence and maximum-ply behavior so checked positions are never misclassified as quiet stand-pat nodes.
- [ ] Define centipawn and mate-score conventions; normalize mate scores by ply for table storage and retrieval.
- [ ] Report measured principal variation, score, nodes, and elapsed-time fields consistently through UCI.
- [ ] Implement a bounded managed transposition table with full-key validation, depth, bound type, best move, score, and generation.
- [ ] Apply exact/lower/upper bound semantics and use shallow entries for ordering without unsafe cutoffs.
- [ ] Add and advertise a bounded `Hash` option; cancel/join active search before resize and implement hash clearing only if supported.
- [ ] Protect table score reuse across halfmove-clock and repetition contexts; never persist path-dependent repetition scores or infer draws from zero scores.
- [ ] Add focused tactical, quiescence, mate-distance, table-bound, collision, draw-context, and cancellation regressions.

## Out of scope

- Native-memory or parallel tables, SEE pruning, PVS, aspiration windows, and late-move reductions.
- Evaluation expansion or optimization without the benchmark baseline planned for Sprint 6.

## Acceptance criteria

- Quiescence searches all legal evasions while checked, checks cancellation/limits, and terminates under a documented bound.
- Mate scores prefer faster wins and slower losses; PV moves and score perspective remain valid.
- Table cutoffs require matching keys and valid depth/bounds/context; unsafe contexts may still use the stored move for ordering.
- Hash resizing never races an active search; table clear/new-game behavior follows a documented policy.
- Regression tests distinguish checkmate, stalemate, automatic and claimable draws, and search-cycle repetition.
- The solution builds and all prior perft, search, and UCI tests pass.
