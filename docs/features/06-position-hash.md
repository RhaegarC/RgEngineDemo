# Feature 06 — Deterministic position hash

**Status:** Complete

**Priority:** 6

**Dependencies:** [05 — Position state and validation](archive/05-position-state-and-validation.md)

## Hash definition

The base position key is a 64-bit Zobrist hash. Its keys are generated once in a
fixed order using SplitMix64 with seed `0xC0FFEE123456789A`. SplitMix64 uses
unsigned 64-bit arithmetic with wraparound:

1. For each of 12 piece types, in the order White Pawn, Knight, Bishop, Rook,
   Queen, King, then Black Pawn, Knight, Bishop, Rook, Queen, King, generate
   one key for each square in ascending index order (0–63).
2. Generate keys for White king-side, White queen-side, Black king-side, and
   Black queen-side castling rights.
3. Generate one key for each en-passant target square in ascending index order.
4. Generate the side-to-move key; it is XORed into the position key when Black
   is to move.

The position key is the XOR of the keys for all occupied piece-square pairs,
enabled castling rights, the en-passant target when present, and the side key
when Black is to move. Move counters and repetition history are not included.
The fixed seed, algorithm, and ordering make keys stable across process runs
and runtime versions. As with any 64-bit hash, collisions are possible: the key
is collision-resistant in practice, not collision-proof.

## Summary

Add a deterministic Zobrist base position key over the board state represented by `Position`. This sprint computes keys for constructed positions; move-time incremental updates are deferred until moves are introduced. The base key does not encode move counters or repetition history; later search transposition-table score reuse must account for the halfmove clock separately.

## User story

As a Core caller, I want equal positions to produce stable keys and distinct states to affect the key, so that later repetition and transposition-table work has a verified foundation.

## Acceptance criteria

- [x] Zobrist values are generated from a fixed, documented seed and are stable across process runs and runtime versions.
- [x] The key includes piece placement, side to move, castling rights, and en-passant state.
- [x] Halfmove/fullmove counters and repetition history do not alter the base position key; later transposition-table score reuse handles the halfmove clock separately.
- [x] Equal positions produce equal keys independent of construction path.
- [x] Changing any represented hash-relevant state changes the key in focused tests.
- [x] Position construction computes the key without mutating position state.
- [x] Hash behavior is documented as collision-resistant in practice, not collision-proof.

## Verification

- `dotnet test src/EngineDemo.slnx --no-restore --filter FullyQualifiedName~Hash` — 12 passed.
- `dotnet build src/EngineDemo.slnx` — succeeded with 0 warnings and 0 errors.
- `dotnet test src/EngineDemo.slnx --no-restore` — 72 passed, 0 skipped.

## Tests (TDD)

Add Core.Tests cases proving deterministic keys for repeated/equivalent positions and sensitivity to piece placement, side to move, each castling right, and en-passant state. Verify no position fields change during key computation.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Hash
```

## Non-goals

- Incremental hash updates during `MakeMove`/`UnmakeMove`.
- Repetition equivalence normalization for ineffective en-passant rights.
- Transposition-table storage or search use.
