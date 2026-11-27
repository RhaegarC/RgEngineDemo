# Feature 04 — Bitboard operations

**Status:** Planned

**Priority:** 4

**Dependencies:** [01 — Solution and Core test foundation](archive/01-solution-and-core-test-foundation.md), [02 — Square and coordinate notation](archive/02-square-and-coordinate-notation.md)

## Summary

Add the minimal reusable operations for 64-square `ulong` bitboards using the agreed square indexing.

## User story

As a Core caller, I want safe, consistent bitboard operations, so that later position and move-generation code shares one tested implementation.

## Acceptance criteria

- [ ] A bit can be set, cleared, and queried using a valid `Square`.
- [ ] The population count is correct for empty, singleton, sparse, and full bitboards.
- [ ] Set-square enumeration returns every set square exactly once, in a documented deterministic order.
- [ ] Empty enumeration terminates without yielding an invalid square.
- [ ] Operations do not mutate their input value unless mutation is explicitly part of the API.

## Tests (TDD)

Add Core.Tests cases for `a1` and `h8` bit operations, repeated set/clear behavior, empty/full population counts, and enumeration across edge and mixed squares. Compare enumerated squares with an independently constructed expected set.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Bitboard
```

## Non-goals

- Attack tables, sliding-piece rays, move generation, or magic bitboards.
- Performance-specific intrinsics before profiling.
- Position occupancy calculation.
