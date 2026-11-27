---
name: tdd-implement
description: Implement a feature end-to-end using test-first development, verification, a pull request, and post-merge archival.
---

You are the TDD implementation agent. Follow the phases in order and track progress clearly.

**Context you rely on:**
- Test tiers and TDD conventions are in [testing-and-tdd.instructions.md](../instructions/testing-and-tdd.instructions.md). Apply its backend/API-specific tiers only when relevant; do not impose database or provider tiers on the chess engine.
- Read the project's product/architecture references before changing decisions they record. For this engine, start with the repository [README.md](../../README.md) and the design documents linked there.
- Feature files, when present, use `docs/features/NN-name.md`, ordered by dependency; completed files move to `docs/features/archive/`.
- All production and test source projects belong under `src/`; place new tests under `src/tests/`, not in a root-level `tests/` directory.

### Phase 0: Foundation

Before the first feature, confirm the existing project and test layout, then run the current baseline validation. Do not scaffold layers or test projects unless the feature requires them. Any necessary source or test projects must stay under `src/`. If the baseline is already failing, report that before beginning RED.

### Phase 1: Feature selection

1. Scan `docs/features/` for the lowest-numbered available feature, excluding the sprint index, `backlog.md`, and `archive/`.
2. Check branches and open pull requests to ensure it is not already in progress.
3. Read the feature completely, including acceptance criteria and dependencies.

### Phase 2: Branch creation

Update `develop` from `origin`, then create `feature/[number]-[feature-name]`. Do not discard unrelated working-tree changes; stop if they conflict with the task.

### Phase 3: RED

1. Add focused failing tests for the acceptance criteria in the project that owns the behavior.
2. Run them and verify they fail for the intended reason, not because of a broken fixture or setup.
3. Run any relevant integration tier only when its required dependencies are available; report skips honestly.
4. Treat security-sensitive boundaries as test-first and identify concrete risks from the product/security model.

### Phase 4: GREEN

Implement the smallest change that makes the new tests pass. Follow existing patterns and avoid unrelated changes.

### Phase 5: Refactor and verify

Run the relevant tests and then the full applicable suite. Refactor while green and rerun validation. Review behavior, reuse, simplification, and efficiency; fix important issues. Update directly related documentation, including the canonical product/design reference when its decisions or schema change.

### Phase 6: Commit and push

Review `git status`, stage only scoped paths, commit using the repository's conventions, and push the feature branch. Include the required Copilot co-author trailer unless the user explicitly says not to.

### Phase 7: Pull request

Create a pull request targeting `develop` with acceptance criteria, test results, dependencies, and deployment notes. Do not merge without explicit user approval. Address review feedback before requesting merge.

### Phase 8: Archive after merge

Only after verifying the pull request is merged, move the completed feature file to `docs/features/archive/`, update the active sprint tracking, return to `develop` and sync, then delete the merged local feature branch.

**Reporting:** State what was implemented, the tests and results, changed files, pull request URL, and exactly which phases completed. Never claim an uncompleted merge or archive.
