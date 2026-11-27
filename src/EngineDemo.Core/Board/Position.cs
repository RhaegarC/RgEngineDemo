using System.Collections.ObjectModel;
using System.Numerics;

namespace EngineDemo.Core.Board;

[Flags]
public enum CastlingRights : byte
{
    None = 0,
    WhiteKingSide = 1,
    WhiteQueenSide = 2,
    BlackKingSide = 4,
    BlackQueenSide = 8
}

public readonly record struct PieceBitboards
{
    public PieceBitboards(
        ulong whitePawns = 0,
        ulong whiteKnights = 0,
        ulong whiteBishops = 0,
        ulong whiteRooks = 0,
        ulong whiteQueens = 0,
        ulong whiteKing = 0,
        ulong blackPawns = 0,
        ulong blackKnights = 0,
        ulong blackBishops = 0,
        ulong blackRooks = 0,
        ulong blackQueens = 0,
        ulong blackKing = 0)
    {
        WhitePawns = whitePawns;
        WhiteKnights = whiteKnights;
        WhiteBishops = whiteBishops;
        WhiteRooks = whiteRooks;
        WhiteQueens = whiteQueens;
        WhiteKing = whiteKing;
        BlackPawns = blackPawns;
        BlackKnights = blackKnights;
        BlackBishops = blackBishops;
        BlackRooks = blackRooks;
        BlackQueens = blackQueens;
        BlackKing = blackKing;
    }

    public ulong WhitePawns { get; }
    public ulong WhiteKnights { get; }
    public ulong WhiteBishops { get; }
    public ulong WhiteRooks { get; }
    public ulong WhiteQueens { get; }
    public ulong WhiteKing { get; }
    public ulong BlackPawns { get; }
    public ulong BlackKnights { get; }
    public ulong BlackBishops { get; }
    public ulong BlackRooks { get; }
    public ulong BlackQueens { get; }
    public ulong BlackKing { get; }

    public ulong WhiteOccupancy =>
        WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;

    public ulong BlackOccupancy =>
        BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;

    public ulong TotalOccupancy => WhiteOccupancy | BlackOccupancy;
}

public sealed class Position : IEquatable<Position>
{
    private const CastlingRights AllCastlingRights =
        CastlingRights.WhiteKingSide
        | CastlingRights.WhiteQueenSide
        | CastlingRights.BlackKingSide
        | CastlingRights.BlackQueenSide;

    private readonly List<ulong> _positionHistory;
    private readonly ReadOnlyCollection<ulong> _positionHistoryView;

    public Position(
        PieceBitboards pieces,
        Color sideToMove = Color.White,
        CastlingRights castlingRights = CastlingRights.None,
        Square? enPassantTarget = null,
        int halfmoveClock = 0,
        int fullmoveNumber = 1)
    {
        if (sideToMove is not Color.White and not Color.Black)
        {
            throw new ArgumentOutOfRangeException(nameof(sideToMove), sideToMove, "Side to move must be White or Black.");
        }

        if ((castlingRights & ~AllCastlingRights) != CastlingRights.None)
        {
            throw new ArgumentOutOfRangeException(nameof(castlingRights), castlingRights, "Castling rights contain unsupported flags.");
        }

        if (halfmoveClock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(halfmoveClock), halfmoveClock, "Halfmove clock cannot be negative.");
        }

