# Feature 09 — FEN serialization

**Status:** Planned

**Priority:** 9

**Dependencies:** [07 — Standard initial position](07-standard-initial-position.md), [08 — FEN parsing](08-fen-parsing.md)

## Summary

Serialize every valid `Position` to deterministic six-field standard FEN.

## User story

As a Core caller, I want a position serialized to canonical FEN, so that positions can be logged, compared, and round-tripped through protocol boundaries.

## Acceptance criteria

- [ ] Piece placement emits eight ranks, compresses consecutive empty squares, and uses the standard FEN symbols.
- [ ] Side to move, castling rights, en-passant, halfmove clock, and fullmove number are all emitted.
- [ ] Castling rights use canonical `KQkq` order; absent castling/en-passant state uses `-`.
- [ ] Output is deterministic and contains exactly six fields.
- [ ] The standard initial position serializes to `rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1`.
- [ ] Parsing a serialized position reconstructs an equal position with the same position key.

## Tests (TDD)

Add Core.Tests cases for exact initial-position output, sparse boards with multiple empty runs, each castling/en-passant/clock state, stable repeated output, and parse/serialize/parse equality for representative valid FEN fixtures.

Run:

```text
dotnet test src/RgChess.slnx --filter FullyQualifiedName~FenSerializer
```

## Non-goals

- SAN or PGN output.
- Serializing repetition history, which FEN does not encode.
- Including engine search state or protocol data in FEN.
