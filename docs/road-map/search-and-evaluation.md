# Search and evaluation

**Status:** proposed search design; implement only after legal move generation passes its correctness gates.

## Runtime and memory architecture (.NET 10 / cross-platform)

Target **.NET 10** and keep the design portable across **x64 and ARM64**. Correctness and a useful baseline come before low-level constraints; treat performance choices as hypotheses to measure, not prerequisites for a correct search.

- **Allocation target:** Aim for zero managed allocations in the warmed, steady-state per-node search path. Measure table/search setup and result/UCI reporting separately, and state what the benchmark includes. Do not block a correct baseline on a zero-allocation result.
- **Bounded per-ply storage:** Reuse bounded move/score buffers and UndoState by ply instead of cloning position state at each node. If using `stackalloc`, allocate a bounded buffer once at the root and slice it by ply; do not make an unbounded allocation at every recursive call. Define a maximum ply and surface buffer exhaustion rather than silently truncating legal moves.
- **Compact value types:** Use compact value types for moves and state where they improve measured hot paths. Inline value buffers are an option, not a requirement; avoid heap-allocated per-piece arrays in the search loop.
- **Portable bit operations:** Prefer `System.Numerics.BitOperations` for bitboard scanning and population counts; verify performance on supported runtimes rather than assuming a specific instruction sequence.
- **Sliding-piece lookups:** Plain Magic Bitboards with flat lookup arrays are a portable baseline. Compare alternatives on target hardware before making a lookup strategy a permanent constraint.
- **Runtime configuration:** Do not mandate Server GC or a PGO setting in project configuration. Measure runtime modes on representative searches before changing deployment defaults.

## Search contract

The searcher consumes a valid `Position` and immutable `SearchLimits`; it returns a `SearchResult` containing the best legal root move (if one exists), score, completed depth, principal variation, and useful counters such as nodes and elapsed time. It does not read UCI text, write to the console, or own the game loop.

- **Legal fallback:** Establish a legal root fallback before iterative deepening. If cancellation or a limit prevents the first iteration from completing, return that fallback; otherwise return the result from the last fully completed iteration. Do not require depth 1 to finish if doing so would violate a requested limit.
- **Single legal move:** Do not terminate search merely because the root has one legal move. Search its continuation under the normal limits; the root may skip unnecessary move-choice work.
- **Determinism and cancellation:** Search is initially single-threaded and deterministic for a fixed position, limits, and table state. A cancellation token and monotonic deadline are checked regularly, including in quiescence. Cancellation returns the best move from the last fully completed root iteration, or the legal fallback if none completed.

## Staged algorithm

Implement in this order:

1. Legal one-ply root search with a simple material evaluation, validating the search/position boundary and fallback behavior.
2. Iterative deepening with negamax and alpha-beta pruning.
3. Move ordering: previous principal-variation move, transposition-table move, winning captures/promotions, killer moves, then history-scored quiet moves. Consider incremental in-place selection swaps when benchmarks show they outperform full-list sorting.
4. Quiescence search to reduce the horizon effect, initially searching captures and promotions. Add Static Exchange Evaluation (SEE) filtering for losing captures only after it has focused correctness tests and measured benefit.
5. Zobrist-keyed transposition table with depth, score, bound type (exact/lower/upper), static evaluation, and best move.
6. Principal-variation search and aspiration windows after baseline alpha-beta output is stable.
7. Optional later enhancements such as late-move reductions after test and benchmark evidence.

Keep enhancements individually measurable and easy to disable for comparison. Do not compromise legality or stop responsiveness to gain nominal node speed.

## Scores and terminal positions

Use a documented integer score unit, preferably centipawns, and mate scores near a reserved upper bound. Adjust mate scores by ply when storing/probing the transposition table so the preference for faster wins and slower losses remains stable across root depths.

At each node, use the terminal-resolution order in [chess rules](chess-rules-and-move-generation.md): checkmate first (including when a draw threshold is also met), then recognized draw conditions, then stalemate, otherwise continue search. Claimable threefold/fifty-move conditions may be treated as draws according to the engine policy; automatic fivefold/75-move rules apply only when implemented. Do not let a transposition-table cutoff bypass terminal or path-dependent draw checks.

- **Search-cycle repetition:** A second occurrence on the active search path may be scored as a draw to break search cycles. This is a search heuristic, distinct from a claimable threefold draw in game history. Do not store path-dependent repetition scores in the transposition table.

Negamax changes the point of view at each ply: a child score is negated and searched with the corresponding negated alpha/beta window. Keep this score convention consistent in evaluation, UCI `info score`, transposition-table bounds, and mate reporting.

## Quiescence

At a quiet node, use static evaluation as the stand-pat score and search tactical continuations (captures and promotions) with alpha-beta bounds. Consider pruning obviously losing captures (`SEE < 0`) only after SEE is verified against focused positions; never apply stand-pat or capture pruning while the side to move is in check.

- **In-check rule:** When the side to move is in check, stand pat is invalid; search all legal check evasions, not only captures.
- **Termination and accounting:** Bound quiescence depth or otherwise guarantee termination for pathological checking sequences. Any maximum-ply fallback must not treat a checked position as a quiet stand-pat node; cover the chosen fallback with tests. Count quiescence nodes separately if doing so helps performance diagnosis.
- **TT boundary:** Document explicitly whether quiescence nodes probe or store transposition-table entries (at `depth <= 0`), and verify bound consistency before enabling TT writes in quiescence.

