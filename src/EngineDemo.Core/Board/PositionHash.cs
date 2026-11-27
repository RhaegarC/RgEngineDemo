using System.Numerics;

namespace EngineDemo.Core.Board;

/// <summary>
/// Computes deterministic Zobrist keys for positions.
/// </summary>
/// <remarks>
/// Keys are generated with SplitMix64 from seed 0xC0FFEE123456789A.
/// </remarks>
public static class PositionHash
{
    private const ulong Seed = 0xC0FFEE123456789AUL;

    private static readonly ulong[,] PieceKeys;
    private static readonly ulong[] CastlingKeys;
    private static readonly ulong[] EnPassantKeys;
    private static readonly ulong SideToMoveKey;

    static PositionHash()
    {
        var state = Seed;
        PieceKeys = new ulong[12, 64];
        CastlingKeys = new ulong[4];
        EnPassantKeys = new ulong[64];

        for (var pieceIndex = 0; pieceIndex < 12; pieceIndex++)
        {
            for (var squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                PieceKeys[pieceIndex, squareIndex] = Next(ref state);
            }
        }

        for (var index = 0; index < CastlingKeys.Length; index++)
        {
            CastlingKeys[index] = Next(ref state);
        }

        for (var squareIndex = 0; squareIndex < EnPassantKeys.Length; squareIndex++)
        {
            EnPassantKeys[squareIndex] = Next(ref state);
        }

        SideToMoveKey = Next(ref state);
    }

    public static ulong Compute(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);

        var pieces = position.Pieces;
        var hash = 0UL;

        AddPieces(ref hash, pieces.WhitePawns, 0);
        AddPieces(ref hash, pieces.WhiteKnights, 1);
        AddPieces(ref hash, pieces.WhiteBishops, 2);
        AddPieces(ref hash, pieces.WhiteRooks, 3);
        AddPieces(ref hash, pieces.WhiteQueens, 4);
        AddPieces(ref hash, pieces.WhiteKing, 5);
        AddPieces(ref hash, pieces.BlackPawns, 6);
        AddPieces(ref hash, pieces.BlackKnights, 7);
        AddPieces(ref hash, pieces.BlackBishops, 8);
        AddPieces(ref hash, pieces.BlackRooks, 9);
        AddPieces(ref hash, pieces.BlackQueens, 10);
        AddPieces(ref hash, pieces.BlackKing, 11);

        if (position.SideToMove == Color.Black)
        {
            hash ^= SideToMoveKey;
        }

        if ((position.CastlingRights & CastlingRights.WhiteKingSide) != 0)
        {
            hash ^= CastlingKeys[0];
        }

        if ((position.CastlingRights & CastlingRights.WhiteQueenSide) != 0)
        {
            hash ^= CastlingKeys[1];
        }

        if ((position.CastlingRights & CastlingRights.BlackKingSide) != 0)
        {
            hash ^= CastlingKeys[2];
        }

        if ((position.CastlingRights & CastlingRights.BlackQueenSide) != 0)
        {
            hash ^= CastlingKeys[3];
        }

        if (position.EnPassantTarget is { } enPassantTarget)
        {
            hash ^= EnPassantKeys[enPassantTarget.Index];
        }

        return hash;
    }

    private static void AddPieces(ref ulong hash, ulong bitboard, int pieceIndex)
    {
        while (bitboard != 0)
        {
            var squareIndex = BitOperations.TrailingZeroCount(bitboard);
            hash ^= PieceKeys[pieceIndex, squareIndex];
            bitboard &= bitboard - 1;
        }
    }

    private static ulong Next(ref ulong state)
    {
        unchecked
        {
            state += 0x9E3779B97F4A7C15UL;
            var value = state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
