---
name: Testing and TDD Standard
description: Apply the repository's test-first and test-tier conventions when working on backend or API code.
applyTo: '**/Api/**/*.cs, **/*Repository*.cs, **/*DbContext*.cs'
---

# Testing & TDD Standard

Status: **Draft** (2026-09-18)

A project-independent standard for how tests are written, which tier they belong to, and what the TDD workflow guarantees. It is referenced by the repository's TDD and bug-fix agents. Its detailed tier guidance is scoped to backend/API and persistence code; it does not prescribe database tiers for RgChess's chess engine.

Nothing here names a feature, a project, or a count. Which behavior a suite covers is status, not standard: record it in that project's feature or sprint index. This document says how a test is written and where it runs.

## Scope and project structure

This standard applies to backend/API code. Do not apply its ORM, database-container, external-provider, or host-testing requirements to non-backend components such as the RgChess engine.

Where a backend is layered, each layer carries a sibling test project, and new tests go in the project matching the layer they exercise.

## Test tiers

A test belongs to the tier that owns its claim. A claim decided by input alone (validation, policy construction, pagination, ownership, permissions) runs offline. A claim about what the persistent store actually does runs against the real engine, container-backed where needed. No fake database stands in for either.

| Tier | Claim kind | Tooling | Runs |
|---|---|---|---|
| **Unit** | Input-only rules: validation, policy construction, pagination, ownership/permission evaluation, and reflection assertions | Test framework | Always; fast and offline |
| **Model** | ORM model and generated SQL: keys, column types/lengths, filters, constraints, and relationship shape | Test framework + ORM | Always; offline, no connection |
| **Database** | Real engine behavior: migrations, constraints, filters, defaults, and persisted round-trips | Test framework + engine container | Container-tagged; skips when unreachable |
| **External service** | Real provider-client behavior, such as uploads, credentials, routing, and moves | Test framework + provider SDK | Container-tagged; skips when unreachable |
| **Host** | Real application pipeline and startup behavior without a database dependency | Test framework + host factory | Always; no database |

A recording double may be used only to prove a request was rejected before a store operation. It records calls and does not implement the store contract. State plainly where separate tests do not cover the wiring between service and repository.

Withdraw claims for behavior that does not exist rather than inventing tests that cannot fail. An assertion that only restates framework ordering is not a test of application behavior.

## Container tier

If the configured endpoint cannot be reached, skip the container test. Once connected, any test failure is a real failure; do not convert it to a skip.

In .NET, `Skip.IfNot` requires `[SkippableFact]` or `[SkippableTheory]`, and must run outside exception recorders that would catch the skip. Prefer fixtures that handle skips explicitly over hand-rolled availability branches.

Use one shared database per test run, migrated once to head. Tests isolate by their own rows; do not assert on entire tables. A test that must stop at an intermediate schema needs a unique scratch database, dropped when that test finishes. Keep the shared run database after teardown for diagnosis; the next run replaces it.

Apply migrations with `Migrate()`, never `EnsureCreated()`. `EnsureCreated` bypasses migrations and cannot validate the migration set. Avoid running migrations concurrently from each fixture.

## External services and test safety

Route vendor access through one abstraction so provider types remain isolated. Prefer a local emulator that needs no credentials; use the real service only when deliberately configured. Provision or change access only when the endpoint is positively verified as loopback. Tests must never silently change access settings on a real account.

Use test-specific environment variable names, never application variables that may point to a real environment. Keep `.env` ignored and commit only a secret-free `.env.example`.

## TDD discipline

1. **RED:** write a failing test for the intended behavior and confirm the reason.
2. **GREEN:** make the smallest implementation that passes it.
3. **Refactor:** improve the implementation while tests remain green, then rerun the relevant suite.

Test-first hot spots include authorization and ownership, credential issuance, input validation at boundaries, and caller-controlled strings that reach queries, paths, or commands.

When there is no runtime behavior to assert:

- **doc-assertion:** test a durable property of a file and confirm the test fails if the property is removed.
- **verification-only:** record what was run and observed; explain why there is no meaningful test. Do not invent a test that cannot fail.

## Commands and configuration

Document actual repository commands rather than copying placeholders. Keep test dependencies separate from local-development dependencies. Report skipped tests explicitly; a skip is not a pass, and a run with skips must not be described as all tests passing.
