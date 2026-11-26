---
name: list-features
description: List all features with their current status
disable-model-invocation: true
---

List every feature in `docs/features/` (including `docs/features/archive/` if it exists), excluding
the non-features: the sprint index and `backlog.md`.

For each feature show:
- **Number + name** (from the filename)
- **Priority** (lower number = higher)
- **Status**: not started / in progress / done & archived
- **Dependencies**

Determine status from:
- Which files are in `docs/features/` vs. `docs/features/archive/`
- Open `feature/*` branches and open pull requests (`gh pr list`) — in progress
- Whether the corresponding branch has been merged to `develop`

Present as a **table sorted by number**, then a one-line "next up" recommendation (the
lowest-numbered feature that is not started and not in progress).

**Surface any sequencing caveat the sprint index declares.** If the sprint file records that some
feature makes `develop` unsafe to deploy until it lands, and that feature is not merged, say so
plainly rather than letting a clean-looking table imply otherwise. Read the caveat from the sprint
file; do not carry the rule here, where it would drift.
