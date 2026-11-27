# Feature 14 — Legal move filtering

**Status:** Planned

**Depends on:** [Feature 10 — Attack generation](../archive/sprint-02/10-attack-generation.md), [Feature 12 — Ordinary move application](../archive/sprint-02/12-ordinary-move-application.md), [Feature 13 — Pseudo-legal move generation](13-pseudo-legal-move-generation.md)

## Summary

Filter ordinary pseudo-legal candidates so every returned move leaves the moving side's king safe.

## Scope

- Apply each candidate, test the moving side's king against independent attack queries, then restore the position.
- Support check evasion, pins, king moves, and double-check behavior through the legality filter.
- Expose check status and distinguish checkmate from stalemate when no legal move exists.
- Keep special-move legality for the castling, en-passant, and promotion subtasks.

## Out of scope

- Direct pin-aware generation optimization, draw adjudication, or search.

## Acceptance criteria

- Every returned legal move leaves its moving king unattacked.
- Tests cover pinned pieces, single/double check, king moves into attack, checkmate, and stalemate.
- Attack detection does not depend on legal-move generation.
- The position is restored exactly after candidate testing.
- The solution builds and the full test suite passes.
