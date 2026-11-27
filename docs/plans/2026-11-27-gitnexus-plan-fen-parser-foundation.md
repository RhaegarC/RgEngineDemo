# GitNexus Engineering Plan

> Task: Implement Feature 08, strict six-field FEN parsing into validated positions.
> Evidence verified at commit fcb4d88be7cc851f01e17af84462956275d14e46; GitNexus index refreshed this session (--index-only --pdg), status up-to-date (CLI 1.6.11).
> Evidence provenance schema 2; global dirty digest 0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd; cited-path manifest 12 sorted entries; exact generated plan path excluded.

## Objective (§1)
Implement the Feature 08 acceptance criteria in `docs/features/08-fen-parsing.md`: parse the six standard FEN fields, reject malformed or structurally invalid input with actionable field context, and return a validated `Position` with correct occupancy and deterministic hash.

## Current Behaviour (§2–3)
[verified] No FEN parser exists in `src/`; existing notation primitives are `Piece.FromFenSymbol` and `Square.TryParse` (`Piece.cs:69`, `Square.cs:37`). The existing `Position` constructor accepts `PieceBitboards` and metadata, validates masks/kings/counters, then computes `Hash` (`Position.cs:75-126,149`). `PositionFactory.CreateStandardInitialPosition` provides a canonical comparison position (`PositionFactory.cs:11`).

## Findings (§4–5)
- [verified] Feature requirements are six fields, exact board/rank widths, recognized pieces, exact `w`/`b`, unique `KQkq` or `-`, side-consistent en-passant rank, bounded counters, structural validation, contextual errors, and hash/occupancy (`docs/features/08-fen-parsing.md:19-27`).
- [verified] `Piece.FromFenSymbol` maps all 12 standard symbols and throws `FormatException` for unsupported symbols; `Square.TryParse` accepts lowercase valid coordinates and reports failure without constructing an invalid square (`Piece.cs:69-84`, `Square.cs:37-56`). Existing tests establish these contracts (`PieceTests.cs:20-40`, `SquareTests.cs:12-58`).
- [verified] Position validation rejects overlapping bitboards and missing/duplicate kings (`Position.cs:108-126,149-174`; `PositionTests.cs:45-80`). Construction derives occupancy and computes the position key; no changes to `Position` are required.
- [graph] `context` for `Piece.FromFenSymbol` shows only its two existing `PieceTests` callers; `context` for `Square.TryParse` shows `Square.Parse`, two `SquareTests`, and the `Parse → Square` flow.
- [graph] `impact` for the `Position` constructor reports 16 direct dependents, all tests: five in `PositionHashTests` (`Hash_ChangesForEachCastlingRight`, `Hash_ChangesWhenPiecePlacementChanges`, `Hash_ChangesWhenSideToMoveChanges`, `Hash_DistinguishesEnPassantTargetStateAndSquare`, `Hash_IgnoresMoveCounters`) and eleven in `PositionTests` (`Position_AcceptsValidSparseStateAndDefaultsToNoEnPassantTarget`, `Position_DerivesMixedColorOccupancyFromPieceBitboards`, `Position_EqualityIncludesPiecesAndEveryMetadataField`, `Position_RejectsDuplicateKings`, `Position_RejectsFullmoveNumbersBelowOne`, `Position_RejectsInvalidEnPassantSquare`, `Position_RejectsInvalidSideAndCastlingRights`, `Position_RejectsNegativeHalfmoveClock`, `Position_RejectsOverlappingPieceBitboardsWithUsefulError`, `Position_RepresentsOptionalEnPassantTargetExplicitly`, `Position_RequiresOneKingForEachColor`). No production callers were reported. The parser must not modify this HIGH-risk constructor.
- [inferred] Follow the existing `Parse` convention: expose `FenParser.Parse(string)` returning `Position`, throw `FormatException` with field/structural context, and preserve the original validation exception as inner exception for structural failures. The graph keyword query was degraded because GitNexus FTS could not load; direct context/impact and source reads were used instead.
- [verified] No PDG slice was available: the local CLI exposes no `pdg_query` command and the new parser has no indexed statements. The index was built with `--pdg`; no PDG edges are inferred here.

