# UCI protocol

**Status:** proposed behavior for the future `EngineDemo.Uci` executable.

## Transport and output rules

- Read one UCI command per line from standard input.
- Write only protocol lines to standard output. Flush after handshake responses and search information so a GUI does not wait on buffered output.
- Write diagnostics and optional debug logs to standard error. Never print banners, prompts, stack traces, or build/runtime messages to stdout.
- Keep each output line atomic; do not interleave output from command handling and search callbacks.
- Tokenize by whitespace while preserving the `position fen ...` fields and the remainder of `go` as command arguments. Do not parse commands with fragile fixed token positions where a variable-length section is defined.

## Commands

Implement protocol commands in stages. The first usable version should support the required lifecycle and a minimal but complete `go` path; the broader limit set should be added before claiming compatibility with general-purpose GUIs.

| Command | Required behavior |
|---|---|
| `uci` | Emit `id name RgChess`, `id author ...`, each supported `option` declaration, then `uciok`. Do not start a search. |
| `isready` | Emit `readyok` promptly, including while a search is running. It must not stop or restart search. |
| `ucinewgame` | Stop/join any active search, reset game-position history, and clear the transposition table when implemented. The GUI will send a new `position` afterward. |
| `position startpos [moves ...]` | Build the initial position and replay each move in order. |
| `position fen <six FEN fields> [moves ...]` | Parse the supplied position and replay each following move. Build it transactionally, then replace the current position only after the FEN and every move are valid. Reject malformed FEN or illegal moves explicitly on stderr; do not silently substitute the initial position. |
| `go ...` | Start exactly one search using parsed limits. Emit iterative `info` lines and one final `bestmove`. |
| `stop` | Request cancellation, wait for the active search to return, and emit its best-so-far move exactly once. |
| `setoption name <name> [value <value>]` | Validate known option names and values. Unknown or invalid options must not corrupt state; report diagnostics on stderr. |
| `quit` | Cancel and join search, then exit cleanly. Do not block indefinitely waiting for a search that ignores cancellation. |

Support standard UCI move notation: coordinate source and destination squares, with a lowercase promotion suffix (`q`, `r`, `b`, or `n`). Chess960-specific notation and options are out of scope.

## Search limits and options

The first release should support at least `depth`, `movetime`, and `infinite`. Add clock-based limits (`wtime`, `btime`, `winc`, `binc`, and `movestogo`) before declaring robust GUI clock support. `nodes`, `searchmoves`, and `ponder`/`ponderhit` are useful follow-up support; do not advertise behavior that is not implemented.

Recommended initial option:

| Option | Type | Initial behavior |
|---|---|---|
| `Hash` | spin, megabytes | Set transposition-table capacity within documented safe minimum/maximum bounds; default to a modest fixed size such as 16 MB. A change takes effect without corrupting an active search. |

Advertise only options the engine actually honors. A fixed single search thread does not need a `Threads` option. Add `Clear Hash` only when the table exists and the command works.

Enforce a requested `nodes` limit at node boundaries without intentional overshoot. Check wall-clock deadlines and cancellation at documented bounded intervals; because a deadline can expire between checks, reserve time for protocol/scheduler overhead and test the maximum observed response delay. Do not promise that `movetime` is an exact wall-clock cutoff.

Unknown `go` tokens must not be reinterpreted as valid limits. For malformed or conflicting limits, report the issue to stderr and reject the command; never start an unbounded search or substitute a success-shaped fallback.

## Search lifecycle and concurrency

- Permit at most one active search.
- The command loop remains able to process `isready`, `stop`, and `quit` during search. Use cancellation signaling and a clear completion path; do not block the input reader on the search task.
- Before a new `position`, `ucinewgame`, or replacement `go` mutates search-owned state, cancel and join the previous search. The implementation must define whether replacement commands also emit the prior `bestmove`; never emit duplicate best moves.
- `stop` is idempotent. If no search is active, it has no effect.
- When a finite limit expires, return the best move from the most recently completed iteration. If no iteration completed, return the best legal fallback found before cancellation.
- If the root position has no legal move, emit `bestmove 0000`.
- Ensure every started search reaches one terminal path and that terminal path cannot be bypassed by exceptions or cancellation.
- `quit` cancels and joins active work before process exit. Do not promise `bestmove` after `quit`; the GUI is ending the session.

Search execution should use `CancellationToken` plus a monotonic elapsed-time source. Protocol parsing and output formatting remain outside the search algorithm; see [architecture](architecture.md) and [search and evaluation](search-and-evaluation.md).

## Output

After handshake, a search may report one `info` line per completed depth, for example:

```text
info depth 8 seldepth 12 score cp 34 nodes 12034 time 91 nps 132241 pv e2e4 e7e5
bestmove e2e4 ponder e7e5
```

Fields not yet measured should be omitted rather than fabricated. Scores are from the side-to-move perspective. Use `score mate <plies>` for forced-mate reporting and a documented mate-score convention internally. The `ponder` move is optional and may only be emitted when it is the second move in a valid principal variation.

Do not emit a `bestmove` at each iterative-deepening depth: intermediate results use `info`; exactly one `bestmove` terminates a search.

## Protocol acceptance checks

- `uci` produces `uciok`; `isready` produces `readyok`, including during active search.
- A valid `position startpos` or six-field FEN plus legal move sequence is accepted.
- A malformed position is reported and does not silently become the initial position.
- `go depth N` ends with one legal `bestmove`; `go infinite` remains active until stopped.
- `stop` returns promptly and produces at most one final `bestmove`.
- No diagnostic or non-protocol output appears on stdout.
- Repeated new-game/search/stop/quit cycles terminate without leaked tasks or cross-search output.
