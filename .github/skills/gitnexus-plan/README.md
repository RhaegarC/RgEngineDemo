# GitNexus planning skill

This directory is a Copilot Agent Skill. Invoke `/gitnexus-plan <task>` in Copilot chat or ask Copilot to use the `gitnexus-plan` skill. The full workflow is in [SKILL.md](./SKILL.md).

## What it produces

An implementation-ready plan at `docs/plans/YYYY-MM-DD-gitnexus-plan-<slug>.md`, with a machine-readable implementation context pack. The plan combines GitNexus navigation, optional PDG constraints, and verification against current source.

## Supporting files

- `references/context-ledger.md` — query/read ledger and anti-reread rules.
- `references/context-pack.md` — implementation context pack schema.
- `references/evidence-provenance.md` — evidence byte contract.
- `references/pdg-slice.md` — bounded PDG slice construction.
- `references/plan-template.md` — plan structure.
- `scripts/evidence-provenance.mjs` — versioned plan reader/writer and provenance helper.

Keep the full skill directory intact. The skill requires a GitNexus index for graph-backed planning; when GitNexus is unavailable, follow the documented source-only fallback and state the limitation.