## Proposed Changes (§6)
Add `src/EngineDemo.Core/Board/FenParser.cs` with the new public parse entry point. Tokenize on whitespace and require six tokens; parse placement by eight FEN ranks and eight squares per rank, reusing `Piece.FromFenSymbol`; parse side, unique castling flags, side-dependent en-passant rank, and decimal counters; construct `PieceBitboards` and delegate structural validation/hash creation to `Position`. Add focused Core tests and update Feature 08/sprint status and verification after all criteria pass.

## Implementation Sequence (§7)
1. Add focused parser tests first for the initial FEN, sparse positions, each metadata field, malformed ranks/symbols/fields, invalid counters/en-passant, missing/duplicate kings, useful error context, occupancy, and key.
2. Implement the parser in a new file only; do not modify `Position`, `Piece`, or `Square`. Run focused tests and refine only within Feature 08 scope.
3. Run build and full suite; update Feature 08 acceptance/status and Sprint 1 status; validate documentation links.

## Test Strategy (§8)
Compare the standard initial FEN against `PositionFactory.CreateStandardInitialPosition`; cover a valid sparse FEN with both side-to-move/en-passant rank conventions and non-default rights/counters; table-test field-count, placement width/count/symbol, side, castling duplicates/invalid symbols, en-passant coordinates/ranks, and counter failures; assert errors name the field; cover missing/duplicate kings and verify parsed occupancy/hash.

