---
name: implement
description: Implement a feature with TDD (branch → RED → GREEN → verify)
disable-model-invocation: true
---

You are executing the `/implement` command. Follow the TDD workflow encoded in the `tdd-implement`
agent — `.github/agents/tdd-implement.agent.md` (Phases 1–5 below; Phase 0 of the agent is the one-time
foundation). Use a feature number or file path supplied with the invocation when available.

**Resolve the feature target:**
- No target supplied → run Phase 1 selection: the next feature is the **lowest-numbered** file in
  `docs/features/` — excluding the sprint index, `backlog.md`, and anything under `archive/` — that
  is not already done, archived, or in progress.
- A number → resolve to the matching file (e.g. `06` → `docs/features/06-feature-name.md`).
- A path → use that feature file directly (it must be under `docs/features/`).

**Execute the TDD phases:**

**Phase 1** — Read the feature file completely. Restate its acceptance criteria and its dependencies
on lower-numbered features. Verify the feature is not already in progress (check feature branches and
open pull requests) and not archived.

**Phase 2** — Ensure you are on an up-to-date `develop` (`git checkout develop`; `git pull origin
develop`), then create the branch: `git checkout -b feature/<number>-<name>`.

**Phase 3 (RED)** — Write failing tests for the acceptance criteria, using applicable guidance in
`.github/instructions/testing-and-tdd.instructions.md`. Start any relevant container-backed tier the project has; without it
those tiers skip rather than fail, which is not the same as passing. Run the project's test command
and confirm the new tests fail **for the expected reason**.

**Phase 4 (GREEN)** — Implement the minimum code to make the tests pass. Follow existing code
patterns and conventions. Add comments for complex logic.

**Phase 5 (REFACTOR & VERIFY)** — Run the full suite until all tests pass. Refactor while keeping
tests green. If the schema changed, update the data model in the project's PRD and the feature file.

**When done:** continue straight through Phases 6–7 — commit, push the branch, and open the pull
request against `develop` — **without stopping to ask for approval first**. Report a concise summary
once the PR exists: what was implemented, test results, changed files, and the PR URL. Review happens
*in the pull request*, not before it — the user reads the diff there and leaves comments if anything
is wrong.

**Merging:** a PR may be merged only once **every** review comment on it has been addressed. Never
merge without explicit approval — a PR that is green and carries no comments is still not approval to
merge.
