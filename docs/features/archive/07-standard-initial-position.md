# Feature 07 — Standard initial position

**Status:** Archived

**Priority:** 7

**Dependencies:** [05 — Position state and validation](05-position-state-and-validation.md), [06 — Deterministic position hash](06-position-hash.md)

## Summary

Provide a factory for the standard chess starting position using the canonical square indexing and validated position model.

## User story

As a client of Core, I want to request the standard initial position, so that engine startup and protocol setup have a canonical position.

## Acceptance criteria

- [x] The factory returns all 32 starting pieces on their correct squares.
- [x] Side to move is White; all four castling rights are present; en-passant is absent.
- [x] The halfmove clock is zero and fullmove number is one.
- [x] Derived white, black, and total occupancy match the expected starting ranks and piece counts.
- [x] The returned position passes structural validation and has its deterministic position key.
- [x] Repeated calls return equal positions and equal keys without sharing mutable state.

## Verification

- `dotnet test src/EngineDemo.slnx --no-restore --filter FullyQualifiedName~InitialPosition` — 4 passed.
- `dotnet build src/EngineDemo.slnx` — succeeded with 0 warnings and 0 errors.
- `dotnet test src/EngineDemo.slnx --no-restore` — 76 passed, 0 skipped.

## Tests (TDD)

Add Core.Tests cases for exact piece placement by color/type, occupied squares, metadata, validity, and key stability across repeated factory calls.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~InitialPosition
```

## Non-goals

- Generating legal moves from the start position.
- Applying moves or maintaining game history.
- Custom starting positions or chess variants.
