# Feature 10 — Attack generation

**Status:** Archived

**Depends on:** [Sprint 1 — Core foundation and position notation](../sprint-01/00-mission-01-sprint.md)

## Summary

Implement attack masks and attacked-square queries for standard chess pieces, independently from legal-move generation.

## Scope

- Generate pawn, knight, king, bishop, rook, and queen attacks from a square.
- Account for blockers in sliding attacks and provide position-level queries for whether a square is attacked by a color.
- Treat attacks by pinned enemy pieces according to attacked-square rules; do not ask whether the attacker has a legal move.
- Use `System.Numerics.BitOperations` for bit scans/popcounts and flat-array Magic Bitboards as the portable sliding-attack baseline.

## Out of scope

- Move generation, move legality, make/unmake, perft, search, or protocol behavior.
- Hardware-specific intrinsics or alternative sliding lookup strategies without later benchmark evidence.

## Acceptance criteria

- Tests cover each piece's attack geometry, board edges, blockers, and occupied target squares.
- Sliding attacks stop at the nearest blocker and include the blocker square.
- Pinned enemy pieces still contribute attacks to attacked-square queries.
- Attack detection is independent of legal-move generation and does not recurse through it.
- The solution builds and the full test suite passes.
