# Chess rules and move generation

**Status:** proposed rules and correctness contract for standard chess.

## Position representation

Use twelve piece bitboards (`WhitePawns` through `BlackKing`) and cached occupancy bitboards for White, Black, and both sides. A square is an integer from 0 to 63 with `a1 = 0`, `h1 = 7`, `a8 = 56`, and `h8 = 63`. Convert to and from algebraic notation only at parsing/output boundaries.

The position must also track side to move, four castling rights, the en-passant target square (or none), the halfmove clock, the fullmove number, a Zobrist hash, and the reversible-move history needed for repetition detection. The FEN string alone does not carry repetition history; the `position ... moves ...` sequence does.

## FEN

Parse and format all six standard FEN fields:

1. Piece placement.
2. Side to move (`w`/`b`).
3. Castling availability (`KQkq` subset or `-`).
4. En-passant target square or `-`.
5. Halfmove clock.
6. Fullmove number.

Reject invalid field counts, invalid symbols, malformed ranks, impossible square names, invalid counters, and positions that violate basic structural invariants. Error messages should identify the bad field where practical. Do not silently normalize invalid input or replace it with the start position. Define a clear distinction between structural FEN validation and full reachability validation; avoid rejecting legal composed positions solely because they are unusual.

Formatting should be deterministic and round-trip valid positions. Castling rights should be emitted in `KQkq` order, and absent en-passant/castling values as `-`.

## Move representation and generation

A move records source square, destination square, optional promotion piece, and any required special-move flags. Provide deterministic move formatting in UCI coordinate notation. Generate all four promotion choices (queen, rook, bishop, knight), including underpromotions.

Start with pseudo-legal move generation followed by make/check/unmake filtering for legality. Keep the attack test independent from legal-move generation to avoid recursion and to support king-safety rules. Optimize to direct pin/check-aware legal generation only after perft is correct and profiling shows a need.

Generate:

- Pawn pushes (single and initial double push), diagonal captures, and promotions.
- Knight, bishop, rook, queen, and king moves.
- Castling candidates when the relevant rights and rook/king placement permit them.
- En-passant captures when the target square and capturing pawn are valid.

Legal filtering must ensure the moving side's king is not attacked after the move. Attack detection must account for attacks by pinned enemy pieces according to the rule for whether a square is attacked; it must not rely on the opponent having a legal move to that square.

## Special-move rules

### Check and king safety

The king may not move to an attacked square. A move that leaves its own king in check is illegal. When in check, only moves that remove the check are legal; double check can only be answered by a king move.

### Castling

Castling is legal only while the matching right remains, the king and rook are on their required home squares, all intervening squares are empty, and the king is not in check or crossing/landing on an attacked square. A rook's path may cross an attacked square; only the king's start, transit, and destination squares have the attacked-square restriction.

Clear castling rights when the king moves, the corresponding rook moves from its home square, or that rook is captured on its home square. Record prior rights in UndoState so unmake restores them exactly.

### En passant

Set the en-passant target after a two-square pawn advance. Permit a capture only on the immediately following ply. Remove the captured pawn from its actual square, not the destination. After applying the move, check king safety: en-passant can uncover a rook/bishop line and therefore be illegal even when the pawn's destination appears safe.

### Promotion

On reaching the final rank, a pawn must promote to queen, rook, bishop, or knight. Promotion may also capture an opposing piece. UCI move parsing must preserve the promotion choice and validate the resulting legal move.

## Make/unmake contract

For each move, update piece bitboards, cached occupancies, side to move, castling rights, en-passant square, clocks, repetition state, and Zobrist hash consistently. Capture the old values in `UndoState`; do not attempt to reconstruct them from the new board.

`UnmakeMove` must restore every meaningful field exactly. Test this for ordinary moves, captures, both castles, en-passant, promotions, and promotion captures. In debug/test builds, recomputing occupancy and hash from scratch is a useful invariant check.

## Draw handling

Maintain position keys across the game/search path to detect repetition. Repetition identity must distinguish an en-passant right only when a legal en-passant capture is available; if the transposition-table key conservatively distinguishes additional en-passant states, maintain a normalized repetition key or compare legal-move availability for repetition checks. Recognize the halfmove clock and repetition history during search. Treat claimable threefold repetition and the fifty-move rule as draws for engine search when the position qualifies, while documenting that a UCI engine does not itself execute a player's formal claim. A twofold occurrence within the active search path may be used as a cycle-avoidance heuristic, but it is distinct from game-history draw adjudication and its path-dependent score must not be stored in the transposition table. Transposition-table score reuse must account for the halfmove-clock context because it affects fifty-move draw scores even though it is not part of the base position key.

Resolve terminal states in this order:

1. If the side to move is in check and has no legal move, score checkmate. Checkmate takes precedence even when a draw threshold is also reached, including the 75-move or fivefold automatic-draw thresholds.
2. Apply recognized draw conditions: claimable threefold repetition and the fifty-move rule (which this engine treats as draws in search), automatic fivefold repetition, the automatic 75-move rule, and insufficient material. The automatic thresholds are five occurrences of the same position and 150 halfmoves without a pawn move or capture. Under FIDE Laws of Chess Articles 9.6.1–9.6.2, a checkmating move takes precedence over these automatic draws.
3. If no draw condition applies and the side to move has no legal move while not in check, score stalemate.
4. Otherwise, search continues normally.

This order makes the result deterministic when a draw threshold coincides with checkmate or stalemate: checkmate wins precedence; otherwise a qualifying draw is scored before classifying a no-legal-move position as stalemate.

If automatic seventy-five-move or fivefold rules are not implemented yet, do not claim support for them. Insufficient-material handling should be explicit and covered by tests, not inferred from a simplistic material total.

## Perft correctness gates

Perft recursively counts legal leaf nodes without search pruning or evaluation. Compare exact node totals at each depth; report divide output by root move to isolate errors.

| Position | Depth | Expected nodes |
|---|---:|---:|
| Initial position | 1 | 20 |
| Initial position | 2 | 400 |
| Initial position | 3 | 8,902 |
| Initial position | 4 | 197,281 |
| Kiwipete: `r3k2r/p1ppqpb1/bn2pnp1/2pP4/1p2P3/2N2N2/PPQBBPPP/R3K2R w KQkq - 0 1` | 1 | 48 |
| Kiwipete (same FEN) | 2 | 2,039 |
| Kiwipete (same FEN) | 3 | 97,862 |

Do not proceed to search-strength work until the initial-position and special-move perft suite passes. Expand perft coverage for castling, promotions, en-passant pins, checks, and discovered attacks as soon as the corresponding rule is implemented. See [testing and roadmap](testing-and-roadmap.md).
