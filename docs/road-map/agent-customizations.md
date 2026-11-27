# Copilot agent customizations

Project-wide Copilot resources live under `.github/`, the native workspace location shared by supported Copilot clients. These files were adapted from the repository's Claude Code agents, rules, skills, and commands.

## Layout

| Resource | Location | Use |
|---|---|---|
| Repository instructions | `.github/copilot-instructions.md` | Always-on project context and conventions |
| Targeted instructions | `.github/instructions/*.instructions.md` | File-pattern-specific guidance; testing tiers are limited to backend/API files |
| Custom agents | `.github/agents/*.agent.md` | Select a role such as `bug-fix` or `tdd-implement` in Copilot chat |
| Agent Skills | `.github/skills/<name>/SKILL.md` | Reusable workflows and references, available as slash skills and for automatic selection |

The former Claude `/commands` were converted to on-demand Agent Skills so they work with the Copilot harness and avoid deprecated Local-harness prompt files. Supporting references, templates, and scripts remain within their skill folders.

This migration covers the repository's agents, rules, skills, and commands. Claude-specific hooks, local settings, backups, and runtime state are not Copilot customizations; obsolete project settings, hooks, and temporary helper files were removed rather than migrated.

## Available resources

See the index at [`.github/skills/README.md`](../../.github/skills/README.md) for all migrated skills and command workflows. The skill folders with scripts or reference material must be kept intact when editing or copying a skill.

## Using them

- Select `bug-fix` or `tdd-implement` from the custom-agent picker for the corresponding end-to-end workflow.
- Type `/` in Copilot chat to discover skills, or invoke a skill by name and add task context. Skills marked `disable-model-invocation: true` are intentionally user-invoked only.
- The testing-and-TDD instruction file applies only to matching backend/API or persistence paths. It is not a requirement for the chess-engine core.

Customization files guide behavior but do not replace validation, review, or explicit user approval for consequential actions such as merging.

## References

- [Custom agents in VS Code](https://code.visualstudio.com/docs/agent-customization/custom-agents)
- [Agent Skills in VS Code](https://code.visualstudio.com/docs/agent-customization/agent-skills)
- [Custom instructions in VS Code](https://code.visualstudio.com/docs/agent-customization/custom-instructions)
