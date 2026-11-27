# Feature 02 — Square and coordinate notation

**Status:** Complete

**Priority:** 2

**Dependencies:** [01 — Solution and Core test foundation](archive/01-solution-and-core-test-foundation.md)

## Summary

Add a strongly typed square value with the engine's documented 0–63 indexing and strict coordinate parsing/formatting.

## User story

As a Core caller, I want squares represented by a validated type, so that board indexing and algebraic coordinates cannot silently disagree.

## Acceptance criteria

- [x] `a1` maps to index 0, `h1` to 7, `a8` to 56, and `h8` to 63.
- [x] A square can be created only for an index from 0 through 63.
- [x] Valid lowercase coordinates (`a1` through `h8`) parse and format back to the same coordinate.
- [x] Invalid length, file, rank, casing, and out-of-range indices are rejected explicitly.
- [x] The type exposes zero-based `File` and `Rank` accessors consistent with the index mapping.
- [x] Invalid `TryParse` calls return failure and leave the output square invalid.

## Tests (TDD)

Core.Tests covers the four corner mappings, all 64 coordinate round-trips, file/rank accessors, invalid coordinates/indices, and the invalid default output from failed `TryParse` calls.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Square
```

Verified: the focused Square tests passed (20); the full solution build completed with zero warnings/errors and all 20 tests passed.

## Non-goals

- Piece movement or attack rules.
- Parsing FEN or UCI moves.
- Supporting uppercase or non-standard square notation.
