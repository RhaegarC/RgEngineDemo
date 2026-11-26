---
name: sprint-status
description: Show sprint progress, DoD status, branches, and open pull requests
disable-model-invocation: true
---

Produce a concise sprint status report:

1. **Feature progress** — list every feature in `docs/features/` with status (not started / in
   progress / archived), based on file locations and git state. The sprint file's feature table is
   **the record you are reporting on, not a second copy of it**: if your reading and its `Status`
   column disagree, say so rather than choosing one.
2. **Sprint DoD** — report the Definition of Done items in the sprint index, checked against what is
   actually complete. Read the criteria from that file; do not carry a list of them here, where it
   would drift.
3. **Git state** — current branch, recent commits (`git log --oneline -5`), open `feature/*` and
   `fix/*` branches, and any open pull requests (`gh pr list`).
4. **Next actions** — recommend the next feature to pick up and any blocked DoD items.

Present as a short table plus a two-line "next action" summary.

**Two honest caveats to surface whenever they apply:**

- **A sequencing caveat.** If the sprint file records that some feature makes `develop` unsafe to
  deploy until it lands, and it has not landed, say so in the summary rather than reporting a
  clean-looking sprint.
- **The container tiers, and the difference between a skip and a pass.** The container-backed tiers
  need their dependency running; without it they **skip**, and a skip is not a pass. Report the
  skipped count alongside the passed count, and never describe a run containing skips as "all tests
  passing". If a test count is written down anywhere, it has one home — quote it from there, because
  a number carried in two places goes stale in one of them.
