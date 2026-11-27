using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class SquareTests
{
    [Theory]
    [InlineData("a1", 0)]
    [InlineData("h1", 7)]
    [InlineData("a8", 56)]
    [InlineData("h8", 63)]
    public void Parse_MapsCornerCoordinatesToDocumentedIndices(string coordinate, int expectedIndex)
    {
        var square = Square.Parse(coordinate);

        Assert.Equal(expectedIndex, square.Index);
    }

    [Fact]
    public void EverySquare_RoundTripsThroughCoordinateNotation()
    {
        for (var index = 0; index < 64; index++)
        {
            var square = new Square(index);

            Assert.True(Square.TryParse(square.ToString(), out var parsed));
            Assert.Equal(square, parsed);
        }
    }

    [Fact]
    public void EverySquare_ExposesConsistentFileAndRank()
    {
        for (var index = 0; index < 64; index++)
        {
            var square = new Square(index);

            Assert.Equal(index % 8, square.File);
            Assert.Equal(index / 8, square.Rank);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("a10")]
    [InlineData("A1")]
    [InlineData("a0")]
    [InlineData("a9")]
    [InlineData("i1")]
    [InlineData(" 1")]
    public void TryParse_RejectsInvalidCoordinatesWithoutReturningAValidSquare(string? coordinate)
    {
        Assert.False(Square.TryParse(coordinate, out var square));
        Assert.False(square.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Constructor_RejectsIndicesOutsideTheBoard(int index)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Square(index));
    }

    [Fact]
    public void DefaultSquare_IsInvalid()
    {
        Assert.False(default(Square).IsValid);
    }
}
