using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class PositionFactoryTests
{
    [Fact]
    public void CreateStandardInitialPosition_SetsExactPiecePlacement()
    {
        var position = PositionFactory.CreateStandardInitialPosition();

        Assert.Equal(Mask("a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2"), position.Pieces.WhitePawns);
        Assert.Equal(Mask("b1", "g1"), position.Pieces.WhiteKnights);
        Assert.Equal(Mask("c1", "f1"), position.Pieces.WhiteBishops);
        Assert.Equal(Mask("a1", "h1"), position.Pieces.WhiteRooks);
        Assert.Equal(Mask("d1"), position.Pieces.WhiteQueens);
        Assert.Equal(Mask("e1"), position.Pieces.WhiteKing);

        Assert.Equal(Mask("a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7"), position.Pieces.BlackPawns);
        Assert.Equal(Mask("b8", "g8"), position.Pieces.BlackKnights);
        Assert.Equal(Mask("c8", "f8"), position.Pieces.BlackBishops);
        Assert.Equal(Mask("a8", "h8"), position.Pieces.BlackRooks);
        Assert.Equal(Mask("d8"), position.Pieces.BlackQueens);
        Assert.Equal(Mask("e8"), position.Pieces.BlackKing);
    }

    [Fact]
    public void CreateStandardInitialPosition_SetsStandardMetadataAndOccupancy()
    {
        var position = PositionFactory.CreateStandardInitialPosition();

        Assert.Equal(Color.White, position.SideToMove);
        Assert.Equal(
            CastlingRights.WhiteKingSide
            | CastlingRights.WhiteQueenSide
            | CastlingRights.BlackKingSide
            | CastlingRights.BlackQueenSide,
            position.CastlingRights);
        Assert.Null(position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(1, position.FullmoveNumber);

        Assert.Equal(0x000000000000FFFFUL, position.WhiteOccupancy);
        Assert.Equal(0xFFFF000000000000UL, position.BlackOccupancy);
        Assert.Equal(0xFFFF00000000FFFFUL, position.TotalOccupancy);
        Assert.Equal(16, Bitboard.PopCount(position.WhiteOccupancy));
        Assert.Equal(16, Bitboard.PopCount(position.BlackOccupancy));
        Assert.Equal(32, Bitboard.PopCount(position.TotalOccupancy));
    }

    [Fact]
    public void CreateStandardInitialPosition_IsValidatedAndHasItsDeterministicKey()
    {
        var position = PositionFactory.CreateStandardInitialPosition();

        Assert.Equal(1, Bitboard.PopCount(position.Pieces.WhiteKing));
        Assert.Equal(1, Bitboard.PopCount(position.Pieces.BlackKing));
        Assert.Equal(position.Hash, PositionHash.Compute(position));
    }

    [Fact]
    public void CreateStandardInitialPosition_ReturnsEqualPositionsAndKeysAcrossCalls()
    {
        var first = PositionFactory.CreateStandardInitialPosition();
        var second = PositionFactory.CreateStandardInitialPosition();

        Assert.Equal(first, second);
        Assert.Equal(first.Hash, second.Hash);
    }

    private static ulong Mask(params string[] coordinates)
    {
        var bitboard = 0UL;
        foreach (var coordinate in coordinates)
        {
            bitboard = Bitboard.Set(bitboard, Square.Parse(coordinate));
        }

        return bitboard;
    }
}
