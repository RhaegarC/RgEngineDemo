using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class PieceTests
{
    [Theory]
    [InlineData('P', Color.White, PieceType.Pawn)]
    [InlineData('N', Color.White, PieceType.Knight)]
    [InlineData('B', Color.White, PieceType.Bishop)]
    [InlineData('R', Color.White, PieceType.Rook)]
    [InlineData('Q', Color.White, PieceType.Queen)]
    [InlineData('K', Color.White, PieceType.King)]
    [InlineData('p', Color.Black, PieceType.Pawn)]
    [InlineData('n', Color.Black, PieceType.Knight)]
    [InlineData('b', Color.Black, PieceType.Bishop)]
    [InlineData('r', Color.Black, PieceType.Rook)]
    [InlineData('q', Color.Black, PieceType.Queen)]
    [InlineData('k', Color.Black, PieceType.King)]
    public void FenSymbol_MapsToPieceAndRoundTrips(char symbol, Color expectedColor, PieceType expectedType)
    {
        var parsed = Piece.FromFenSymbol(symbol);
        var constructed = new Piece(expectedColor, expectedType);

        Assert.Equal(expectedColor, parsed.Color);
        Assert.Equal(expectedType, parsed.Type);
        Assert.Equal(symbol, parsed.ToFenSymbol());
        Assert.Equal(symbol, constructed.ToFenSymbol());
        Assert.Equal(parsed, Piece.FromFenSymbol(parsed.ToFenSymbol()));
    }

    [Theory]
    [InlineData('\0')]
    [InlineData('x')]
    [InlineData('X')]
    [InlineData('1')]
    [InlineData(' ')]
    public void FromFenSymbol_RejectsUnsupportedSymbols(char symbol)
    {
        Assert.Throws<FormatException>(() => Piece.FromFenSymbol(symbol));
    }

    [Fact]
    public void Constructor_RejectsInvalidColor()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Piece((Color)2, PieceType.Pawn));
    }

    [Fact]
    public void Constructor_RejectsInvalidPieceType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Piece(Color.White, (PieceType)6));
    }
}
