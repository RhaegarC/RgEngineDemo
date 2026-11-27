# GitNexus Engineering Plan

> Task: Continue Feature 10 after Sprint 1 completion and implement standard-chess attack masks and attacked-square queries.
> Evidence verified at commit `ac3e55a597704ac2ff65db16603fef3ecd35b1db`. Sprint 1 archival is committed; graph findings are carried forward from the prior refresh at parent commit `1cff7abd81056749d47a0914169e71e2f4b88cb2` and are not load-bearing. Windows FTS is unavailable, so keyword-search results are degraded; source evidence was re-anchored at this commit.
> Evidence provenance schema 2; global dirty digest `0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd`; cited-path manifest 14 sorted entries; exact generated plan path excluded.

## 1. Objective

With Sprint 1 complete and Feature 09 archived, begin the next board-rules increment: pure attack masks for standard pieces and a position-level query for whether a square is attacked.

## 2. Current Behaviour

- [verified] Feature 09 is archived and the Sprint 1 mission is Complete (`docs/features/archive/09-fen-serialization.md`; `docs/features/00-mission-01-sprint.md`, committed at `ac3e55a`).
- [verified] `Position` provides validated piece bitboards and position metadata; `Square` exposes index/file/rank/validity; `Bitboard` provides membership and set-square operations (`Position.cs:67-139`, `Square.cs:19-25`, `Bitboard.cs:11-18`).
- [verified] `Piece` represents piece type and color (`Piece.cs:25-85`). Existing tests establish square-edge/index and bitboard-membership conventions (`SquareTests.cs`, `BitboardTests.cs`).
- [verified] No attack-generation implementation exists under `src/`; the architecture assigns attack masks and attacked-square queries to `AttackGenerator` (`docs/road-map/architecture.md:89`).
- [verified] The rules roadmap says pinned enemy pieces count as attacking and attack detection must not depend on legal-move generation (`docs/road-map/chess-rules-and-move-generation.md:39`).

## 3. Relevant Architecture

- [verified] Board representations already separate coordinates (`Square`), piece/color identity (`Piece`), position data (`Position`), and bitboard operations (`Bitboard`).
- [verified] The architecture places `AttackGenerator` in the board-rules layer; the testing roadmap treats attack detection as a prerequisite to move-generation correctness (`docs/road-map/architecture.md:89`; `docs/road-map/testing-and-roadmap.md:17,52`).
- [inferred] A pure static attack utility can consume these existing representations without changing `Position` or introducing move-generation dependencies. Keep this additive boundary; do not broaden Feature 10 into move generation.

## 4. GitNexus Findings

- [graph] At parent commit `1cff7abd`, `context --uid Record:src/EngineDemo.Core/Board/Square.cs:Square` reported `Index`, `File`, `Rank`, and `IsValid` properties with no incoming/process edges; `context --uid Record:src/EngineDemo.Core/Board/Position.cs:Position` reported `Pieces` and occupancy properties. `context` for `Piece` reported `Color` and `Type`. These record-level graph views guide navigation only; missing edges are not proof of no use.
- [graph] `context` for `Bitboard` exposed `IsSet`, `Set`, `Clear`, `Mask`, `PopCount`, and `EnumerateSquares`. `impact --uid Method:src/EngineDemo.Core/Board/Bitboard.cs:Bitboard.IsSet#2 --depth 3 --include-tests` reported 3 direct dependents: `FenSerializer.GetPieceAt`, `BitboardTests.Operations_RejectDefaultSquare`, and `BitboardTests.SetAndClear_UseSquareIndicesWithoutMutatingInputs`; overall risk LOW. Feature 10 adds a consumer and does not modify `Bitboard.IsSet`.
- [graph] `query --query=attack --context=board --goal=attacks` returned no processes and warned that FTS was unavailable (Windows error 126). No attack-flow or related-process claim is made from that result. The index is stale after documentation changes; these prior graph observations are not load-bearing.
- [inferred] The planned changes add a new implementation and tests while leaving existing API symbols unchanged; therefore the reported `Bitboard.IsSet` dependents are not changed by this feature.

## 5. Statement-Level PDG Findings

