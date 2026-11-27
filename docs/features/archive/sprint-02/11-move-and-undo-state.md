# Feature 11 — Move and undo-state model

**Status:** Archived

**Depends on:** [Sprint 1 — Core foundation and position notation](../sprint-01/00-mission-01-sprint.md)

## Summary

Define compact value types for a chess move and the exact information needed to reverse it.

## Scope

- Represent source, destination, optional promotion piece, and flags needed for special moves.
- Define `UndoState` fields for captured-piece information and all prior position metadata required by unmake.
- Keep move data independent of mutable position state and suitable for bounded per-ply storage.
- Encode board squares as valid indices from 0 through 63; reject a source equal to its destination.
- Allow promotion only to a knight, bishop, rook, or queen. The default `Move` is invalid and reports `IsValid == false`.
- Represent captures, double pawn pushes, en passant, and both castling sides with flags; reject unknown or incompatible flags.
- Preserve the previous side to move, castling rights, en-passant target, halfmove/fullmove counters, position hash, and captured piece/square in `UndoState`.
- Document invalid/default move behavior and the valid ranges for encoded fields.

## Out of scope

- Applying moves, generating legal moves, or parsing UCI text.
- Heap-allocated move objects or speculative extensibility interfaces.

## Acceptance criteria

- Tests cover valid ordinary and special-move values, promotion choices, and invalid field boundaries.
- Move values are compact value types and do not contain mutable position data.
- Undo state can represent the prior metadata needed by ordinary and special-move restoration.
- The solution builds and the full test suite passes.
