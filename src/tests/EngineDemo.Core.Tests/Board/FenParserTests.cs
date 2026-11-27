using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class FenParserTests
{
    private const string SparsePosition = "4k3/8/8/3pP3/8/8/8/4K3";

    [Fact]
    public void Parse_InitialPositionMatchesStandardPositionFactory()
    {
        var position = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        var expected = PositionFactory.CreateStandardInitialPosition();

        Assert.Equal(expected, position);
        Assert.Equal(expected.Hash, position.Hash);
    }

    [Fact]
    public void Parse_SparsePositionPreservesPiecesAndMetadata()
    {
        var position = FenParser.Parse($"{SparsePosition} b Kq e3 7 42");

        Assert.Equal(Color.Black, position.SideToMove);
        Assert.Equal(CastlingRights.WhiteKingSide | CastlingRights.BlackQueenSide, position.CastlingRights);
        Assert.Equal(Square.Parse("e3"), position.EnPassantTarget);
        Assert.Equal(7, position.HalfmoveClock);
        Assert.Equal(42, position.FullmoveNumber);
        Assert.Equal((1UL << 4) | (1UL << 36), position.WhiteOccupancy);
        Assert.Equal((1UL << 35) | (1UL << 60), position.BlackOccupancy);
        Assert.Equal(position.WhiteOccupancy | position.BlackOccupancy, position.TotalOccupancy);
        Assert.Equal(position.Hash, FenParser.Parse($"{SparsePosition} b Kq e3 7 42").Hash);
    }

    [Theory]
    [InlineData("w", "e6")]
    [InlineData("b", "e3")]
    public void Parse_AcceptsSideConsistentEnPassantRank(string side, string target)
    {
        var position = FenParser.Parse($"4k3/8/8/8/8/8/8/4K3 {side} - {target} 0 1");

        Assert.Equal(Square.Parse(target), position.EnPassantTarget);
    }

    [Fact]
    public void Parse_AcceptsWhitespaceBetweenExactlySixFields()
    {
        var position = FenParser.Parse("  4k3/8/8/8/8/8/8/4K3\tw   - - 0 1  ");

        Assert.Equal(Color.White, position.SideToMove);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(1, position.FullmoveNumber);
    }

    [Theory]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - -")]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0")]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 1 extra")]
    public void Parse_RequiresExactlySixFields(string fen)
    {
        var exception = Assert.Throws<FormatException>(() => FenParser.Parse(fen));

        Assert.Contains("six fields", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("4k3/8/8/8/8/8/4K3", "piece placement")]
    [InlineData("4k2/8/8/8/8/8/8/4K3", "piece placement")]
    [InlineData("4k4/8/8/8/8/8/8/4K3", "piece placement")]
    [InlineData("4k3/8/8/8/8/8/8/4K2X", "piece placement")]
    [InlineData("4k3/8/8/8/8/8/8/4K03", "piece placement")]
    [InlineData("4k3/8/8/8/8/8/8/4K93", "piece placement")]
    public void Parse_RejectsMalformedPiecePlacementWithFieldContext(string placement, string field)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{placement} w - - 0 1"));

        Assert.Contains(field, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_RejectsInvalidSideToMoveWithFieldContext()
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{SparsePosition} white - - 0 1"));

        Assert.Contains("side", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("KK")]
    [InlineData("A")]
    [InlineData("--")]
    public void Parse_RejectsInvalidCastlingRightsWithFieldContext(string castling)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{SparsePosition} w {castling} - 0 1"));

        Assert.Contains("castling", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("w", "e3")]
    [InlineData("b", "e6")]
    [InlineData("w", "E6")]
    [InlineData("b", "e9")]
    public void Parse_RejectsInvalidEnPassantTargetWithFieldContext(string side, string target)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{SparsePosition} {side} - {target} 0 1"));

        Assert.Contains("en-passant", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("one")]
    [InlineData("2147483648")]
    public void Parse_RejectsInvalidHalfmoveClockWithFieldContext(string halfmoveClock)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{SparsePosition} w - - {halfmoveClock} 1"));

        Assert.Contains("halfmove", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("one")]
    [InlineData("2147483648")]
    public void Parse_RejectsInvalidFullmoveNumberWithFieldContext(string fullmoveNumber)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{SparsePosition} w - - 0 {fullmoveNumber}"));

        Assert.Contains("fullmove", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("4k3/8/8/8/8/8/8/8")]
    [InlineData("4kk2/8/8/8/8/8/8/4K3")]
    public void Parse_RejectsStructuralPositionErrorsWithInnerContext(string placement)
    {
        var exception = Assert.Throws<FormatException>(() =>
            FenParser.Parse($"{placement} w - - 0 1"));

        Assert.Contains("position", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void Parse_RejectsNullInput()
    {
        string? fen = null;

        Assert.Throws<ArgumentNullException>(() => FenParser.Parse(fen!));
    }
}
