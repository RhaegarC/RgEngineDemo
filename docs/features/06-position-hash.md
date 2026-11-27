# Feature 06 — Deterministic position hash

**Status:** Planned

**Priority:** 6

**Dependencies:** [05 — Position state and validation](05-position-state-and-validation.md)

## Summary

Add a deterministic Zobrist base position key over the board state represented by `Position`. This sprint computes keys for constructed positions; move-time incremental updates are deferred until moves are introduced. The base key does not encode move counters or repetition history; later search transposition-table score reuse must account for the halfmove clock separately.

## User story

As a Core caller, I want equal positions to produce stable keys and distinct states to affect the key, so that later repetition and transposition-table work has a verified foundation.

## Acceptance criteria

- [ ] Zobrist values are generated from a fixed, documented seed and are stable across process runs and runtime versions.
- [ ] The key includes piece placement, side to move, castling rights, and en-passant state.
- [ ] Halfmove/fullmove counters and repetition history do not alter the base position key; later transposition-table score reuse handles the halfmove clock separately.
- [ ] Equal positions produce equal keys independent of construction path.
- [ ] Changing any represented hash-relevant state changes the key in focused tests.
- [ ] Position construction/parsing computes the key without mutating position state.
- [ ] Hash behavior is documented as collision-resistant in practice, not collision-proof.

## Tests (TDD)

Add Core.Tests cases proving deterministic keys for repeated/equivalent positions and sensitivity to piece placement, side to move, each castling right, and en-passant state. Verify no position fields change during key computation.

Run:

```text
dotnet test src/RgChess.slnx --filter FullyQualifiedName~Hash
```

## Non-goals

- Incremental hash updates during `MakeMove`/`UnmakeMove`.
- Repetition equivalence normalization for ineffective en-passant rights.
- Transposition-table storage or search use.
