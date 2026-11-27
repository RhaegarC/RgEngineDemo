# Feature 01 — Solution and Core test foundation

**Status:** Planned

**Priority:** 1

**Dependencies:** None

## Summary

Replace the one-project scaffold layout with a .NET solution rooted under `src/`, and establish the Core library, UCI executable, and Core test project. This feature creates project boundaries and build/test entry points only; it does not implement chess behavior.

## User story

As a contributor, I want a buildable solution and a Core test project configured under `src/`, so that later chess features can be implemented and verified without putting source or tests at the repository root.

## Scope

- Create `src/RgChess.slnx`.
- Create `src/RgChess.Core/RgChess.Core.csproj` as a class library.
- Create `src/RgChess.Uci/RgChess.Uci.csproj` as a console executable referencing Core.
- Create `src/tests/RgChess.Core.Tests/RgChess.Core.Tests.csproj`, referencing Core and using xUnit.
- Set `<RootNamespace>EngineDemo</RootNamespace>` explicitly in each project; keep project/assembly names `RgChess.*`.
- Move or replace the existing `src/EngineDemo` scaffold without leaving duplicate entry points or solution files.
- Ensure all source and project files remain below `src/`.

## Acceptance criteria

- [ ] `src/RgChess.slnx` contains Core, UCI, and Core.Tests projects with correct references.
- [ ] Core, UCI, and Core.Tests target .NET 10 and build without warnings or errors.
- [ ] Every project explicitly uses the `EngineDemo` root namespace.
- [ ] `dotnet build src/RgChess.slnx` succeeds from the repository root.
- [ ] `dotnet test src/RgChess.slnx` succeeds and builds the Core test project; no placeholder assertion is added just to create a test count.
- [ ] No application/test source, project, or solution files exist outside `src/`.
- [ ] The UCI executable remains a scaffold; it does not claim to implement UCI commands.

## Tests (TDD) and verification

This is project scaffolding and does not introduce chess behavior. Do not add a placeholder assertion merely to create a test count. Verify the solution build and that the test command completes; the next feature adds the first behavior tests to Core.Tests.

```text
dotnet build src/RgChess.slnx
dotnet test src/RgChess.slnx
```

## Non-goals

- Implementing the UCI protocol, a chess board, move generation, or search.
- Adding a benchmark project before there is a measured hot path.
- Adding root-level test folders or solution files.
