# GitNexus Engineering Plan

> Task: Implement deterministic six-field FEN serialization for validated chess positions.
> Evidence verified at commit `7cd2c0cb6acc7fbae9af1e4b386986e3f0176073`; GitNexus index refreshed this session with `node .gitnexus/run.cjs analyze --index-only` (CLI 1.6.11). Windows FTS is unavailable, so source search is weighted higher.
> Evidence provenance schema 2; global dirty digest `0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd`; cited-path manifest 12 sorted entries; exact generated plan path excluded.

## Objective (§1)

Add a canonical, deterministic serializer for all valid `Position` values, with FEN parse/serialize/parse preserving position equality and hash.

## Current Behaviour (§2–3)

- [verified] Feature 09 requires six standard FEN fields, canonical `KQkq` castling order, `-` for absent castling/en-passant state, and an exact initial-position output (`docs/features/09-fen-serialization.md`).
- [verified] `FenParser.Parse` exists in the Board namespace (`src/EngineDemo.Core/Board/FenParser.cs:7`); `Position` exposes piece bitboards and all five metadata values (`Position.cs:129-139`).
- [verified] Existing helpers provide standard piece symbols (`Piece.cs:52`) and lowercase square coordinates (`Square.cs:58`); the standard initial position factory is available (`PositionFactory.cs:11`).
- [verified] No serializer implementation exists under `src/`; the baseline focused test filter succeeds but currently finds no serializer tests.

## Findings (§4–5)

- [graph] GitNexus impact on `Piece.ToFenSymbol` (downstream, depth 1) reported one direct dependent, `PieceTests`, with low impact; the method is reused, not modified.
- [inferred] A sibling static `FenSerializer.Serialize(Position)` is the narrowest complement to `FenParser.Parse`; no `Position` model or parser changes are needed.
- [verified] The compact feature does not require a PDG slice; no statement-level control/data-flow claims are made.

## Proposed Changes (§6)

- Add `src/EngineDemo.Core/Board/FenSerializer.cs`: serialize rank 8 through rank 1 and file a through h; compress each consecutive empty run; use existing piece bitboards and `Piece.ToFenSymbol`; append side, canonical castling rights, en-passant coordinate or `-`, and invariant-culture counters. Emit exactly six fields separated by one ASCII space, with no trailing whitespace. Reject null with `ArgumentNullException`.
- Add `src/tests/EngineDemo.Core.Tests/Board/FenSerializerTests.cs` for exact output, metadata combinations, determinism, and parse/serialize/parse equality and hash.
- Update `docs/features/09-fen-serialization.md` and the Feature 09 row in `docs/features/00-mission-01-sprint.md` after acceptance criteria pass.

## Implementation Sequence (§7)

1. Add focused serializer tests first; run the `FenSerializer` filter and confirm the expected RED failure because the API is absent.
2. Implement the smallest serializer using the existing board/value helpers; preserve `Position`, `FenParser`, and `Piece` behavior.
3. Run focused tests, build, and the full test suite; fix any issues caused by this change.
4. Mark Feature 09 complete and update the sprint table only after verification.

## Test Strategy (§8)

- Initial position → serialize → exact required initial FEN.
- Sparse/asymmetric valid position → serialize → correct rank/file orientation, piece case, and empty-run compression.
- Castling-right permutations/subsets, both sides to move, present/absent en-passant targets, and non-default counters → serialize → canonical six-field output.
- Each test position → serialize then parse → equal position and identical hash; repeat serialization → identical string.
- Verify null rejection and counter formatting are deterministic.
- Commands: `dotnet test src\EngineDemo.slnx --no-restore --filter FullyQualifiedName~FenSerializer`; `dotnet build src\EngineDemo.slnx --no-restore`; `dotnet test src\EngineDemo.slnx --no-restore`.

## Implementation Context (§11)

