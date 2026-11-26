---
name: next
description: Select the next feature and implement it (TDD)
disable-model-invocation: true
---

You are executing the `/next` command. This is equivalent to `/implement` with no arguments, using
Phase 1 selection from the `tdd-implement` agent — `.github/agents/tdd-implement.agent.md`.

1. **Select** — determine the next feature: the **lowest-numbered** file in `docs/features/` —
   excluding the sprint index, `backlog.md`, and anything under `archive/` — that is not already
   done, archived, or in progress (check feature branches and open pull requests).
2. If there is **no remaining feature**, report that and suggest pruning or adding to
   `docs/features/backlog.md` — then stop.
3. Otherwise, state which feature you are starting (file, number, priority, dependencies), then
   execute the full `/implement` workflow for it (Phases 1–7: branch → RED → GREEN → verify → commit
   → push → pull request). Do not stop for approval before committing — review happens in the PR.

**Watch for a feature blocked on something that does not exist yet.** A feature can depend on an
artefact outside the repository — an export from a design tool, a credential, an upstream change,
another team's decision. If selection lands on one of those and the artefact is not present, say so
and stop rather than inventing it. Read the dependency from the feature file and the sprint index;
do not carry a list of blocked features here, where it would drift.
