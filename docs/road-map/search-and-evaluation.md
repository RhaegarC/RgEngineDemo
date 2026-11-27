# Search and evaluation

**Status:** proposed search design; implement only after legal move generation passes its correctness gates.

## Search contract

The searcher consumes a valid `Position` and immutable `SearchLimits`; it returns a `SearchResult` containing the best legal root move (if one exists), score, completed depth, principal variation, and useful counters such as nodes and elapsed time. It does not read UCI text, write to the console, or own the game loop.

Search is initially single-threaded and deterministic for a fixed position, limits, and table state. A cancellation token and monotonic deadline are checked regularly, including in quiescence. Cancellation returns the best move from the last fully completed root iteration.

## Staged algorithm

Implement in this order:

1. Legal one-ply root search with a simple material evaluation, validating the search/position boundary.
2. Iterative deepening with negamax and alpha-beta pruning.
3. Move ordering: previous principal-variation move, transposition-table move, winning captures/promotions, killer moves, then history-scored quiet moves.
4. Quiescence search to reduce the horizon effect, initially searching captures and promotions.
5. Zobrist-keyed transposition table with depth, score, bound type (exact/lower/upper), and best move.
6. Principal-variation search and aspiration windows after baseline alpha-beta output is stable.
7. Optional later enhancements such as late-move reductions after test and benchmark evidence.

Keep enhancements individually measurable and easy to disable for comparison. Do not compromise legality or stop responsiveness to gain nominal node speed.

## Scores and terminal positions

Use a documented integer score unit, preferably centipawns, and mate scores near a reserved upper bound. Adjust mate scores by ply when storing/probing the transposition table so the preference for faster wins and slower losses remains stable across root depths.

At each node, detect no-legal-move states: checkmate is a loss for the side to move; stalemate is a draw. Apply the documented repetition, fifty-move, and insufficient-material handling from [chess rules](chess-rules-and-move-generation.md). Terminal rule checks must not be bypassed by a transposition-table cutoff.

Negamax changes the point of view at each ply: a child score is negated and searched with the corresponding negated alpha/beta window. Keep this score convention consistent in evaluation, UCI `info score`, transposition-table bounds, and mate reporting.

## Quiescence

At a quiet node, use static evaluation as the stand-pat score and search tactical continuations (captures and promotions) with alpha-beta bounds. Avoid searching obviously losing captures when a later, tested pruning rule is safe.

When the side to move is in check, stand pat is invalid: search all legal check evasions, not only captures. Bound quiescence depth or otherwise guarantee termination for pathological checking sequences. Count quiescence nodes separately if doing so helps performance diagnosis.

## Transposition table

Use a bounded table sized by the UCI `Hash` option. Store a position key, searched depth, normalized score, bound type, best move, and replacement metadata. A probe may return a score cutoff only when the stored depth and bound justify it; a stored move can still guide ordering when the score is too shallow.

Clear or age the table on `ucinewgame` according to a documented policy. A `Hash` resize must be safe while no search is using the table. Do not treat a hash match as proof of identity without comparing the full key. If a future compact table uses partial keys, document and test collision behavior.

The transposition key must include all state that changes legal continuations, including side to move, castling rights, and en-passant state. Repetition identity has a stricter rule: an en-passant right distinguishes positions only when a legal en-passant capture is available. If the table key retains extra en-passant distinctions for simplicity, use a normalized repetition key or a legal-move comparison for repetition detection.

## Evaluation progression

Start with a transparent, side-to-move-relative evaluation:

- Material values for pawn, knight, bishop, rook, and queen.
- Piece-square terms for development, centralization, and king safety, initially simple and symmetric by color.
- Tempo bonus only if it is consistently applied and measured.

Keep evaluation pure with respect to the position. Add phase interpolation, pawn structure, mobility, king safety, or passed-pawn terms only when there are tests or benchmark positions demonstrating intended behavior. Values are tuning parameters, not guarantees of playing strength.

## Time management

Use monotonic elapsed time (for example `Stopwatch`) rather than wall-clock timestamps. The UCI layer translates `depth`, `nodes`, `movetime`, or side-to-move clock/increment/moves-to-go into `SearchLimits`.

- `depth` and `nodes` are hard limits.
- `movetime` reserves a small fixed safety margin for protocol and scheduler overhead.
- Clock allocation is based on remaining time, increment, and optional moves-to-go; retain a safety reserve rather than spending the full clock in one move.
- `infinite` continues until cancelled.
- Check time and cancellation at bounded intervals; ensure quiescence obeys the same limits.
- On timeout, finish the current safe point and return the most recent completed iteration.

Keep the initial allocation formula simple, documented, and testable with a fake monotonic clock. Tune only after measuring time-control behavior against representative short and long clock scenarios.

## Search reporting

After each completed iterative-deepening iteration, the UCI adapter may report depth, selective depth (if measured), score, nodes, elapsed time, nodes per second, and principal variation. Do not report metrics that were not measured. Search never formats UCI strings itself.

Assess changes using both fixed-position node counts and fixed-time behavior. A faster search is not an improvement if it returns illegal moves, mishandles draws, violates time limits, or regresses tactical/correctness tests.
