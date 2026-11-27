# Feature 03 — Piece and color model

**Status:** Planned

**Priority:** 3

**Dependencies:** [01 — Solution and Core test foundation](01-solution-and-core-test-foundation.md)

## Summary

Define typed chess colors and piece kinds, plus a small piece representation and reversible mapping to the twelve standard FEN piece symbols.

## User story

As a Core caller, I want color and piece kinds represented explicitly, so that position storage and FEN parsing do not rely on ambiguous characters or magic integer values.

## Acceptance criteria

- [ ] `Color` represents exactly White and Black.
- [ ] `PieceType` represents Pawn, Knight, Bishop, Rook, Queen, and King.
- [ ] A `Piece` value combines one valid color and one valid piece type.
- [ ] Each of the twelve colored pieces maps to exactly one standard FEN symbol, with case determined by color.
- [ ] Every valid FEN piece symbol maps back to the same `Piece`.
- [ ] Unknown symbols and invalid enum values are rejected explicitly.

## Tests (TDD)

Add Core.Tests cases covering all twelve symbol mappings and round-trips, color/case behavior, and rejection of unsupported symbols or invalid values. Avoid tests that only assert enum numeric constants.

Run:

```text
dotnet test src/EngineDemo.slnx --filter FullyQualifiedName~Piece
```

## Non-goals

- Piece movement, attacks, material values, or evaluation.
- FEN rank parsing.
- Chess variants or additional piece kinds.
