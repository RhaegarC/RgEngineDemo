---
name: testing-and-tdd
description: Use when designing or running tests for backend/API or persistence code, or following the repository's test-first workflow. Do not apply database/provider tiers to the chess engine.
---

# Testing & TDD Standard

Status: **Draft** (2026-09-18)

A project-independent standard for test tiers and test-first development. Its detailed tier definitions apply to backend/API code, not to the RgChess engine. For engine tests, use the perft, rules, search, and UCI test strategy in `docs/road-map/testing-and-roadmap.md`.

Nothing here names a feature, a project, or a test count. The feature/sprint documents own behavior coverage; this skill says how tests are written and where they run.

## Scope and tier model

Where a backend is layered, each layer carries a sibling test project, and new tests go in the project matching the layer they exercise.

| Tier | Claim kind | Tooling | Runs |
|---|---|---|---|
| **Unit** | Rules decided from input alone: validation, policy construction, pagination, ownership and permission evaluation, reflection assertions | Test framework only | Always; fast and offline |
| **Model** | ORM model and generated SQL: keys, column types/lengths, soft-delete predicates, constraint SQL, and relationship shape | Test framework + ORM | Always; offline, opens no connection |
| **Database** | Real engine behavior: migrations, schema constraints, duplicate keys, filters, structured data round-trips, defaults, and persistence across scopes | Test framework + engine container | Container-tagged; skips when unreachable |
| **External service** | Real provider-client behavior: uploads, content-type round-trips, credentials, routing, and moves | Test framework + provider SDK | Container-tagged; skips when unreachable |
| **Host** | Real application pipeline and startup behavior, including missing configuration, registrations, and anonymous access | Test framework + host factory | Always; deliberately no database |

A test belongs to the tier that owns its claim. Input-only behavior runs offline; claims about a store or provider run against a real instance. A fake implementation of the repository or storage contract is not a test of that contract.

**A test double belongs to no tier.** The one sanctioned use is a purpose-built recording double inside the test, whose only job is to prove that a request was rejected before a store operation. State the service/repository wiring seam when no tier exercises it end-to-end.

Withdraw claims for behavior that does not exist rather than inventing assertions. If a model declares no foreign keys, cascade behavior is not a test tier. An assertion that only restates framework ordering is not a test of application behavior.

`Category!=Container` is not necessarily the offline run if a container-tagged tier can fall back to an emulator or local stand-in. Use one container trait and separate filters for container tiers versus tiers that touch no container.

## Offline model checks

Model tests can compare ORM metadata and inspect generated SQL without opening a connection. They verify what the ORM would send, not that a real database accepts it; keep the database tier for executed migrations and engine behavior.

## Container tier

### Skip only when the endpoint is unreachable

If the configured endpoint cannot be reached, skip; a stopped container is not a code defect. Once connected, any test failure is a real failure and must not be changed into a skip.

In .NET, `Skip.IfNot` requires `[SkippableFact]` or `[SkippableTheory]`. Call it outside exception recorders such as `Record.ExceptionAsync` or `Assert.ThrowsAsync`, which would catch the skip and turn it into a failure. Resolve fixtures before entering the recorder; prefer fixtures that express skipping over ad hoc availability branches.

### Database lifetime and isolation

Every container test uses one database per test run, created once and migrated to head. Each test class receives a scope over it. Isolation is by row: assert only on IDs written by the test, never on a whole table.

Rollback transactions are not a substitute: the same ORM context may return tracked state instead of database state, and a second connection in a transaction may require a distributed transaction coordinator. Row cleanup also assumes hard deletes and becomes fragile when soft deletes or foreign keys are introduced.

A test that must stop at an intermediate migration uses a unique scratch database, dropped when that test finishes. Keep the shared run database after teardown for diagnosis; the next run drops/replaces it before creating its own. Inspect a container's volume behavior before tearing the container down because a disposable container can also destroy the retained database.

### Migrations

Use `Migrate()`, never `EnsureCreated()`. `EnsureCreated` builds from the model and bypasses the migration assembly, allowing broken migrations to go undetected. Apply migrations once per test run, not concurrently from every fixture.

## External services and safety

Route vendor access through one abstraction so provider types remain isolated. Prefer a local emulator that needs no credentials; use the real service only when deliberately configured. Emulator provisioning may change access controls only after positively verifying that the endpoint is loopback. A test must never silently change access settings in a real account.

Test variables must be named separately from application variables, which may point to a real environment. Compose test connection strings from test-only variables. Keep `.env` ignored and commit only a secret-free `.env.example`.

## TDD discipline

1. **RED:** Write a test for the intended behavior and confirm it fails for the right reason.
2. **GREEN:** Make the smallest implementation that passes.
3. **Refactor:** Improve the implementation while green, then rerun the relevant suite.

Test-first hot spots include:

- Authorization and ownership: who can read or mutate, administrative overrides, and anonymous denial.
- Credential issuance: reject unauthorized callers before provider operations; enforce bounded server-side lifetime.
- Boundary validation: allowlists, size limits, and per-owner caps, tested at the boundary.
- Caller-controlled strings reaching a query, path, or command structurally: validate the accepted shape before use.

When there is no runtime behavior to assert:

- **doc-assertion:** test a durable property of a file and confirm the test fails if the property is removed.
- **verification-only:** record what was run and observed, and explain why no meaningful test exists. Do not invent a test that cannot fail.

## Commands and reporting

Document actual repository commands, not placeholders. Keep test dependency configuration separate from local-development configuration. Report skipped tests explicitly; a skip is not a pass, and a run with skips must not be described as "all tests passing."