        if (fullmoveNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fullmoveNumber), fullmoveNumber, "Fullmove number must be at least one.");
        }

        if (enPassantTarget.HasValue && !enPassantTarget.Value.IsValid)
        {
            throw new ArgumentException("En-passant target must be a valid square.", nameof(enPassantTarget));
        }

        ValidateDisjointPieceBitboards(pieces);

        if (Bitboard.PopCount(pieces.WhiteKing) != 1)
        {
            throw new ArgumentException("Position must contain exactly one white king.", nameof(pieces));
        }

        if (Bitboard.PopCount(pieces.BlackKing) != 1)
        {
            throw new ArgumentException("Position must contain exactly one black king.", nameof(pieces));
        }

        Pieces = pieces;
        SideToMove = sideToMove;
        CastlingRights = castlingRights;
        EnPassantTarget = enPassantTarget;
        HalfmoveClock = halfmoveClock;
        FullmoveNumber = fullmoveNumber;
        UpdateOccupancies(pieces);
        Hash = PositionHash.Compute(pieces, sideToMove, castlingRights, enPassantTarget);
        _positionHistory = new List<ulong> { Hash };
        _positionHistoryView = _positionHistory.AsReadOnly();
    }

    public PieceBitboards Pieces { get; private set; }

    public Color SideToMove { get; private set; }

    public CastlingRights CastlingRights { get; private set; }

    public Square? EnPassantTarget { get; private set; }

    public int HalfmoveClock { get; private set; }

    public int FullmoveNumber { get; private set; }

    public ulong Hash { get; private set; }

    public ulong WhiteOccupancy { get; private set; }

    public ulong BlackOccupancy { get; private set; }

    public ulong TotalOccupancy { get; private set; }

    public IReadOnlyList<ulong> PositionHistory => _positionHistoryView;

    /// <summary>
    /// Applies an ordinary move. Piece geometry and king safety are not checked here.
    /// </summary>
    public UndoState MakeMove(Move move)
    {
        ValidateSupportedMove(move);

        if (!TryGetPieceAt(Pieces, move.From, out var movingPiece))
        {
            throw new InvalidOperationException($"There is no piece on {move.From}.");
        }

        if (movingPiece.Color != SideToMove)
        {
            throw new InvalidOperationException("The move source must contain a piece of the side to move.");
        }

        var hasCapturedPiece = TryGetPieceAt(Pieces, move.To, out var destinationPiece);
        if (hasCapturedPiece && destinationPiece.Color == movingPiece.Color)
        {
            throw new InvalidOperationException("A move cannot capture a piece of the moving side.");
        }

        if (hasCapturedPiece && destinationPiece.Type == PieceType.King)
        {
            throw new InvalidOperationException("A move cannot capture a king.");
        }

        var isCapture = (move.Flags & MoveFlags.Capture) != 0;
        if (isCapture != hasCapturedPiece)
        {
            throw new InvalidOperationException("Capture flag must match whether the destination contains an enemy piece.");
        }

        if (movingPiece.Type == PieceType.Pawn)
        {
            ValidatePawnMove(move, movingPiece.Color, hasCapturedPiece);
        }
        else if ((move.Flags & MoveFlags.DoublePawnPush) != 0)
        {
            throw new InvalidOperationException("Only a pawn can make a double pawn push.");
        }

        var nextCastlingRights = UpdateCastlingRights(
            CastlingRights,
            movingPiece,
            move.From,
            hasCapturedPiece ? destinationPiece : null,
            move.To);
        var nextSideToMove = OppositeColor(SideToMove);
        var nextHalfmoveClock = movingPiece.Type == PieceType.Pawn || hasCapturedPiece
            ? 0
            : checked(HalfmoveClock + 1);
        var nextFullmoveNumber = SideToMove == Color.Black
            ? checked(FullmoveNumber + 1)
            : FullmoveNumber;
        Square? nextEnPassantTarget = (move.Flags & MoveFlags.DoublePawnPush) != 0
            ? new Square((move.From.Index + move.To.Index) / 2)
            : null;
        var nextPieces = ApplyMoveToPieces(Pieces, movingPiece, move, hasCapturedPiece ? destinationPiece : null);
        var nextHash = PositionHash.Compute(
            nextPieces,
            nextSideToMove,
            nextCastlingRights,
            nextEnPassantTarget);
        var undoState = new UndoState(
            hasCapturedPiece ? destinationPiece : null,
            hasCapturedPiece ? move.To : null,
            SideToMove,
            CastlingRights,
            EnPassantTarget,
            HalfmoveClock,
            FullmoveNumber,
            Hash);

        _positionHistory.Add(nextHash);
        Pieces = nextPieces;
        SideToMove = nextSideToMove;
        CastlingRights = nextCastlingRights;
        EnPassantTarget = nextEnPassantTarget;
        HalfmoveClock = nextHalfmoveClock;
        FullmoveNumber = nextFullmoveNumber;
        Hash = nextHash;
        UpdateOccupancies(nextPieces);

        return undoState;
    }

    public void UnmakeMove(Move move, UndoState undoState)
    {
        ValidateSupportedMove(move);

        if (!undoState.IsValid)
        {
            throw new ArgumentException("Undo state is invalid.", nameof(undoState));
        }

        if (_positionHistory.Count < 2
            || _positionHistory[^1] != Hash
            || _positionHistory[^2] != undoState.PreviousHash)
        {
            throw new InvalidOperationException("Undo state does not match the most recently made move.");
        }

        if (SideToMove != OppositeColor(undoState.PreviousSideToMove))
        {
            throw new InvalidOperationException("Undo state does not match the current side to move.");
        }

        if (!TryGetPieceAt(Pieces, move.To, out var movingPiece)
            || movingPiece.Color != undoState.PreviousSideToMove)
        {
            throw new InvalidOperationException("The moved piece is not on the move destination.");
        }

        if (TryGetPieceAt(Pieces, move.From, out _))
        {
            throw new InvalidOperationException("The move source must be empty before unmaking the move.");
        }

        var isCapture = (move.Flags & MoveFlags.Capture) != 0;
        if (isCapture != undoState.CapturedPiece.HasValue
            || isCapture != undoState.CapturedSquare.HasValue)
        {
            throw new InvalidOperationException("Undo capture data does not match the move.");
        }

        if (isCapture
            && (undoState.CapturedSquare != move.To
                || undoState.CapturedPiece!.Value.Color != OppositeColor(undoState.PreviousSideToMove)
                || undoState.CapturedPiece.Value.Type == PieceType.King))
        {
            throw new InvalidOperationException("Undo capture data does not match the move destination.");
        }

        var expectedCastlingRights = UpdateCastlingRights(
            undoState.PreviousCastlingRights,
            movingPiece,
            move.From,
            undoState.CapturedPiece,
            move.To);
        Square? expectedEnPassantTarget = (move.Flags & MoveFlags.DoublePawnPush) != 0
            ? new Square((move.From.Index + move.To.Index) / 2)
            : null;
        var expectedHalfmoveClock = movingPiece.Type == PieceType.Pawn || isCapture
            ? 0
            : checked(undoState.PreviousHalfmoveClock + 1);
        var expectedFullmoveNumber = undoState.PreviousSideToMove == Color.Black
            ? checked(undoState.PreviousFullmoveNumber + 1)
            : undoState.PreviousFullmoveNumber;

        if (CastlingRights != expectedCastlingRights
            || EnPassantTarget != expectedEnPassantTarget
            || HalfmoveClock != expectedHalfmoveClock
            || FullmoveNumber != expectedFullmoveNumber)
        {
            throw new InvalidOperationException("Undo state does not match the current position metadata.");
        }

        var restoredPieces = ApplyMoveToPieces(
            Pieces,
            movingPiece,
            move,
            undoState.CapturedPiece,
            reverse: true);
        var restoredHash = PositionHash.Compute(
            restoredPieces,
            undoState.PreviousSideToMove,
            undoState.PreviousCastlingRights,
            undoState.PreviousEnPassantTarget);
        if (restoredHash != undoState.PreviousHash)
        {
            throw new InvalidOperationException("Undo state does not restore the previous position hash.");
        }

        _positionHistory.RemoveAt(_positionHistory.Count - 1);
        Pieces = restoredPieces;
        SideToMove = undoState.PreviousSideToMove;
        CastlingRights = undoState.PreviousCastlingRights;
        EnPassantTarget = undoState.PreviousEnPassantTarget;
        HalfmoveClock = undoState.PreviousHalfmoveClock;
        FullmoveNumber = undoState.PreviousFullmoveNumber;
        Hash = undoState.PreviousHash;
        UpdateOccupancies(restoredPieces);
    }

    public bool Equals(Position? other) =>
        other is not null
        && Pieces == other.Pieces
        && SideToMove == other.SideToMove
        && CastlingRights == other.CastlingRights
        && EnPassantTarget == other.EnPassantTarget
        && HalfmoveClock == other.HalfmoveClock
        && FullmoveNumber == other.FullmoveNumber
        && Hash == other.Hash;

    public override bool Equals(object? obj) => obj is Position other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(
            Pieces,
            SideToMove,
            CastlingRights,
            EnPassantTarget,
            HalfmoveClock,
            FullmoveNumber,
            Hash);

    public static bool operator ==(Position? left, Position? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(Position? left, Position? right) => !(left == right);

    private static void ValidateSupportedMove(Move move)
    {
        if (!move.IsValid)
        {
            throw new ArgumentException("Move must be valid.", nameof(move));
        }

        if (move.Promotion.HasValue
            || (move.Flags & (MoveFlags.EnPassant | MoveFlags.KingSideCastle | MoveFlags.QueenSideCastle)) != 0)
        {
            throw new NotSupportedException("Castling, en-passant, and promotion moves are not supported yet.");
        }
    }

    private void ValidatePawnMove(Move move, Color color, bool hasCapturedPiece)
    {
        var direction = color == Color.White ? 1 : -1;
        var rankDelta = move.To.Rank - move.From.Rank;
        var fileDelta = move.To.File - move.From.File;
        var isDoublePawnPush = (move.Flags & MoveFlags.DoublePawnPush) != 0;

        if (move.To.Rank == (color == Color.White ? 7 : 0))
        {
            throw new NotSupportedException("Promotion moves are not supported yet.");
        }

        if (isDoublePawnPush)
        {
            var startingRank = color == Color.White ? 1 : 6;
            if (move.From.Rank != startingRank
                || fileDelta != 0
                || rankDelta != 2 * direction
                || hasCapturedPiece)
            {
                throw new InvalidOperationException("Double pawn push must move two clear squares from the starting rank.");
            }

            var intermediateSquare = new Square(move.From.Index + direction * 8);
            if (Bitboard.IsSet(TotalOccupancy, intermediateSquare))
            {
                throw new InvalidOperationException("Double pawn push path must be clear.");
            }

            return;
        }

        if (hasCapturedPiece)
        {
            if (Math.Abs(fileDelta) != 1 || rankDelta != direction)
            {
                throw new InvalidOperationException("A pawn capture must move one rank diagonally forward.");
            }
        }
        else if (fileDelta != 0 || rankDelta != direction)
        {
            throw new InvalidOperationException("A quiet pawn move must move one square forward.");
        }
    }

    private static PieceBitboards ApplyMoveToPieces(
        PieceBitboards pieces,
        Piece movingPiece,
        Move move,
        Piece? capturedPiece,
        bool reverse = false)
    {
        Span<ulong> bitboards = stackalloc ulong[]
        {
            pieces.WhitePawns,
            pieces.WhiteKnights,
            pieces.WhiteBishops,
            pieces.WhiteRooks,
            pieces.WhiteQueens,
            pieces.WhiteKing,
            pieces.BlackPawns,
            pieces.BlackKnights,
            pieces.BlackBishops,
            pieces.BlackRooks,
            pieces.BlackQueens,
            pieces.BlackKing
        };
        var movingPieceIndex = GetPieceIndex(movingPiece);
        var fromMask = 1UL << move.From.Index;
        var toMask = 1UL << move.To.Index;

        if (reverse)
        {
            bitboards[movingPieceIndex] &= ~toMask;
            bitboards[movingPieceIndex] |= fromMask;
            if (capturedPiece.HasValue)
            {
                bitboards[GetPieceIndex(capturedPiece.Value)] |= toMask;
            }
        }
        else
        {
            bitboards[movingPieceIndex] &= ~fromMask;
            if (capturedPiece.HasValue)
            {
                bitboards[GetPieceIndex(capturedPiece.Value)] &= ~toMask;
            }

            bitboards[movingPieceIndex] |= toMask;
        }

        return new PieceBitboards(
            whitePawns: bitboards[0],
            whiteKnights: bitboards[1],
            whiteBishops: bitboards[2],
            whiteRooks: bitboards[3],
            whiteQueens: bitboards[4],
            whiteKing: bitboards[5],
            blackPawns: bitboards[6],
            blackKnights: bitboards[7],
            blackBishops: bitboards[8],
            blackRooks: bitboards[9],
            blackQueens: bitboards[10],
            blackKing: bitboards[11]);
    }

    private static bool TryGetPieceAt(PieceBitboards pieces, Square square, out Piece piece)
    {
        Span<ulong> bitboards = stackalloc ulong[]
        {
            pieces.WhitePawns,
            pieces.WhiteKnights,
            pieces.WhiteBishops,
            pieces.WhiteRooks,
            pieces.WhiteQueens,
            pieces.WhiteKing,
            pieces.BlackPawns,
            pieces.BlackKnights,
            pieces.BlackBishops,
            pieces.BlackRooks,
            pieces.BlackQueens,
            pieces.BlackKing
        };
        var mask = 1UL << square.Index;

        for (var index = 0; index < bitboards.Length; index++)
        {
            if ((bitboards[index] & mask) != 0)
            {
                piece = new Piece((Color)(index / 6), (PieceType)(index % 6));
                return true;
            }
        }

        piece = default;
        return false;
    }

    private static int GetPieceIndex(Piece piece) =>
        ((int)piece.Color * 6) + (int)piece.Type;

    private static CastlingRights UpdateCastlingRights(
        CastlingRights rights,
        Piece movingPiece,
        Square from,
        Piece? capturedPiece,
        Square to)
    {
        if (movingPiece.Type == PieceType.King)
        {
            rights &= movingPiece.Color == Color.White
                ? ~(CastlingRights.WhiteKingSide | CastlingRights.WhiteQueenSide)
                : ~(CastlingRights.BlackKingSide | CastlingRights.BlackQueenSide);
        }

        if (movingPiece.Type == PieceType.Rook)
        {
            rights &= ~RookCastlingRight(movingPiece.Color, from);
        }

        if (capturedPiece is { Type: PieceType.Rook } rook)
        {
            rights &= ~RookCastlingRight(rook.Color, to);
        }

        return rights;
    }

    private static CastlingRights RookCastlingRight(Color color, Square square) =>
        (color, square.Index) switch
        {
            (Color.White, 0) => CastlingRights.WhiteQueenSide,
            (Color.White, 7) => CastlingRights.WhiteKingSide,
            (Color.Black, 56) => CastlingRights.BlackQueenSide,
            (Color.Black, 63) => CastlingRights.BlackKingSide,
            _ => CastlingRights.None
        };

    private static Color OppositeColor(Color color) =>
        color == Color.White ? Color.Black : Color.White;

    private void UpdateOccupancies(PieceBitboards pieces)
    {
        WhiteOccupancy = pieces.WhiteOccupancy;
        BlackOccupancy = pieces.BlackOccupancy;
        TotalOccupancy = pieces.TotalOccupancy;
    }

    private static void ValidateDisjointPieceBitboards(PieceBitboards pieces)
    {
        Span<ulong> bitboards = stackalloc ulong[]
        {
            pieces.WhitePawns,
            pieces.WhiteKnights,
            pieces.WhiteBishops,
            pieces.WhiteRooks,
            pieces.WhiteQueens,
            pieces.WhiteKing,
            pieces.BlackPawns,
            pieces.BlackKnights,
            pieces.BlackBishops,
            pieces.BlackRooks,
            pieces.BlackQueens,
            pieces.BlackKing
        };

        var occupied = 0UL;

        foreach (var bitboard in bitboards)
        {
            var overlap = occupied & bitboard;
            if (overlap != 0)
            {
                var square = new Square(BitOperations.TrailingZeroCount(overlap));
                throw new ArgumentException(
                    $"Piece bitboards overlap at square {square}.",
                    nameof(pieces));
            }

            occupied |= bitboard;
        }
    }
}
