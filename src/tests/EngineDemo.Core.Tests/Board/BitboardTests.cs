using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class BitboardTests
{
    [Fact]
    public void SetAndClear_UseSquareIndicesWithoutMutatingInputs()
    {
        var empty = 0UL;
        var a1 = new Square(0);
        var h8 = new Square(63);

        var withA1 = Bitboard.Set(empty, a1);
        var withBothCorners = Bitboard.Set(withA1, h8);
        var withoutA1 = Bitboard.Clear(withBothCorners, a1);

        Assert.Equal(0UL, empty);
        Assert.False(Bitboard.IsSet(empty, a1));
        Assert.True(Bitboard.IsSet(withA1, a1));
        Assert.False(Bitboard.IsSet(withA1, h8));
        Assert.Equal((1UL << 63) | 1UL, withBothCorners);
        Assert.True(Bitboard.IsSet(withBothCorners, h8));
        Assert.False(Bitboard.IsSet(withoutA1, a1));
        Assert.True(Bitboard.IsSet(withoutA1, h8));
    }

    [Fact]
    public void SetAndClear_AreIdempotent()
    {
        var square = new Square(27);
        var setOnce = Bitboard.Set(0UL, square);
        var setTwice = Bitboard.Set(setOnce, square);
        var clearedOnce = Bitboard.Clear(setTwice, square);
        var clearedTwice = Bitboard.Clear(clearedOnce, square);

        Assert.Equal(setOnce, setTwice);
        Assert.Equal(0UL, clearedOnce);
        Assert.Equal(clearedOnce, clearedTwice);
    }

    [Fact]
    public void PopCount_HandlesEmptySingletonSparseAndFullBitboards()
    {
        var sparse = Bitboard.Set(Bitboard.Set(0UL, new Square(0)), new Square(63));

        Assert.Equal(0, Bitboard.PopCount(0UL));
        Assert.Equal(1, Bitboard.PopCount(Bitboard.Set(0UL, new Square(36))));
        Assert.Equal(2, Bitboard.PopCount(sparse));
        Assert.Equal(64, Bitboard.PopCount(ulong.MaxValue));
    }

    [Fact]
    public void EnumerateSquares_EmptyBoardYieldsNothing()
    {
        Assert.Empty(Bitboard.EnumerateSquares(0UL));
    }

    [Fact]
    public void EnumerateSquares_ReturnsEachSetSquareOnceInAscendingIndexOrder()
    {
        var original = 0UL;
        var bitboard = Bitboard.Set(original, new Square(63));
        bitboard = Bitboard.Set(bitboard, new Square(17));
        bitboard = Bitboard.Set(bitboard, new Square(0));
        bitboard = Bitboard.Set(bitboard, new Square(35));

        var expectedSquares = new[]
        {
            new Square(0),
            new Square(17),
            new Square(35),
            new Square(63)
        };
        var expectedSet = new HashSet<Square>(expectedSquares);
        var actualSquares = Bitboard.EnumerateSquares(bitboard).ToArray();

        Assert.True(expectedSet.SetEquals(actualSquares));
        Assert.Equal(expectedSquares, actualSquares);
        Assert.All(actualSquares, square => Assert.True(square.IsValid));
        Assert.Equal(0UL, original);
    }

    [Fact]
    public void Operations_RejectDefaultSquare()
    {
        Assert.Throws<ArgumentException>(() => Bitboard.Set(0UL, default));
        Assert.Throws<ArgumentException>(() => Bitboard.Clear(0UL, default));
        Assert.Throws<ArgumentException>(() => Bitboard.IsSet(0UL, default));
    }
}
