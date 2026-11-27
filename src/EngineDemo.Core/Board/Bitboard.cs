using System.Numerics;

namespace EngineDemo.Core.Board;

public static class Bitboard
{
    public static ulong Set(ulong bitboard, Square square) => bitboard | Mask(square);

    public static ulong Clear(ulong bitboard, Square square) => bitboard & ~Mask(square);

    public static bool IsSet(ulong bitboard, Square square) => (bitboard & Mask(square)) != 0;

    public static int PopCount(ulong bitboard) => BitOperations.PopCount(bitboard);

    /// <summary>
    /// Enumerates set squares in ascending square-index order.
    /// </summary>
    public static IEnumerable<Square> EnumerateSquares(ulong bitboard)
    {
        while (bitboard != 0)
        {
            var index = BitOperations.TrailingZeroCount(bitboard);
            yield return new Square(index);
            bitboard &= bitboard - 1;
        }
    }

    private static ulong Mask(Square square)
    {
        if (!square.IsValid)
        {
            throw new ArgumentException("Square must be valid.", nameof(square));
        }

        return 1UL << square.Index;
    }
}
