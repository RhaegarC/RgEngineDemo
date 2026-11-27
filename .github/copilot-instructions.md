# Copilot instructions

- RgChess is a C#/.NET 10 standard-chess engine project intended to communicate through UCI. The current executable is only a scaffold; treat design documents as proposals, not implemented behavior.
- Keep all source-bearing projects and source files under `src/`, including production code, tests, and optional benchmarks. Keep the solution under `src/` too; do not create root-level source, test, or benchmark directories.
- Use `EngineDemo` as the explicit C# root namespace; keep `RgChess` for product and project naming. Follow the sub-namespace conventions in `docs/road-map/architecture.md`.
- Before changing engine architecture, rules, UCI behavior, search, or testing strategy, read the relevant documents under `docs/road-map/`.
- Keep chess rules and search independent of console I/O. UCI protocol output belongs on stdout; diagnostics belong on stderr.
- Preserve existing project conventions and validate changes with the narrowest relevant build or tests. Do not invent test projects, commands, or runtime capabilities that are not present.
- Use the repository-specific workflows and reference material in `.github/skills/` when they apply. Workspace agents are in `.github/agents/`.
