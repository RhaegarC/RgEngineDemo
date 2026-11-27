# Feature 02 — Square and coordinate notation

**Status:** Planned

**Priority:** 2

**Dependencies:** [01 — Solution and Core test foundation](01-solution-and-core-test-foundation.md)

## Summary

Add a strongly typed square value with the engine's documented 0–63 indexing and strict coordinate parsing/formatting.

## User story

As a Core caller, I want squares represented by a validated type, so that board indexing and algebraic coordinates cannot silently disagree.

## Acceptance criteria

- [ ] `a1` maps to index 0, `h1` to 7, `a8` to 56, and `h8` to 63.
- [ ] A square can be created only for an index from 0 through 63.
- [ ] Valid lowercase coordinates (`a1` through `h8`) parse and format back to the same coordinate.
- [ ] Invalid length, file, rank, casing, and out-of-range indices are rejected explicitly.
- [ ] The type exposes rank/file or equivalent accessors consistent with the index mapping.

## Tests (TDD)

Add Core.Tests cases for the four corner mappings, all 64 coordinate round-trips, file/rank accessors, and invalid coordinates/indices. Invalid `TryParse` calls must return failure without exposing a valid square.

Run:

```text
dotnet test src/RgChess.slnx --filter FullyQualifiedName~Square
```

## Non-goals

- Piece movement or attack rules.
- Parsing FEN or UCI moves.
- Supporting uppercase or non-standard square notation.