- [verified] The central `AttackGenerator` methods do not exist yet, so there is no existing attack-generation function to slice. The prior index refresh included `--pdg`, but no unrelated function was used as a proxy; this plan makes no PDG control/data-dependence claim.
- [inferred] Keep the new attack calculation pure: read piece masks and occupancy, return masks/results, and leave all `Position` fields and hash unchanged.

## 6. Proposed Changes

- [verified] Sprint 1 archival and mission completion are already committed; do not repeat or modify those files in Feature 10 work.
- `docs/features/00-mission-02-board-rules.md`: add the Sprint 2 mission, scope, and ordered roadmap link.
- `docs/features/10-attack-generation.md`: specify mask and query behavior, edge/blocker semantics, pinned-attacker behavior, validation, tests, and exclusions.
- `docs/road-map/testing-and-roadmap.md`: link Sprint 2 and identify attack generation as the next board-rules gate.
- `src/EngineDemo.Core/Board/AttackGenerator.cs` (`AttackGenerator`): add pawn, knight, king, bishop, rook, and queen attack-mask methods plus `IsSquareAttacked(Position, Square, Color)`. Use bounded coordinate offsets for leapers/pawns and occupancy-aware rays for sliders. Include the first occupied square and stop beyond it. Count pinned pieces as geometric attackers; do not depend on legal-move generation or mutate the position.
- `src/tests/EngineDemo.Core.Tests/Board/AttackGeneratorTests.cs` (`AttackGeneratorTests`): test each mask family, edge behavior, blockers, pinned attackers, invalid arguments, and immutability.

## 7. Implementation Sequence

1. Verify the Sprint 1 archive and mission completion at commit `ac3e55a`; this step is precompleted and must not be repeated.
2. Add the Sprint 2 mission and Feature 10 acceptance spec; keep the scope bounded to attack masks and attacked-square queries.
3. Add focused `AttackGenerator` tests and confirm RED because the production API is absent.
4. Implement the smallest attack generator using existing `Square`, `Piece`, `Position`, and `Bitboard` representations; do not modify those existing APIs.
5. Run focused tests, solution build, and full test suite. Create the implementation commit and PR; do not merge without approval.

## 8. Test Strategy

Add `src/tests/EngineDemo.Core.Tests/Board/AttackGeneratorTests.cs` with these concrete cases:

- White and black pawns from center and rank edges -> expected forward diagonals only, with no file wrap.
- Knights and kings from center and corners -> exact in-board destinations.
- Bishop and rook rays with empty paths and occupied squares -> include the first blocker and exclude squares beyond it; queen mask -> bishop union rook mask.
- Each piece family attacking a target -> `true`; an obstructed slider -> `false`; a pinned piece with a geometric attack -> `true`.
- Default square, invalid color, and null `Position` -> explicit argument exceptions; capture position fields/hash before and after each query -> unchanged.

Related existing conventions are in `src/tests/EngineDemo.Core.Tests/Board/BitboardTests.cs` and `SquareTests.cs`. The solution and test project are tracked at `src/EngineDemo.slnx` and `src/tests/EngineDemo.Core.Tests/EngineDemo.Core.Tests.csproj`. Run:

```powershell
dotnet test src\EngineDemo.slnx --no-restore --filter FullyQualifiedName~AttackGenerator
dotnet build src\EngineDemo.slnx --no-restore
dotnet test src\EngineDemo.slnx --no-restore
```

## 9. Risk and Impact Analysis

- [graph] The only impact pass was on the existing `Bitboard.IsSet` dependency used by the planned implementation. Its direct dependents are `FenSerializer.GetPieceAt`, `BitboardTests.Operations_RejectDefaultSquare`, and `BitboardTests.SetAndClear_UseSquareIndicesWithoutMutatingInputs`; none is modified. Do not change `IsSet` in this feature.
- [inferred] This is additive API surface, so compatibility risk is low if existing board types remain unchanged. The main correctness risks are color-dependent pawn orientation, board-edge wrapping, stopping slider rays at the first blocker, and counting pinned attackers.
- [inferred] Coordinate/ray stepping may be slower than lookup tables, but no performance claim or optimization is in scope. Validate correctness first; defer optimization until perft/profiling work exists.
- [verified] The query is specified to be independent of legal-move generation. No concurrency, persistence, migration, or external-service concerns apply.
- [assumed] Argument validation should follow existing board API exception conventions; verify exact exception types against local patterns when implementing.

