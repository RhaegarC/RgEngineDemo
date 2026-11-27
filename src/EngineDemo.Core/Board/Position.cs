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

public sealed record Position
{
    private const CastlingRights AllCastlingRights =
        CastlingRights.WhiteKingSide
        | CastlingRights.WhiteQueenSide
        | CastlingRights.BlackKingSide
        | CastlingRights.BlackQueenSide;

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
        Hash = PositionHash.Compute(this);
    }

    public PieceBitboards Pieces { get; }

    public Color SideToMove { get; }

    public CastlingRights CastlingRights { get; }

    public Square? EnPassantTarget { get; }

    public int HalfmoveClock { get; }

    public int FullmoveNumber { get; }

    public ulong Hash { get; }

    public ulong WhiteOccupancy => Pieces.WhiteOccupancy;

    public ulong BlackOccupancy => Pieces.BlackOccupancy;

    public ulong TotalOccupancy => Pieces.TotalOccupancy;

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
