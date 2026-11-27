# Feature 05 — Position state and validation

**Status:** Archived (complete)

**Priority:** 5

**Dependencies:** [02 — Square and coordinate notation](02-square-and-coordinate-notation.md), [03 — Piece and color model](03-piece-and-color-model.md), [04 — Bitboard operations](04-bitboard-operations.md)

## Summary

Create the Core `Position` representation for standard-chess state, validate its structural invariants, and derive side/total occupancy from the twelve piece bitboards.

## User story

As a Core caller, I want a validated position value, so that parsers and later move/search code operate on consistent board and metadata state.

## Acceptance criteria

- [x] A position stores twelve colored piece bitboards, side to move, four castling rights, optional en-passant target, halfmove clock, and fullmove number.
- [x] White, Black, and total occupancy are derived consistently from piece bitboards, not maintained as conflicting independent input.
- [x] Piece bitboards are disjoint; overlap is rejected with a useful error.
- [x] A structurally valid position contains exactly one king of each color.
- [x] The halfmove clock is non-negative and the fullmove number is at least one.
- [x] No en-passant target is represented explicitly as absent; present targets are valid board squares.
- [x] Equality or an equivalent comparison includes all state fields needed to distinguish positions.

## Tests (TDD)

Core.Tests covers empty-side and mixed-color occupancy derivation, a valid sparse position, overlapping piece masks, missing/duplicate kings, invalid clocks, en-passant validity, invalid metadata enums, and equality when each metadata field changes.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Position
```

Verified: the focused Position tests passed (15); the full solution build completed with zero warnings/errors and all 60 tests passed.

## Non-goals

- Move application/undo, repetition history, legal-position reachability, and draw adjudication.
- Hashing (covered by Feature 06).
- Accepting malformed positions and silently repairing them.
