# Mission: Sprint 2 — Complete chess rules core

**Status:** Planned

**Sprint:** 2

**Design references:** [Architecture](../road-map/architecture.md), [Chess rules and move generation](../road-map/chess-rules-and-move-generation.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Complete the standard-chess Core rules layer on top of Sprint 1's position and FEN foundation. At the end of this sprint, callers can detect attacks, generate every legal move, apply and undo moves exactly, count legal positions with perft, and classify the defined draw conditions.

## Subtasks

- [ ] Define compact `Move` and complete `UndoState` value types, including promotion and special-move information.
- [ ] Implement portable attack masks and square-attacked queries for every piece type, including blocker handling and attacks by pinned pieces.
- [ ] Add bit scans/popcounts using `System.Numerics.BitOperations` and a flat-array Magic Bitboard baseline for sliding attacks.
- [ ] Implement make/unmake for quiet moves and captures, restoring all piece masks, occupancies, side, rights, en-passant state, clocks, history, and hash.
- [ ] Generate pseudo-legal pawn, knight, bishop, rook, queen, and king moves.
- [ ] Implement castling, en-passant, all promotions and promotion captures, with exact king-safety rules.
- [ ] Filter pseudo-legal candidates into legal moves, covering check, pins, double check, king moves, and attacked castling transit squares.
- [ ] Parse and format legal UCI coordinate moves, including lowercase promotion suffixes.
- [ ] Add perft/divide test helpers and pass initial-position and Kiwipete reference counts.
- [ ] Track normalized repetition identity and implement claimable/automatic draws, insufficient material, and terminal-state precedence.

## Out of scope

- Search, UCI process commands, SAN/PGN, Chess960, and performance tuning beyond the portable attack baseline.
- Rejecting structurally valid composed positions solely because they are unusual.

## Acceptance criteria

- Initial-position perft matches 20, 400, 8,902, and 197,281 at depths 1–4; Kiwipete matches 48, 2,039, and 97,862 at depths 1–3.
- Focused tests cover special moves, checks/pins, discovered attacks, checkmate, stalemate, all promotion types, and illegal en-passant exposure.
- Make/unmake restores every meaningful position and history field exactly.
- Repetition identity distinguishes en-passant only when a legal en-passant capture exists.
- Tests cover threefold/fifty-move handling, automatic fivefold/75-move draws, insufficient material, and checkmate precedence over draw thresholds.
- The solution builds and the full test suite passes; no search capability is claimed.
