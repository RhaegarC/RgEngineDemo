# Mission: Sprint 6 — Benchmarking and evidence-based improvements

**Status:** Planned

**Sprint:** 6

**Design references:** [Search and evaluation](../road-map/search-and-evaluation.md), [UCI protocol](../road-map/uci-protocol.md), [Testing and roadmap](../road-map/testing-and-roadmap.md), [Architecture](../road-map/architecture.md)

## Mission

Establish a repeatable performance and compatibility baseline, then evaluate a small set of optional search or evaluation improvements. At the end of this sprint, every retained improvement is justified by focused correctness tests and measurements that can be reproduced.

## Subtasks

- [ ] Add an optional benchmark project under `src/benchmarks/` only if stable search paths make it useful; keep tests independent of it.
- [ ] Define fixed tactical/endgame positions and equivalent fixed-depth, fixed-node, and fixed-time benchmark runs.
- [ ] Specify consistent runtime warmup, repeated runs, and median/variation reporting.
- [ ] Record build/runtime, CPU architecture, commit, positions, limits, hash state/size, nodes, time, and allocation measurements.
- [ ] Separate one-time initialization/reporting allocations from warmed steady-state search allocations.
- [ ] Establish deterministic node-count baselines and explain intentional search-tree changes.
- [ ] Run controlled UCI compatibility checks or a small match against a known engine with reproducible environment, options, positions, and time controls.
- [ ] Run cross-platform measurements on x64 and ARM64 when target hardware is available; report unavailable platforms explicitly.
- [ ] Select and measure at most a small set of roadmap candidates, such as PVS/aspiration windows, SEE, LMR, or evaluation terms, one at a time.
- [ ] Retain only candidates that pass focused and full correctness tests and show repeatable benefit; document rejected/inconclusive candidates and clean-checkout commands.

## Out of scope

- Treating rating estimates or a single match as correctness proof or an Elo guarantee.
- Bundling algorithm changes into baseline measurements or changing correctness tests to favor an optimization.
- Making optional benchmark tooling a prerequisite for ordinary tests.

## Acceptance criteria

- Performance measurements are reproducible and include environment/configuration and variation.
- Search allocation claims distinguish setup/reporting from warmed search; no zero-allocation claim is made without measurement.
- UCI compatibility results contain enough detail to repeat and are not represented as correctness proof.
- Every adopted optimization preserves legal move, draw, score, cancellation, and time-limit behavior.
- Simpler behavior remains the default when an experiment lacks repeatable benefit.
- The solution and full test suite pass independently of optional benchmark tooling; docs give working build, test, UCI, and benchmark commands.
