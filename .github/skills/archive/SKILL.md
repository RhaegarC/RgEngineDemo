---
name: archive
description: Archive a completed feature after its PR is merged to develop
disable-model-invocation: true
---

You are executing the `/archive` command (Phase 8 of the `tdd-implement` agent —
`.github/agents/tdd-implement.agent.md`). Use the feature number or file path supplied with the invocation to resolve `docs/features/<number>-<name>.md`; if none is supplied, ask which feature to archive.

**Only archive after the feature PR is merged to `develop`:**

1. **Verify the merge** — confirm the feature's changes are in `origin/develop` (check
   `git branch -a --contains <feature-branch>` on `develop`/`origin/develop`, or that the PR is
   closed/merged). If **not merged, stop** and explain why.
2. Create `docs/features/archive/` if it does not exist.
3. Move the file: `git mv docs/features/<file> docs/features/archive/<file>`.
4. Confirm the Phase 1 selection scan now skips the archived feature.
5. Report the archived feature and suggest updating sprint tracking.

**Before archiving, check whether this feature was the one a sequencing caveat named.** If the sprint
index records that some feature makes `develop` unsafe until it lands, and *this* is that feature,
confirm the gap is genuinely closed before archiving — the evidence is merged tests covering the
relevant rule, not the feature file's ticked checkboxes. Read the caveat from the sprint file; it is
not restated here, where it would drift.