## Implementation Context (§11)
```json
{
  "implementation_context": {
    "task_summary": "Implement strict six-field standard-chess FEN parsing into validated Position values.",
    "acceptance_criteria": [
      "Require exactly six whitespace-separated fields.",
      "Require eight placement ranks, each expanding to eight squares, with recognized piece symbols and digits 1-8 only.",
      "Accept only w or b for side to move.",
      "Accept only unique KQkq castling symbols or -.",
      "Accept - or a valid lowercase en-passant coordinate on the side-consistent FEN rank.",
      "Parse a non-negative halfmove clock and fullmove number of at least one.",
      "Enforce Position structural invariants without silent normalization.",
      "Include the failing field or structural condition in errors.",
      "Return correct occupancy and deterministic hash for valid positions."
    ],
    "evidence_provenance": {
      "schema_version": 2,
      "head_commit": "fcb4d88be7cc851f01e17af84462956275d14e46",
      "generated_plan_path": "docs/plans/2026-11-27-gitnexus-plan-fen-parser-foundation.md",
      "global_dirty_digest": {
        "algorithm": "sha256",
        "canonicalization": "gitnexus-evidence-provenance-v2 NUL-framed UTF-8 records",
        "value": "0a9c85780067d9afcd0764f307b60891e3cee927ee11eaeb5ec7826d10fd82cd"
      },
      "cited_path_manifest": [
        {
          "path": "docs/features/08-fen-parsing.md",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:58a047810d985d5f3439929a40ba2bcc753231940d0dd7be89446874cc776322",
          "index_digest": "sha256:58a047810d985d5f3439929a40ba2bcc753231940d0dd7be89446874cc776322",
          "worktree_digest": "sha256:8435ab9e9c5f7420483c9226eb92d1b699dd6b7b7a0a4050721ce021f28e0dbe",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/features/archive/sprint-01/05-position-state-and-validation.md",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:0a409f59a1fc3adac823a6c8631c55e3e4aef080bf24483581dbbb0eb0c76c46",
          "index_digest": "sha256:0a409f59a1fc3adac823a6c8631c55e3e4aef080bf24483581dbbb0eb0c76c46",
          "worktree_digest": "sha256:1d4e0f8e426a311d3967a6fa31abbfab19949f8b70c5a1a1eb5ff0249bd4aaf9",
          "untracked_digest": "absent"
        },
        {
          "path": "docs/features/archive/sprint-01/06-position-hash.md",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:a8abd0f7ad1f2bc80fdd1453575d035cc6cc1a715a01e0e685efaab4e70f0b1a",
          "index_digest": "sha256:a8abd0f7ad1f2bc80fdd1453575d035cc6cc1a715a01e0e685efaab4e70f0b1a",
          "worktree_digest": "sha256:7a7d0bddda01793000d2cd80c33313088f08a0508c4fe7d1c9356ad8c7732c3c",
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
          "path": "src/tests/EngineDemo.Core.Tests/Board/PieceTests.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:5254ed9415e79e17120e78f47a935a3a0540cfeb6d5fafc3e04a34f01a6e11f7",
          "index_digest": "sha256:5254ed9415e79e17120e78f47a935a3a0540cfeb6d5fafc3e04a34f01a6e11f7",
          "worktree_digest": "sha256:d922ee1196dbff18f01bd1d09042e4ea5aef48354bcec559e56bd300bd4fafbd",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/PositionTests.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:4e16da90ff94baf0acf4dedc589389fe672ecc62bd571fbb188ba12d58baad89",
          "index_digest": "sha256:4e16da90ff94baf0acf4dedc589389fe672ecc62bd571fbb188ba12d58baad89",
          "worktree_digest": "sha256:f4f9a0497dea476354aca40de955ac9cb6d23b85365eff09d57c8c0ea5274050",
          "untracked_digest": "absent"
        },
        {
          "path": "src/tests/EngineDemo.Core.Tests/Board/SquareTests.cs",
          "object_kind": {"head": "regular", "index": "regular", "worktree": "regular", "untracked": "absent"},
          "state": "clean", "rename_from": null, "rename_to": null,
          "head_digest": "sha256:10112362aa2fc8c1c2245b96a0f785df2959c9acc079c7ed888e0f3f077804ed",
          "index_digest": "sha256:10112362aa2fc8c1c2245b96a0f785df2959c9acc079c7ed888e0f3f077804ed",
          "worktree_digest": "sha256:b6a35c5c591663d3808e74fbf0c796f64d0191e02f5086e97b24ec77a68808d4",
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
      {"file": "src/EngineDemo.Core/Board/FenParser.cs", "symbols": ["FenParser.Parse (new)"], "intended_change": "Add strict six-field tokenization, placement/metadata parsing, field-context errors, and Position construction."},
      {"file": "src/tests/EngineDemo.Core.Tests/Board/FenParserTests.cs", "symbols": ["FenParserTests (new)"], "intended_change": "Cover valid FENs, all invalid fields, structural errors, occupancy, and hash."},
      {"file": "docs/features/08-fen-parsing.md", "symbols": ["Feature 08 status and acceptance"], "intended_change": "Mark verified criteria complete and record actual test/build results."},
      {"file": "docs/features/00-mission-01-sprint.md", "symbols": ["Feature 08 row"], "intended_change": "Mark Feature 08 Done after verification."}
    ],
    "tests": [
      {"file": "src/tests/EngineDemo.Core.Tests/Board/FenParserTests.cs", "scenarios": ["Initial FEN equals canonical factory position and hash", "Valid sparse FEN preserves pieces and every metadata field", "Whitespace-separated input requires exactly six fields", "Malformed rank count/width/symbol reports piece-placement context", "Invalid side/castling/en-passant/counters report the corresponding field", "Missing or duplicate kings report structural context", "Valid parse derives expected occupancy and deterministic hash"]}
    ],
    "verification_commands": [
      "dotnet test src/EngineDemo.slnx --no-restore --filter FullyQualifiedName~FenParser",
      "dotnet build src/EngineDemo.slnx",
      "dotnet test src/EngineDemo.slnx --no-restore"
    ],
    "assumptions": [
      "Use FormatException with field-specific text for malformed FEN, matching existing Square.Parse and Piece.FromFenSymbol conventions; use ArgumentNullException for null API input.",
      "En-passant validation checks only the side-dependent rank required by Feature 08, not full move reachability or capturable-pawn presence.",
      "Whitespace runs separate fields; exactly six non-empty tokens are required."
    ],
    "open_questions": [],
    "avoid": [
      "Do not modify Position constructor or existing piece/square APIs.",
      "Do not repair, normalize, ignore extra fields, or infer reachability.",
      "Do not parse a following move list or introduce UCI integration in Feature 08."
    ]
  }
}
```

## Assumptions and Open Questions (§12)
- [assumed] `FenParser.Parse` uses `FormatException` with field-specific messages, matching existing parsing helpers; the feature spec does not prescribe a result-wrapper API.
- [verified] En-passant validation is limited to the required side-dependent rank; full reachability and capturable-pawn checks are explicitly out of scope.
- [assumed] Runs of whitespace separate fields; all six tokens must be non-empty.
- Open questions: none.

## Definition of Done (§13)
All nine Feature 08 criteria are covered; focused tests, build, and full test suite pass; actual counts are recorded in the feature and sprint docs; no existing `Position`, `Piece`, or `Square` API is changed.