## 10. Files Expected to Change

Sprint 1 archival is already committed at `ac3e55a`; this table lists the remaining Feature 10 files.

| File | Symbols | Reason |
| ---- | ------- | ------ |
| `docs/features/00-mission-02-board-rules.md` | — | Introduce Sprint 2 |
| `docs/features/10-attack-generation.md` | — | Define Feature 10 behavior and scope |
| `docs/road-map/testing-and-roadmap.md` | — | Link the next mission and attack-generation gate |
| `src/EngineDemo.Core/Board/AttackGenerator.cs` | `AttackGenerator` | Implement attack masks and query |
| `src/tests/EngineDemo.Core.Tests/Board/AttackGeneratorTests.cs` | `AttackGeneratorTests` | Add focused behavior tests |

## 11. Reusable Implementation Context

The full machine-readable context pack follows. `evidence_provenance` is copied from the schema-2 helper snapshot; the exact generated plan path is excluded from its global digest.

```json
{
  "implementation_context": {
    "task_summary": "With Sprint 1 complete and Feature 09 archived, implement the first Feature 10 board-rule primitive: standard-chess attack masks and a pure attacked-square query.",
    "acceptance_criteria": [
      "Expose pawn, knight, king, bishop, rook, and queen attack masks with correct board-edge behavior.",
      "Sliding attacks include the first occupied square and do not continue beyond it; queen attacks equal bishop union rook attacks.",
      "IsSquareAttacked checks all piece families and counts pinned pieces as attackers without depending on legal move generation.",
      "Queries do not mutate Position; invalid square/color and null Position inputs fail explicitly.",
      "Sprint 1 archival is already complete; add a Sprint 2 mission and Feature 10 acceptance spec."
    ],
    "evidence_provenance": {
      "schema_version": 2,
      "head_commit": "ac3e55a597704ac2ff65db16603fef3ecd35b1db",
      "generated_plan_path": "docs/plans/2026-11-27-gitnexus-plan-attack-generation-query-foundation.md",
      "global_dirty_digest": {
        "algorithm": "sha256",
        "canonicalization": "gitnexus-evidence-provenance-v2 NUL-framed UTF-8 records",
        "value": "0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd"
      },
      "cited_path_manifest": [
        {
          "path": "docs/features/00-mission-01-sprint.md",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:3fe0675d6c29f08d0750c2ea1031baf1962f2d44f9388a429fbd72ac1d3678d5",
          "index_digest": "sha256:3fe0675d6c29f08d0750c2ea1031baf1962f2d44f9388a429fbd72ac1d3678d5",
          "worktree_digest": "sha256:d286ad383544eab95a1220b04a9c910d7e64fb73bfe26511d381fc0c4c705759",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/features/archive/09-fen-serialization.md",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:79d87b0ce22d2e47fea2dfc8140461083fa32e2c610e872c6aaa0e9fe84259a1",
          "index_digest": "sha256:79d87b0ce22d2e47fea2dfc8140461083fa32e2c610e872c6aaa0e9fe84259a1",
          "worktree_digest": "sha256:8727f782079fe7aabe9aff1c7897a06274690f4034a71c1b6df63dd77a6813e4",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/road-map/architecture.md",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:ac44a0f9fda7a31b63ad426d161a933064b37ac67902f1c4c5ad8997d0eceddb",
          "index_digest": "sha256:ac44a0f9fda7a31b63ad426d161a933064b37ac67902f1c4c5ad8997d0eceddb",
          "worktree_digest": "sha256:4a7e0fd8b394bc92709a7c9850250554d99f7e9fc27c5f2029793b08eb315125",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/road-map/chess-rules-and-move-generation.md",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:905174a1a16efb3caf7e2ebac81ad4713de28ce16061fb08f95442512476856d",
          "index_digest": "sha256:905174a1a16efb3caf7e2ebac81ad4713de28ce16061fb08f95442512476856d",
          "worktree_digest": "sha256:6468c9efedb61d96b2140c077b259f9474151c1d77069bdb28899699f0f22686",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/road-map/testing-and-roadmap.md",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:915743a467baba36d8f2bade0d90cafffb04044092b56d48843d95592aea91ac",
          "index_digest": "sha256:915743a467baba36d8f2bade0d90cafffb04044092b56d48843d95592aea91ac",
          "worktree_digest": "sha256:1479b37591aec7b1a7f2c200212a3cb5c18e22ecc6e2a63665ab108fe614ca69",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Bitboard.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:82362fb48930d58174005b267ce6d2fbf4cac3580625101f83365b98698cec8e",
          "index_digest": "sha256:82362fb48930d58174005b267ce6d2fbf4cac3580625101f83365b98698cec8e",
          "worktree_digest": "sha256:c87a0ef23de1a51ca86ede48874810f01deb445eca1173d4f603d58656696487",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/FenSerializer.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:682cf33f6d18a7562b561f6c8140ff7cfdb99d2da1872a96dacd1c0f598b3c5a",
          "index_digest": "sha256:682cf33f6d18a7562b561f6c8140ff7cfdb99d2da1872a96dacd1c0f598b3c5a",
          "worktree_digest": "sha256:2b2d7e94d2fb81db8d1f7447abe74e5e3e3e0cf52b5829bfa722ed74019c27cf",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Piece.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:808a6926123cd55b8026587f854fbb736ece30e5edb95338e7a1c6d98c023e6d",
          "index_digest": "sha256:808a6926123cd55b8026587f854fbb736ece30e5edb95338e7a1c6d98c023e6d",
          "worktree_digest": "sha256:cfe5ae1127c2bfa1a1c73fade858bf0b028a9ca06a1d4171ba4b1ad4391c0dfc",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Position.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:c995ce93c964f1e7623f11a18adb19cb1cd117c501e02df9152b293be189d422",
          "index_digest": "sha256:c995ce93c964f1e7623f11a18adb19cb1cd117c501e02df9152b293be189d422",
          "worktree_digest": "sha256:05eb5bec3791e412e9b7c71d854196879ac659b2c4b0c5104e318e515b19806c",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Square.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:9a986bd2bc3b850563338d344d86daf5e819e5ee8466301333ff5f77b9ff2233",
          "index_digest": "sha256:9a986bd2bc3b850563338d344d86daf5e819e5ee8466301333ff5f77b9ff2233",
          "worktree_digest": "sha256:354e8cebcc50521aa739fee6f4654c8aaf7164e70af8a565f408d75719fec350",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.slnx",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:d4f61a9c54472052aea97966b8caf46c7b9742962311c26be40309aad8efc67c",
          "index_digest": "sha256:d4f61a9c54472052aea97966b8caf46c7b9742962311c26be40309aad8efc67c",
          "worktree_digest": "sha256:a010b3749a85d9d09897d980b55866601e4b9e1019fea6baeb28fc01dfd6adc6",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/BitboardTests.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:6c9f11441c29c02e7adfb5585d162e1c8558e6fbca8fb18baa094f43e59232bd",
          "index_digest": "sha256:6c9f11441c29c02e7adfb5585d162e1c8558e6fbca8fb18baa094f43e59232bd",
          "worktree_digest": "sha256:2f6d5eacffddbdf5d1db3fb380832052be869c5a1e29ca76dfeb18c0d6de8c72",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/SquareTests.cs",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:10112362aa2fc8c1c2245b96a0f785df2959c9acc079c7ed888e0f3f077804ed",
          "index_digest": "sha256:10112362aa2fc8c1c2245b96a0f785df2959c9acc079c7ed888e0f3f077804ed",
          "worktree_digest": "sha256:b6a35c5c591663d3808e74fbf0c796f64d0191e02f5086e97b24ec77a68808d4",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/EngineDemo.Core.Tests.csproj",
          "object_kind": {
            "head": "regular",
            "index": "regular",
            "worktree": "regular",
            "untracked": "absent"
          },
          "state": "clean",
          "rename_from": null,
          "rename_to": null,
          "head_digest": "sha256:3f1f43af2a3fbde2b4243d44b9f283bcfe358c3e64b8cb918e035034c8cc10ae",
          "index_digest": "sha256:3f1f43af2a3fbde2b4243d44b9f283bcfe358c3e64b8cb918e035034c8cc10ae",
          "worktree_digest": "sha256:b4373401ac441b7d256064560a86fd5bcb76cccc96b2d33b3ec5fb69f54fb69d",
          "untracked_digest": "absent"
        }
      ]
    },
    "primary_symbols": [
      {
        "symbol": "AttackGenerator",
        "file": "docs/road-map/architecture.md",
        "lines": "89",
        "role": "Documented owner for attack masks and attacked-square queries; implementation is new."
      },
      {
        "symbol": "Position",
        "file": "src/EngineDemo.Core/Board/Position.cs",
        "lines": "67-139",
        "role": "Validated position and piece/occupancy masks consumed by the query."
      },
      {
        "symbol": "Square",
        "file": "src/EngineDemo.Core/Board/Square.cs",
        "lines": "19-25",
        "role": "Board coordinate and validity input for attack masks."
      },
      {
        "symbol": "Piece",
        "file": "src/EngineDemo.Core/Board/Piece.cs",
        "lines": "25-85",
        "role": "Piece type and color representation used to select attack masks."
      },
      {
        "symbol": "Bitboard.IsSet",
        "file": "src/EngineDemo.Core/Board/Bitboard.cs",
        "lines": "11-18",
        "role": "Existing square-membership operation used by masks/position data."
      }
    ],
    "related_symbols": [
      {
        "symbol": "FenSerializer.GetPieceAt",
        "relationship": "CALLS Bitboard.IsSet (graph impact depth 1)",
        "relevance": "Existing consumer; not modified by this feature."
      },
      {
        "symbol": "BitboardTests.Operations_RejectDefaultSquare",
        "relationship": "test-of Bitboard.IsSet",
        "relevance": "Existing direct dependent; not modified."
      },
      {
        "symbol": "BitboardTests.SetAndClear_UseSquareIndicesWithoutMutatingInputs",
        "relationship": "test-of Bitboard.IsSet",
        "relevance": "Existing direct dependent; not modified."
      },
      {
        "symbol": "SquareTests",
        "relationship": "tests Square",
        "relevance": "Existing edge/index test conventions."
      }
    ],
    "execution_path": [
      "Validate the input square and color; reject a null Position for position-level queries.",
      "Compute leaper masks with bounded coordinate offsets and pawn masks with color-dependent diagonal offsets.",
      "Compute bishop and rook rays from the origin using position occupancy; include and stop at the first blocker.",
      "Combine bishop and rook masks for queen attacks.",
      "For IsSquareAttacked, test the target against each attacker-piece family without generating legal moves or mutating Position."
    ],
    "pdg_constraints": [],
    "architectural_patterns": [
      {
        "pattern": "Board data is represented by Square coordinates, Position piece/occupancy masks, and Bitboard operations.",
        "example_location": "src/EngineDemo.Core/Board/Square.cs; src/EngineDemo.Core/Board/Position.cs; src/EngineDemo.Core/Board/Bitboard.cs",
        "usage_guidance": "Compose the new calculation from existing board representations; do not introduce a parallel board model."
      },
      {
        "pattern": "Attack detection is separate from legal-move generation and pinned pieces remain geometric attackers.",
        "example_location": "docs/road-map/chess-rules-and-move-generation.md:39",
        "usage_guidance": "Keep the query independent of move legality and make the pinned-attacker behavior explicit in tests."
      }
    ],
    "files_to_modify": [
      {
        "file": "docs/features/00-mission-02-board-rules.md",
        "symbols": [],
        "intended_change": "Add the Sprint 2 mission and link Feature 10."
      },
      {
        "file": "docs/features/10-attack-generation.md",
        "symbols": [],
        "intended_change": "Define Feature 10 scope, behavior, and acceptance criteria."
      },
      {
        "file": "docs/road-map/testing-and-roadmap.md",
        "symbols": [],
        "intended_change": "Link Sprint 2 and clarify the attack-generation gate."
      },
      {
        "file": "src/EngineDemo.Core/Board/AttackGenerator.cs",
        "symbols": [
          "AttackGenerator"
        ],
        "intended_change": "Provide standard piece attack masks and a pure attacked-square query."
      },
      {
        "file": "src/tests/EngineDemo.Core.Tests/Board/AttackGeneratorTests.cs",
        "symbols": [
          "AttackGeneratorTests"
        ],
        "intended_change": "Add attack mask, blocker, pinned-attacker, and argument-validation tests."
      }
    ],
    "tests": [
      {
        "file": "src/tests/EngineDemo.Core.Tests/Board/AttackGeneratorTests.cs",
        "scenarios": [
          "White/black pawn from center and rank edge -> expected diagonals without file wrap.",
          "Knight/king from center and corner -> exact in-board destinations.",
          "Bishop/rook with empty paths and first blockers; queen -> union of both rays.",
          "Each piece family attacking a target -> true; obstructed slider -> false; pinned geometric attacker -> true.",
          "Default square, invalid Color, or null Position -> explicit argument exception; Position state/hash unchanged."
        ]
      }
    ],
    "verification_commands": [
      "dotnet test src\\EngineDemo.slnx --no-restore --filter FullyQualifiedName~AttackGenerator",
      "dotnet build src\\EngineDemo.slnx --no-restore",
      "dotnet test src\\EngineDemo.slnx --no-restore"
    ],
    "risks": [
      "Coordinate/ray stepping is simple but may be slower than lookup tables; do not claim optimization without benchmarks.",
      "The attacked-square query must count pinned pieces and use the attacking color rather than side-to-move.",
      "Do not mutate Position or alter existing Bitboard, Square, Piece, or Position APIs."
    ],
    "assumptions": [
      "Check that Feature 10 remains limited to attack geometry and attacked-square queries; keep move generation, make/unmake, perft, and UCI out of scope.",
      "Check that slider masks include the first occupied square regardless of color; later move generation filters friendly destinations.",
      "Check that straightforward coordinate/ray stepping is used first; defer lookup-table optimization until perft and profiling justify it.",
      "Check GitNexus keyword-search limitations before relying on search results; source verification remains authoritative.",
      "Check that Sprint 1 completion and Feature 09 archival are already present at HEAD ac3e55a; do not redo them."
    ],
    "open_questions": [],
    "avoid": [
      "Do not implement pseudo-legal or legal move generation, move application/undo, perft, or UCI behavior in Feature 10.",
      "Do not infer attacks from legal-move generation; pinned pieces still count as attackers.",
      "Do not add lookup-table optimization without later perft correctness and profiling evidence.",
      "Do not modify existing board data APIs or unrelated FEN serialization behavior.",
      "Do not repeat full repository discovery; re-verify only assumptions or cited evidence that drifted."
    ]
  }
}
```

