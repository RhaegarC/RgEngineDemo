# Mission: Sprint 2 — Complete chess rules core

**Status:** Planned

**Sprint:** 2

**Design references:** [Architecture](../road-map/architecture.md), [Chess rules and move generation](../road-map/chess-rules-and-move-generation.md), [Testing and roadmap](../road-map/testing-and-roadmap.md)

## Mission

Complete the standard-chess Core rules layer on top of Sprint 1's position and FEN foundation. At the end of this sprint, callers can detect attacks, generate every legal move, apply and undo moves exactly, count legal positions with perft, and classify the defined draw conditions.

## Subtask sequence

Complete the linked subtasks in dependency order. Each spec defines its own scope and acceptance checks; the mission acceptance criteria remain the overall sprint gate.

| Subtask | Scope | Depends on | Status |
|---|---|---|---|
| [10 — Attack generation](archive/sprint-02/10-attack-generation.md) | Implement independent attack masks and attacked-square queries. | Sprint 1 | Archived |
| [11 — Move and undo-state model](archive/sprint-02/11-move-and-undo-state.md) | Define compact moves and complete undo snapshots. | Sprint 1 | Archived |
| [12 — Ordinary move application](sprint-02/12-ordinary-move-application.md) | Make/unmake ordinary moves and captures with exact state restoration. | 11 | In progress |
| [13 — Pseudo-legal move generation](sprint-02/13-pseudo-legal-move-generation.md) | Generate ordinary pawn and piece moves. | 10, 11, 12 | Planned |
| [14 — Legal move filtering](sprint-02/14-legal-move-filtering.md) | Enforce king safety and classify check, mate, and stalemate. | 10, 12, 13 | Planned |
| [15 — Castling](sprint-02/15-castling.md) | Implement castling eligibility, move application, and restoration. | 12, 14 | Planned |
| [16 — En passant](sprint-02/16-en-passant.md) | Implement en-passant state, capture, and discovered-check legality. | 12, 14 | Planned |
| [17 — Promotion](sprint-02/17-promotion.md) | Implement all promotion choices and promotion captures. | 12, 14 | Planned |
| [18 — UCI move notation and perft](sprint-02/18-uci-move-notation-and-perft.md) | Parse/format legal moves and pass required perft gates. | 14–17 | Planned |
| [19 — Game history and draw rules](sprint-02/19-game-history-and-draw-rules.md) | Track normalized repetition identity and resolve draw conditions. | 15–18 | Planned |

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
