namespace EngineDemo.Core.Board;

/// <summary>
/// The previous position metadata needed to restore a position after making a move.
/// </summary>
public readonly record struct UndoState
{
    private const CastlingRights AllCastlingRights =
        CastlingRights.WhiteKingSide
        | CastlingRights.WhiteQueenSide
        | CastlingRights.BlackKingSide
        | CastlingRights.BlackQueenSide;

    public UndoState(
        Piece? capturedPiece,
        Square? capturedSquare,
        Color previousSideToMove,
        CastlingRights previousCastlingRights,
        Square? previousEnPassantTarget,
        int previousHalfmoveClock,
        int previousFullmoveNumber,
        ulong previousHash)
    {
        if (capturedPiece.HasValue != capturedSquare.HasValue)
        {
            throw new ArgumentException(
                "Captured piece and captured square must either both be present or both be absent.");
        }

        if (capturedSquare.HasValue && !capturedSquare.Value.IsValid)
        {
            throw new ArgumentException("Captured square must be valid.", nameof(capturedSquare));
        }

        if (previousSideToMove is not Color.White and not Color.Black)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousSideToMove),
                previousSideToMove,
                "Side to move must be White or Black.");
        }

        if ((previousCastlingRights & ~AllCastlingRights) != CastlingRights.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousCastlingRights),
                previousCastlingRights,
                "Castling rights contain unsupported flags.");
        }

        if (previousEnPassantTarget.HasValue && !previousEnPassantTarget.Value.IsValid)
        {
            throw new ArgumentException("En-passant target must be a valid square.", nameof(previousEnPassantTarget));
        }

        if (previousHalfmoveClock < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousHalfmoveClock),
                previousHalfmoveClock,
                "Halfmove clock cannot be negative.");
        }

        if (previousFullmoveNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousFullmoveNumber),
                previousFullmoveNumber,
                "Fullmove number must be at least one.");
        }

        CapturedPiece = capturedPiece;
        CapturedSquare = capturedSquare;
        PreviousSideToMove = previousSideToMove;
        PreviousCastlingRights = previousCastlingRights;
        PreviousEnPassantTarget = previousEnPassantTarget;
        PreviousHalfmoveClock = previousHalfmoveClock;
        PreviousFullmoveNumber = previousFullmoveNumber;
        PreviousHash = previousHash;
    }

    public Piece? CapturedPiece { get; }

    public Square? CapturedSquare { get; }

    public Color PreviousSideToMove { get; }

    public CastlingRights PreviousCastlingRights { get; }

    public Square? PreviousEnPassantTarget { get; }

    public int PreviousHalfmoveClock { get; }

    public int PreviousFullmoveNumber { get; }

    public ulong PreviousHash { get; }

    public bool IsValid =>
        CapturedPiece.HasValue == CapturedSquare.HasValue
        && (!CapturedSquare.HasValue || CapturedSquare.Value.IsValid)
        && PreviousSideToMove is Color.White or Color.Black
        && (PreviousCastlingRights & ~AllCastlingRights) == CastlingRights.None
        && (!PreviousEnPassantTarget.HasValue || PreviousEnPassantTarget.Value.IsValid)
        && PreviousHalfmoveClock >= 0
        && PreviousFullmoveNumber >= 1;
}
