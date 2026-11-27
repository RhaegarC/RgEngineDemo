# Feature 08 — FEN parsing

**Status:** Planned

**Priority:** 8

**Dependencies:** [02 — Square and coordinate notation](archive/02-square-and-coordinate-notation.md), [03 — Piece and color model](archive/03-piece-and-color-model.md), [05 — Position state and validation](05-position-state-and-validation.md), [06 — Deterministic position hash](06-position-hash.md)

## Summary

Parse all six standard FEN fields into a validated `Position`, returning explicit errors for malformed or structurally invalid input.

## User story

As a Core caller, I want to load a position from FEN, so that tests and future UCI `position fen` commands can create a precise board state.

## Acceptance criteria

- [ ] Exactly six whitespace-separated fields are required.
- [ ] Piece placement has exactly eight ranks, each expanding to exactly eight squares, with only recognized piece symbols and digits 1–8.
- [ ] Side to move is exactly `w` or `b`.
- [ ] Castling rights accept only unique `KQkq` symbols or `-`.
- [ ] En-passant is `-` or a valid coordinate consistent with the FEN side-to-move rank convention.
- [ ] Halfmove clock parses as a non-negative integer; fullmove number parses as an integer of at least one.
- [ ] Structural position invariants are enforced; malformed input is never silently replaced or normalized.
- [ ] Errors identify the invalid field or structural condition sufficiently for a caller to report it.
- [ ] A valid parsed position has correct occupancy and a deterministic position key.

## Tests (TDD)

Add Core.Tests cases for the initial-position FEN, a valid sparse position, each metadata field, malformed rank widths/counts, invalid symbols/side/castling/en-passant/counters, and missing or duplicate kings. Assert both the error result and useful field context for invalid inputs.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~FenParser
```

## Non-goals

- Validating that a position is reachable from the standard start.
- Parsing a move list after the FEN.
- Repairing invalid castling/en-passant claims or ignoring extra fields.
