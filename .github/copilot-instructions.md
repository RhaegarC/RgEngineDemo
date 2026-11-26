# Copilot instructions

- RgChess is a C#/.NET 10 standard-chess engine project intended to communicate through UCI. The current executable is only a scaffold; treat design documents as proposals, not implemented behavior.
- Before changing engine architecture, rules, UCI behavior, search, or testing strategy, read the relevant documents under `docs/`.
- Keep chess rules and search independent of console I/O. UCI protocol output belongs on stdout; diagnostics belong on stderr.
- Preserve existing project conventions and validate changes with the narrowest relevant build or tests. Do not invent test projects, commands, or runtime capabilities that are not present.
- Use the repository-specific workflows and reference material in `.github/skills/` when they apply. Workspace agents are in `.github/agents/`.
