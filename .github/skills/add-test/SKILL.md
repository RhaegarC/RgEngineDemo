---
name: add-test
description: Add missing TDD tests (RED) for a feature without implementing them
disable-model-invocation: true
---

You are executing the `/add-test` skill (the TDD **RED** step). Use a feature number or path supplied with the invocation to locate `docs/features/<number>-<name>.md`; if none is supplied, ask which feature to use.

1. **Read** the feature's acceptance criteria and its **Tests (TDD)** block.
2. **Inspect** the existing test projects to see which parts of the feature the suite already covers.
   Where the backend is layered, the tests live in the `*.Test` project matching the layer they
   exercise.
3. **Identify gaps** — acceptance-criteria behaviours not yet covered by a test.
4. **Write NEW failing test(s)** for those gaps, following the tiers in
   `.github/instructions/testing-and-tdd.instructions.md`: offline unit tests for rules decided from the request alone
   (validation, policy construction, permission evaluation); a model tier that stays offline and
   inspects generated queries structurally; and the container-tagged tiers that run the real engine
   and the real provider client. **Nothing stands in for the store** — a store behaviour is asserted
   against a real instance or not at all. Those tiers skip, never fail, when nothing answers. Place
   each test in the project that owns its claim.
5. **Run the relevant suite** and confirm the new tests **fail for the expected reason (RED)**.
6. **Report** the tests added and what each asserts — then **STOP**. Do not implement (GREEN is a
   separate step, e.g. via `/implement`).

**Security hot spots get priority when choosing gaps.** The categories are the ones in
`.github/instructions/testing-and-tdd.instructions.md`; read the project's product/security reference to name the concrete ones. Typically
authorization and ownership evaluation, credential issuance, and input validation at the boundary —
the places where an untested path means data could reach the wrong caller. If a feature touches one
of those and the gap list is long, cover the security boundary first.

**Do not put provider-specific behaviour in the unit tier.** Anything that only proves the real
provider implementation — credential generation, resource existence, content-type round-tripping —
belongs in the integration tier, tagged so it can be excluded without credentials.
