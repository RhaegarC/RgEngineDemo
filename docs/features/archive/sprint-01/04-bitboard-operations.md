# Feature 04 — Bitboard operations

**Status:** Archived (complete)

**Priority:** 4

**Dependencies:** [01 — Solution and Core test foundation](01-solution-and-core-test-foundation.md), [02 — Square and coordinate notation](02-square-and-coordinate-notation.md)

## Summary

Add the minimal reusable operations for 64-square `ulong` bitboards using the agreed square indexing.

## User story

As a Core caller, I want safe, consistent bitboard operations, so that later position and move-generation code shares one tested implementation.

## Acceptance criteria

- [x] A bit can be set, cleared, and queried using a valid `Square`.
- [x] The population count is correct for empty, singleton, sparse, and full bitboards.
- [x] Set-square enumeration returns every set square exactly once in ascending square-index order.
- [x] Empty enumeration terminates without yielding an invalid square.
- [x] Operations do not mutate their input value unless mutation is explicitly part of the API.

## Tests (TDD)

Core.Tests covers `a1` and `h8` bit operations, repeated set/clear behavior, empty/singleton/sparse/full population counts, enumeration across edge and mixed squares, and rejection of the invalid default `Square`. Enumeration is checked against an independently constructed expected set.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Bitboard
```

Verified: the focused Bitboard tests passed (6); the full solution build completed with zero warnings/errors and all 45 tests passed.

## Non-goals

- Attack tables, sliding-piece rays, move generation, or magic bitboards.
- Performance-specific intrinsics before profiling.
- Position occupancy calculation.
