# GitNexus work skill

This directory is a Copilot Agent Skill. Invoke `/gitnexus-work [plan path]` in Copilot chat or ask Copilot to use the `gitnexus-work` skill. The complete execution contract is in [SKILL.md](./SKILL.md).

The skill executes a `gitnexus-plan` deliverable (or a small bounded task), verifies evidence, checks GitNexus impact before edits, runs the plan's tests, and gates commits with change detection. It edits code and may create commits, so review its proposed scope and follow the user's authorization for consequential actions.

Supporting material:

- `references/evidence-provenance.md` — shared evidence-provenance contract.
- `scripts/evidence-provenance.mjs` — plan evidence reader and verifier.

Keep the entire skill directory intact.