## Transposition table

Use a bounded table sized to a power of two when that simplifies indexing, with capacity controlled by the UCI `Hash` option. Start with a managed array within documented safe size limits. An entry should carry enough information to validate a probe and guide ordering, including:

- Full 64-bit position key (reduces, but does not eliminate, collision risk)
- Normalized search score
- Best move
- Searched depth
- Bound type (`Exact`, `Lower`, `Upper`)
- Generation age and cached static evaluation if they provide measured value

A probe may return a score cutoff only when the stored depth, bound, and relevant draw-rule context justify it; a stored move can still guide ordering when the score is too shallow. If a compact entry size or cache alignment is pursued, specify the field packing and allocation guarantees explicitly and benchmark it; neither follows automatically from using a managed array. Consider native memory only if a measured requirement exceeds safe managed storage, and then define ownership, resizing, and disposal behavior.

Clear or age the table on `ucinewgame` according to a documented policy. If `setoption name Hash` is received during search, cancel and join that search before resizing; never replace or mutate the table while search can probe it. Do not treat a hash match as proof of identity without comparing the full key.

The base position key must distinguish piece placement, side to move, castling rights, and en-passant state. Feature 06 defines its deterministic Zobrist values using SplitMix64 with the fixed seed `0xC0FFEE123456789A` and a documented table order in [the archived position-hash specification](../features/archive/sprint-01/06-position-hash.md). The 64-bit key is collision-resistant in practice, not collision-proof; always validate the full key and do not treat a match as proof of position identity. Repetition identity has a stricter rule: an en-passant right distinguishes positions only when a legal en-passant capture is available. Use a dedicated normalized repetition key (or legal-move comparison) so positions differing only by an unplayable en-passant square are recognized as identical.

The halfmove clock affects fifty-move draw scores even though it is not part of the base position key. A matching board key alone is insufficient when the clock differs. Permit score cutoffs across different halfmove clocks only when both clocks are provably unable to reach 100 within their respective remaining search horizons, including extensions and bounded quiescence. Near the threshold, require a validated equivalent draw context (a simple baseline may require equal halfmove clocks); otherwise use the TT entry only for move ordering. Do not infer fifty-move-draw provenance solely from a zero score. Repetition history is path-dependent and must not be treated as part of a reusable position score.

## Evaluation progression

Start with a transparent, side-to-move-relative evaluation:

- Material values for pawn, knight, bishop, rook, and queen.
- Piece-square terms for development, centralization, and king safety, initially simple and symmetric by color.
- Tempo bonus only if it is consistently applied and measured.

Keep evaluation pure with respect to the position. Add phase interpolation, pawn structure, mobility, king safety, or passed-pawn terms only when there are tests or benchmark positions demonstrating intended behavior. Values are tuning parameters, not guarantees of playing strength.

## Time management

Use monotonic elapsed time (`Stopwatch.GetTimestamp()`) rather than wall-clock timestamps. The UCI layer translates `depth`, `nodes`, `movetime`, or side-to-move clock/increment/moves-to-go into `SearchLimits`.

- `depth` and `nodes` are hard limits.
- `movetime` reserves a small fixed safety margin for protocol and scheduler overhead.
- Clock allocation is based on remaining time, increment, and optional moves-to-go; retain a safety reserve rather than spending the full clock in one move.
- With clock-based limits (`wtime`/`btime`), the allocator may choose a reduced budget when the root has exactly one legal move, but search must still examine that move's continuation. This adjustment must not change explicit `depth`, `nodes`, or `movetime` limits, and must not cause `infinite` search to terminate before cancellation.
- `infinite` continues until cancelled.
- Enforce a requested node count at node boundaries without intentional overshoot. Check wall-clock deadlines and cancellation at bounded node intervals to amortize timer overhead; document and test the resulting maximum response delay, including quiescence.
- On timeout, finish the current safe point and return the most recent completed iteration, or the legal root fallback if none completed.

Keep the initial allocation formula simple, documented, and testable with an injectable monotonic clock abstraction. Tune only after measuring time-control behavior against representative short and long clock scenarios.

## Search reporting and verification gates

After each completed iterative-deepening iteration, the UCI adapter may report depth, selective depth (if measured), score, nodes, elapsed time, nodes per second, and principal variation. Do not report metrics that were not measured. Search never formats UCI strings itself.

Correctness gates apply before performance comparisons:

1. **Search correctness:** No illegal root moves; correct mate/stalemate and draw handling; stable score perspective and mate distance; valid TT bounds; cancellation returns the last completed result or legal fallback.
2. **Limit compliance:** Depth and node limits are honored; time-limited searches return promptly within the documented polling/safety margin, including quiescence.
3. **Regression coverage:** Keep targeted positions for TT reuse across different halfmove-clock contexts and for search-cycle repetition versus claimable game-history repetition.

After these gates pass, assess performance using:

1. **Allocation measurements:** Warm up the runtime and report steady-state search allocations separately from setup and reporting. Treat zero allocation as a measured target for the hot path, not a substitute for correctness.
2. **Deterministic node counts:** Record fixed-position node counts across tactical and endgame suites to detect unintended tree growth or pruning regressions; explain intentional search-tree changes.
3. **Fixed-time and cross-platform runs:** Measure time-control compliance and throughput on x64 and ARM64 when the target hardware is available.
