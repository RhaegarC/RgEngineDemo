# Feature 16 — En passant

**Status:** Planned

**Depends on:** [Feature 12 — Ordinary move application](../archive/sprint-02/12-ordinary-move-application.md), [Feature 14 — Legal move filtering](14-legal-move-filtering.md)

## Summary

Implement the en-passant target lifecycle, capture transition, undo, and king-safety validation.

## Scope

- Set the target after a two-square pawn advance and clear it after the immediately following ply if unused.
- Generate a capture only when a capturing pawn and valid target are present.
- Remove the captured pawn from its actual square and restore it during unmake.
- Reject en-passant captures that expose the moving king to a rook, bishop, or queen line.

## Out of scope

- Promotion interactions, repetition-key normalization, and search.

## Acceptance criteria

- Tests cover both colors, expired/missing targets, correct captured-pawn removal, and exact unmake.
- A discovered-check en-passant candidate is rejected even when its destination appears safe.
- Position hash and metadata match the resulting position state and restore exactly.
- The solution builds and the full test suite passes.
