# Feature 19 — Game history and draw rules

**Status:** Planned

**Depends on:** [Feature 15 — Castling](15-castling.md), [Feature 16 — En passant](16-en-passant.md), [Feature 17 — Promotion](17-promotion.md), [Feature 18 — UCI move notation and perft](18-uci-move-notation-and-perft.md)

## Summary

Track repetition identity and classify standard-chess draw conditions without confusing game history with future search-cycle heuristics.

## Scope

- Track position keys through make/unmake and identify repetitions using a normalized key.
- Treat an en-passant right as repetition-relevant only when a legal en-passant capture exists.
- Recognize claimable threefold/fifty-move draws, automatic fivefold/75-move draws, and explicitly defined insufficient material.
- Resolve terminal states in the required order: checkmate, recognized draw, stalemate, then ongoing play.

## Out of scope

- Search-cycle repetition scoring and transposition-table score reuse.
- Executing a player's formal draw claim through a GUI.

## Acceptance criteria

- Tests cover reversible-move history, repetition counts, halfmove thresholds, automatic thresholds, and insufficient-material cases.
- Positions differing only by an unusable en-passant target have the same repetition identity; a legally capturable target distinguishes them.
- Checkmate takes precedence when a draw threshold is also reached; a qualifying draw precedes stalemate otherwise.
- Make/unmake restores repetition history and all position state exactly.
- The solution builds and the full test suite passes.