## 12. Assumptions and Open Questions

Assumptions:

- [verified] Sprint 1 and Feature 09 archival are complete at commit `ac3e55a` and must not be repeated.
- [assumed] Feature 10 is attack geometry and attacked-square queries only; move representation, pseudo-legal/legal generation, make/unmake, perft, and UCI remain later work.
- [assumed] Slider masks include the first occupied square regardless of color; later move generation filters friendly destinations.
- [assumed] Use straightforward coordinate/ray stepping first; no magic-bitboard tables or performance claims until correctness and profiling evidence justify them.
- [assumed] New methods reject invalid/default squares and invalid colors explicitly; confirm precise exception types against established board API patterns.
- [assumed] The implementation consumes existing board types without changing their APIs.

Open questions: none.

Explicitly deferred: pseudo-legal/legal move generation, move application/undo, perft, UCI changes, lookup-table optimization, and unrelated FEN work.

## 13. Definition of Done

- Every piece attack mask has focused edge/boundary tests; pawn masks are color-correct and do not wrap files.
- Sliding masks include the first blocker and exclude all squares beyond it; queen attacks equal bishop union rook attacks.
- `IsSquareAttacked` covers every piece family, counts pinned attackers, and does not use legal-move generation.
- Invalid arguments fail explicitly; querying leaves `Position` fields and hash unchanged.
- Focused tests, solution build, and full suite pass.
- Sprint 1 archival is precompleted at `ac3e55a`; Sprint 2 and Feature 10 documentation accurately define the remaining scope.