```json
{
  "implementation_context": {
    "task_summary": "Implement deterministic standard six-field FEN serialization for validated Position values.",
    "acceptance_criteria": [
      "Emit eight ranks in standard order, compress consecutive empty squares, and use standard piece symbols.",
      "Emit side to move, canonical KQkq castling rights or -, en-passant target or -, halfmove clock, and fullmove number.",
      "Produce deterministic output with exactly six fields and serialize the initial position to the required literal.",
      "Parse/serialize/parse preserves position equality and hash."
    ],
    "evidence_provenance": {
      "schema_version": 2,
      "head_commit": "7cd2c0cb6acc7fbae9af1e4b386986e3f0176073",
      "generated_plan_path": "docs/plans/2026-11-27-gitnexus-plan-fen-serialization-canonical-output.md",
      "global_dirty_digest": {
        "algorithm": "sha256",
        "canonicalization": "gitnexus-evidence-provenance-v2 NUL-framed UTF-8 records",
        "value": "0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd"
      },
      "cited_path_manifest": [
        {
          "path": "docs/features/00-mission-01-sprint.md",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:05a74ca838fcd9f614b6228274bd0b0681f9986fda56a6c4bbeff44b122e62f1",
          "index_digest": "sha256:05a74ca838fcd9f614b6228274bd0b0681f9986fda56a6c4bbeff44b122e62f1",
          "worktree_digest": "sha256:87a50f850ad457c53643beeec91ea3bc0a9a12f942515bdceec493d777ac9c64",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/features/09-fen-serialization.md",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:6bec19e3c05f73d7be1690b39cd29716b7eb28ecae6a1fca15e69768cc1b8364",
          "index_digest": "sha256:6bec19e3c05f73d7be1690b39cd29716b7eb28ecae6a1fca15e69768cc1b8364",
          "worktree_digest": "sha256:9480c57d114f7b6a7526f5728773103b2617509304130e63e53e97b1a745d743",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Bitboard.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:82362fb48930d58174005b267ce6d2fbf4cac3580625101f83365b98698cec8e",
          "index_digest": "sha256:82362fb48930d58174005b267ce6d2fbf4cac3580625101f83365b98698cec8e",
          "worktree_digest": "sha256:c87a0ef23de1a51ca86ede48874810f01deb445eca1173d4f603d58656696487",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/FenParser.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:f65991b2d617dc296db11c8fb83f60d716c2bf1e2e3b93c10d7e173730e4f458",
          "index_digest": "sha256:f65991b2d617dc296db11c8fb83f60d716c2bf1e2e3b93c10d7e173730e4f458",
          "worktree_digest": "sha256:68a34c928dbe6a7c8d19229802719c83d2b12e362f7915d87a0fe858ad72892c",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Piece.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:808a6926123cd55b8026587f854fbb736ece30e5edb95338e7a1c6d98c023e6d",
          "index_digest": "sha256:808a6926123cd55b8026587f854fbb736ece30e5edb95338e7a1c6d98c023e6d",
          "worktree_digest": "sha256:cfe5ae1127c2bfa1a1c73fade858bf0b028a9ca06a1d4171ba4b1ad4391c0dfc",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Position.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:c995ce93c964f1e7623f11a18adb19cb1cd117c501e02df9152b293be189d422",
          "index_digest": "sha256:c995ce93c964f1e7623f11a18adb19cb1cd117c501e02df9152b293be189d422",
          "worktree_digest": "sha256:05eb5bec3791e412e9b7c71d854196879ac659b2c4b0c5104e318e515b19806c",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/PositionFactory.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:1f285d869e80de1159070faaf8b490eb030f74714f905147709c7e8b290b9ff4",
          "index_digest": "sha256:1f285d869e80de1159070faaf8b490eb030f74714f905147709c7e8b290b9ff4",
          "worktree_digest": "sha256:63c1cf04cb65244101ea0ac59f359e39d1a53de0e7d23edd66d0d3860ebbf0d6",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/Board/Square.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:9a986bd2bc3b850563338d344d86daf5e819e5ee8466301333ff5f77b9ff2233",
          "index_digest": "sha256:9a986bd2bc3b850563338d344d86daf5e819e5ee8466301333ff5f77b9ff2233",
          "worktree_digest": "sha256:354e8cebcc50521aa739fee6f4654c8aaf7164e70af8a565f408d75719fec350",
          "untracked_digest": "absent"
        },
        {
          "path": "src/EngineDemo.Core/EngineDemo.Core.csproj",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:775a0ad6fad00a38cc6022949a15062f1eca40f9556b812f198703425f7bdbfd",
          "index_digest": "sha256:775a0ad6fad00a38cc6022949a15062f1eca40f9556b812f198703425f7bdbfd",
          "worktree_digest": "sha256:00ef3eb1842a728ed7f20158f6ddf69ce89d9ab1e167e6a26c1c964053229968",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/FenParserTests.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:6144c68b12d5236d495dd33da64d2aed30f41db11e2a815640d85a46424b1df7",
          "index_digest": "sha256:6144c68b12d5236d495dd33da64d2aed30f41db11e2a815640d85a46424b1df7",
          "worktree_digest": "sha256:bb1f97aada8fb01523c88401356b40792386f399a432b788db368252b6f8665d",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/PieceTests.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:5254ed9415e79e17120e78f47a935a3a0540cfeb6d5fafc3e04a34f01a6e11f7",
          "index_digest": "sha256:5254ed9415e79e17120e78f47a935a3a0540cfeb6d5fafc3e04a34f01a6e11f7",
          "worktree_digest": "sha256:d922ee1196dbff18f01bd1d09042e4ea5aef48354bcec559e56bd300bd4fafbd",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/EngineDemo.Core.Tests.csproj",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:3f1f43af2a3fbde2b4243d44b9f283bcfe358c3e64b8cb918e035034c8cc10ae",
          "index_digest": "sha256:3f1f43af2a3fbde2b4243d44b9f283bcfe358c3e64b8cb918e035034c8cc10ae",
          "worktree_digest": "sha256:b4373401ac441b7d256064560a86fd5bcb76cccc96b2d33b3ec5fb69f54fb69d",
          "untracked_digest": "absent"
        }
      ]
    },
    "files_to_modify": [
      {"file": "src/EngineDemo.Core/Board/FenSerializer.cs", "symbols": ["FenSerializer.Serialize"], "intended_change": "Add static deterministic serialization for validated Position state."},
      {"file": "src/tests/EngineDemo.Core.Tests/Board/FenSerializerTests.cs", "symbols": ["FenSerializerTests"], "intended_change": "Add exact-format, metadata, deterministic, and round-trip/hash tests."},
      {"file": "docs/features/09-fen-serialization.md", "symbols": [], "intended_change": "Mark the feature complete and record the implemented behavior/verification."},
      {"file": "docs/features/00-mission-01-sprint.md", "symbols": [], "intended_change": "Update the Feature 09 roadmap row after the feature passes."}
    ],
    "tests": [
      {"file": "src/tests/EngineDemo.Core.Tests/Board/FenSerializerTests.cs", "scenarios": [
        "Standard initial position -> serialize -> exact required initial FEN.",
        "Sparse asymmetric valid board -> serialize -> correct orientation, symbols, and empty-run compression.",
        "Castling-right permutations/subsets, side-to-move values, optional en-passant target, and counters -> serialize -> canonical six fields.",
        "Representative positions -> serialize then FenParser.Parse -> equal Position and identical hash; repeated serialization -> same string.",
        "Null input -> serialize -> ArgumentNullException; non-default culture -> numeric fields remain invariant."
      ]}
    ],
    "verification_commands": [
      "dotnet test src\\EngineDemo.slnx --no-restore --filter FullyQualifiedName~FenSerializer",
      "dotnet build src\\EngineDemo.slnx --no-restore",
      "dotnet test src\\EngineDemo.slnx --no-restore"
    ],
    "assumptions": [
      "Use the static public API FenSerializer.Serialize(Position); verify naming against local Board conventions before implementation.",
      "A null reference is rejected with ArgumentNullException; Position instances are otherwise valid by construction.",
      "Use invariant-culture decimal formatting to uphold deterministic standard FEN output."
    ],
    "open_questions": [],
    "avoid": [
      "Do not modify Position, FenParser, Piece, Square, or existing FEN symbol/coordinate mappings.",
      "Do not expand scope into UCI, move generation, position validation, or unrelated FEN parsing changes.",
      "Do not repeat full repository discovery; re-verify only assumptions and cited evidence that drifted."
    ]
  }
}
```

## Assumptions and Open Questions (§12)

- [assumed] The public API is `FenSerializer.Serialize(Position)`; confirm the name matches local Board conventions during implementation.
- [assumed] Null input should fail explicitly with `ArgumentNullException`; all non-null `Position` values are already validated.
- [assumed] Invariant-culture numeric formatting is required for deterministic standard FEN output. Open questions: none.

## Definition of Done (§13)

- The exact initial-position literal and all six-field/canonicalization criteria pass focused tests.
- Round-trips preserve `Position` equality and hash; focused tests, build, and full test suite pass.
- Feature 09 and the Sprint 1 roadmap status reflect the verified implementation.