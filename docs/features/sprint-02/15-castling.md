# Feature 15 — Castling

**Status:** Planned

**Depends on:** [Feature 12 — Ordinary move application](12-ordinary-move-application.md), [Feature 14 — Legal move filtering](14-legal-move-filtering.md)

## Summary

Generate, apply, undo, and validate both standard castling moves for both colors.

## Scope

- Require the relevant right, king/rook on their home squares, and empty intervening squares.
- Reject castling while in check or when the king crosses or lands on an attacked square; rook-path attacks alone do not prohibit castling.
- Update castling rights when a king or home rook moves, or a home rook is captured.
- Restore prior rights and piece placement exactly on unmake.

## Out of scope

- Chess960 castling, search, or broader draw handling.

## Acceptance criteria

- Tests cover both sides/colors, missing rights, missing/misplaced rook or king, blocked paths, attacked start/transit/destination squares, and attacked rook paths.
- King/rook moves and home-rook captures clear only the appropriate rights.
- Make/unmake restores rights, occupancies, clocks, and hash exactly.
- The solution builds and the full test suite passes.
