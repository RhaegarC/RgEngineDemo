# Feature 17 — Promotion

**Status:** Planned

**Depends on:** [Feature 12 — Ordinary move application](../archive/sprint-02/12-ordinary-move-application.md), [Feature 14 — Legal move filtering](14-legal-move-filtering.md)

## Summary

Generate and apply every legal pawn promotion, including capture promotions.

## Scope

- Generate queen, rook, bishop, and knight promotions for quiet moves and captures.
- Update piece bitboards, occupancy, captured-piece data, clocks, hash, and history during promotion.
- Restore the pawn and captured piece exactly during unmake.
- Reject promotion values and destination ranks that are invalid for the move.

## Out of scope

- Underpromotion search heuristics, SAN, and non-standard variants.

## Acceptance criteria

- Tests cover all four promotion types for both colors, quiet and capture promotions, and invalid promotion inputs.
- Every promotion move is legal-filtered and represents the correct promoted piece.
- Make/unmake restores all board and metadata state exactly.
- The solution builds and the full test suite passes.
