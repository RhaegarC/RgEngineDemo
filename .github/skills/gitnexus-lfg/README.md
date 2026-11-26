# GitNexus pipeline skill

This directory is a Copilot Agent Skill. Invoke `/gitnexus-lfg <task>` or `/gitnexus-lfg <plan path>` in Copilot chat. The full orchestration workflow is in [SKILL.md](./SKILL.md).

The skill coordinates three other workspace skills:

| Lane | Skill | Purpose |
|---|---|---|
| Plan | `gitnexus-plan` | Produce an evidence-backed plan and request a proceed/stop decision |
| Work | `gitnexus-work` | Execute the approved plan with impact and verification gates |
| Review | `gitnexus-review` | Review the completed changes |

It stops at the plan gate until the user explicitly chooses to proceed. It does not push or open a pull request on its own.
