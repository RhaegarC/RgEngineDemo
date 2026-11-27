# Feature 18 — UCI move notation and perft

**Status:** Planned

**Depends on:** [Feature 14 — Legal move filtering](14-legal-move-filtering.md), [Feature 15 — Castling](15-castling.md), [Feature 16 — En passant](16-en-passant.md), [Feature 17 — Promotion](17-promotion.md)

## Summary

Complete the legal-move boundary representation and establish the sprint's required move-generation correctness gates.

## Scope

- Parse coordinate moves and lowercase promotion suffixes by matching against generated legal moves.
- Format legal moves deterministically in UCI coordinate notation.
- Add perft and divide test helpers that count legal leaf positions without evaluation or pruning.
- Add the required initial-position, Kiwipete, and targeted special-move perft fixtures.

## Out of scope

- SAN/PGN, Chess960 notation, benchmark claims, or weakening expected node counts.

## Acceptance criteria

- Initial-position perft matches 20, 400, 8,902, and 197,281 at depths 1–4.
- Kiwipete (`r3k2r/p1ppqpb1/bn2pnp1/2pP4/1p2P3/2N2N2/PPQBBPPP/R3K2R w KQkq - 0 1`) matches 48, 2,039, and 97,862 at depths 1–3.
- Focused fixtures cover castling, promotions, en-passant pins, checks, and discovered attacks.
- Invalid or illegal UCI moves fail explicitly; every formatted move is legal and round-trips through parsing.
- The solution builds and the full test suite passes.
