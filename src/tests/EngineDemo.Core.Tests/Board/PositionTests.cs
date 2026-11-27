using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class PositionTests
{
    [Fact]
    public void PieceBitboards_DeriveZeroOccupancyForAnEmptySide()
    {
        var pieces = new PieceBitboards(whitePawns: 1UL << 8);

        Assert.Equal(1UL << 8, pieces.WhiteOccupancy);
        Assert.Equal(0UL, pieces.BlackOccupancy);
        Assert.Equal(1UL << 8, pieces.TotalOccupancy);
    }

    [Fact]
    public void Position_DerivesMixedColorOccupancyFromPieceBitboards()
    {
        var pieces = new PieceBitboards(
            whitePawns: 1UL << 8,
            whiteKing: 1UL << 4,
            blackKnights: 1UL << 57,
            blackKing: 1UL << 60);
        var position = new Position(pieces);

        Assert.Equal((1UL << 8) | (1UL << 4), position.WhiteOccupancy);
        Assert.Equal((1UL << 57) | (1UL << 60), position.BlackOccupancy);
        Assert.Equal(position.WhiteOccupancy | position.BlackOccupancy, position.TotalOccupancy);
    }

    [Fact]
    public void Position_AcceptsValidSparseStateAndDefaultsToNoEnPassantTarget()
    {
        var position = new Position(CreateKings());

        Assert.Equal(Color.White, position.SideToMove);
        Assert.Equal(CastlingRights.None, position.CastlingRights);
        Assert.Null(position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(1, position.FullmoveNumber);
    }

    [Fact]
    public void Position_RejectsOverlappingPieceBitboardsWithUsefulError()
    {
        var pieces = new PieceBitboards(
            whitePawns: 1UL << 20,
            whiteKing: 1UL,
            blackKnights: 1UL << 20,
            blackKing: 1UL << 63);

        var exception = Assert.Throws<ArgumentException>(() => new Position(pieces));

        Assert.Contains("overlap", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(Color.White)]
    [InlineData(Color.Black)]
    public void Position_RequiresOneKingForEachColor(Color missingKing)
    {
        var pieces = missingKing == Color.White
            ? new PieceBitboards(blackKing: 1UL << 63)
            : new PieceBitboards(whiteKing: 1UL);

        Assert.Throws<ArgumentException>(() => new Position(pieces));
    }

    [Theory]
    [InlineData(Color.White)]
    [InlineData(Color.Black)]
    public void Position_RejectsDuplicateKings(Color duplicateKing)
    {
        var pieces = duplicateKing == Color.White
            ? new PieceBitboards(whiteKing: 0b11, blackKing: 1UL << 63)
            : new PieceBitboards(whiteKing: 1UL << 63, blackKing: 0b11);

        Assert.Throws<ArgumentException>(() => new Position(pieces));
    }

    [Theory]
    [InlineData(-1)]
    public void Position_RejectsNegativeHalfmoveClock(int halfmoveClock)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Position(CreateKings(), halfmoveClock: halfmoveClock));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Position_RejectsFullmoveNumbersBelowOne(int fullmoveNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Position(CreateKings(), fullmoveNumber: fullmoveNumber));
    }

    [Fact]
    public void Position_RepresentsOptionalEnPassantTargetExplicitly()
    {
        var target = new Square(40);
        var position = new Position(CreateKings(), enPassantTarget: target);

        Assert.Equal(target, position.EnPassantTarget);
    }

    [Fact]
    public void Position_RejectsInvalidEnPassantSquare()
    {
        Assert.Throws<ArgumentException>(() =>
            new Position(CreateKings(), enPassantTarget: default(Square)));
    }

    [Fact]
    public void Position_RejectsInvalidSideAndCastlingRights()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Position(CreateKings(), sideToMove: (Color)2));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Position(CreateKings(), castlingRights: (CastlingRights)16));
    }

    [Fact]
    public void Position_EqualityIncludesPiecesAndEveryMetadataField()
    {
        var pieces = CreateKings();
        var position = new Position(pieces);

        Assert.Equal(position, new Position(pieces));
        Assert.NotEqual(position, new Position(CreateKings(whiteKingIndex: 1)));
        Assert.NotEqual(position, new Position(pieces, sideToMove: Color.Black));
        Assert.NotEqual(position, new Position(pieces, castlingRights: CastlingRights.WhiteKingSide));
        Assert.NotEqual(position, new Position(pieces, enPassantTarget: new Square(40)));
        Assert.NotEqual(position, new Position(pieces, halfmoveClock: 1));
        Assert.NotEqual(position, new Position(pieces, fullmoveNumber: 2));
    }

    private static PieceBitboards CreateKings(int whiteKingIndex = 0, int blackKingIndex = 63) =>
        new(whiteKing: 1UL << whiteKingIndex, blackKing: 1UL << blackKingIndex);
}
