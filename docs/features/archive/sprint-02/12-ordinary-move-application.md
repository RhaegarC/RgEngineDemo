# Feature 12 — Ordinary move application

**Status:** Archived

**Depends on:** [Feature 11 — Move and undo-state model](11-move-and-undo-state.md)

## Summary

Apply and undo ordinary non-castling, non-en-passant, non-promotion moves, including captures, without losing position state.

## Scope

- Expose `Position.MakeMove(Move)` returning an `UndoState` and `Position.UnmakeMove(Move, UndoState)` for strict reverse-order restoration.
- Update piece bitboards, cached occupancies, side to move, en-passant state, halfmove clock, fullmove number, and Zobrist hash.
- Capture prior values in `UndoState`; restore them directly rather than reconstructing metadata.
- Maintain a hash-history stack beginning with the initial position and append/remove entries in make/unmake order.
- Keep move legality checks separate from the position transition primitive; do not test king safety or general piece geometry.

## Out of scope

- Special-move state transitions, legal-move generation, draw adjudication, or search.
- Cloning the entire position for each transition.

## Acceptance criteria

- Tests cover quiet moves, captures, pawn double pushes, clocks, side-to-move changes, and exact undo.
- Every make/unmake pair restores all meaningful position fields, occupancy caches, history, and hash.
- Invalid transitions fail explicitly without leaving partial state changes.
- Unsupported castling, en-passant, and promotion transitions fail explicitly until their dedicated features.
- The solution builds and the full test suite passes.
