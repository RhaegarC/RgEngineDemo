# Copilot Agent Skills

These workspace skills are discovered from `.github/skills/<name>/SKILL.md` by supported GitHub Copilot clients. Type `/` in Copilot chat to discover slash-invocable skills, or describe a matching task for automatic skill selection. Supporting scripts, references, and templates are kept inside their skill directories.

Skills with `disable-model-invocation: true` are intentionally available only when explicitly selected by the user.

## GitNexus

- [gitnexus-cli](./gitnexus-cli/SKILL.md) — analyze/index a repository, check index status, clean the index, generate a wiki, and list indexed repositories.
- [gitnexus-debugging](./gitnexus-debugging/SKILL.md) — trace bugs and errors through the GitNexus graph.
- [gitnexus-exploring](./gitnexus-exploring/SKILL.md) — understand code structure, relationships, and execution flows.
- [gitnexus-guide](./gitnexus-guide/SKILL.md) — reference GitNexus tools, resources, graph schema, and workflows.
- [gitnexus-impact-analysis](./gitnexus-impact-analysis/SKILL.md) — assess change impact and blast radius.
- [gitnexus-lfg](./gitnexus-lfg/SKILL.md) — orchestrate plan, execution gate, implementation, and review.
- [gitnexus-pdg-query](./gitnexus-pdg-query/SKILL.md) — query or extend the control/data-dependence graph.
- [gitnexus-plan](./gitnexus-plan/SKILL.md) — prepare implementation-ready plans using graph, PDG, and source evidence.
- [gitnexus-refactoring](./gitnexus-refactoring/SKILL.md) — safely rename, extract, split, or move code.
- [gitnexus-review](./gitnexus-review/SKILL.md) — review changes with GitNexus evidence and source verification.
- [gitnexus-taint-analysis](./gitnexus-taint-analysis/SKILL.md) — work with CFG, taint, and PDG analysis.
- [gitnexus-work](./gitnexus-work/SKILL.md) — execute a GitNexus plan with impact checks, tests, and change gates.

## Conversation and learning

- [grill-me](./grill-me/SKILL.md) — explicitly invoke a structured, one-question-at-a-time design interview.
- [grilling](./grilling/SKILL.md) — stress-test a plan or decision through a guided interview.
- [handoff](./handoff/SKILL.md) — create a redacted handoff document for a later session.
- [teach](./teach/SKILL.md) — teach a topic over multiple sessions using a stateful learning workspace.
- [writing-great-skills](./writing-great-skills/SKILL.md) — reference for designing predictable, useful Agent Skills.

## Testing guidance

- [testing-and-tdd](./testing-and-tdd/SKILL.md) — detailed test tiers and TDD practices for backend/API and persistence work; not a database-testing mandate for the chess engine.

## Former slash commands

These Claude Code commands were adapted as explicit-invocation Copilot skills:

- [add-test](./add-test/SKILL.md) — add focused, failing TDD tests without implementing the feature.
- [archive](./archive/SKILL.md) — archive a feature only after its pull request merges.
- [capture](./capture/SKILL.md) — capture a feature specification or bug report.
- [doc-assert](./doc-assert/SKILL.md) — run the repository's documentation-invariant checker.
- [implement](./implement/SKILL.md) — implement a feature using the TDD workflow and pull-request lifecycle.
- [list-features](./list-features/SKILL.md) — report feature status and sequencing.
- [next](./next/SKILL.md) — select and implement the next eligible feature.
- [sprint-status](./sprint-status/SKILL.md) — summarize sprint progress, DoD, branches, and pull requests.

## Portability notes

The GitNexus skills may also refer to GitNexus MCP capabilities; using those workflows requires the relevant GitNexus tools or their documented CLI fallback. Files under `agents/` within individual skills are retained as supporting metadata but are not Copilot custom-agent definitions. The actual workspace custom agents are in `.github/agents/`.
