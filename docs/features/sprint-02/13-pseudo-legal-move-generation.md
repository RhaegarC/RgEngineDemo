# Feature 13 — Pseudo-legal move generation

**Status:** In progress

**Depends on:** [Feature 10 — Attack generation](../archive/sprint-02/10-attack-generation.md), [Feature 11 — Move and undo-state model](../archive/sprint-02/11-move-and-undo-state.md), [Feature 12 — Ordinary move application](../archive/sprint-02/12-ordinary-move-application.md)

## Summary

Generate ordinary pseudo-legal moves for every standard piece type before king-safety filtering.

## Scope

- Generate pawn single/double pushes and diagonal captures that do not promote.
- Generate knight, bishop, rook, queen, and king moves with friendly-occupancy exclusion.
- Expose `MoveGenerator.GeneratePseudoLegalMoves(Position, Span<Move>)`; return the count and throw if the destination span cannot hold every candidate.
- Order moves by piece type (pawn, knight, bishop, rook, queen, king), then source square and destination square in ascending index order.
- Keep castling, en-passant, and promotion generation for their dedicated subtasks.

## Out of scope

- Legal filtering, check evasion guarantees, special moves, search ordering heuristics, or perft optimization.

## Acceptance criteria

- Tests cover piece movement, pawn movement, captures, friendly blockers, board edges, and deterministic output.
- Generated candidates remain within board bounds and do not capture friendly pieces or the opposing king.
- Buffer exhaustion is surfaced and never silently truncates candidates.
- Deterministic ordering is stable across repeated calls for the same position.
- The solution builds and the full test suite passes.
