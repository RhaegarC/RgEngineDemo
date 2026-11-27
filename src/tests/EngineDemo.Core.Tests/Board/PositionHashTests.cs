using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class PositionHashTests
{
    [Fact]
    public void PositionConstruction_ComputesDocumentedStableZobristKey()
    {
        var position = CreateGoldenPosition();

        Assert.Equal(0x728F58468133729DUL, position.Hash);
    }

    [Fact]
    public void EqualPositionsConstructedIndependently_HaveEqualKeys()
    {
        var first = CreateGoldenPosition();
        var second = CreateGoldenPosition();

        Assert.Equal(first, second);
        Assert.Equal(first.Hash, second.Hash);
        Assert.Equal(first.Hash, PositionHash.Compute(first));
    }

    [Fact]
    public void Hash_ChangesWhenPiecePlacementChanges()
    {
        var original = new Position(CreateKings());
        var changed = new Position(CreateKings(whitePawns: 1UL << 8));

        Assert.NotEqual(original.Hash, changed.Hash);
    }

    [Fact]
    public void Hash_ChangesWhenSideToMoveChanges()
    {
        var whiteToMove = new Position(CreateKings(), sideToMove: Color.White);
        var blackToMove = new Position(CreateKings(), sideToMove: Color.Black);

        Assert.NotEqual(whiteToMove.Hash, blackToMove.Hash);
    }

    [Theory]
    [InlineData(CastlingRights.WhiteKingSide)]
    [InlineData(CastlingRights.WhiteQueenSide)]
    [InlineData(CastlingRights.BlackKingSide)]
    [InlineData(CastlingRights.BlackQueenSide)]
    public void Hash_ChangesForEachCastlingRight(CastlingRights right)
    {
        var withoutRights = new Position(CreateKings());
        var withRight = new Position(CreateKings(), castlingRights: right);

        Assert.NotEqual(withoutRights.Hash, withRight.Hash);
    }

    [Fact]
    public void Hash_DistinguishesEnPassantTargetStateAndSquare()
    {
        var withoutTarget = new Position(CreateKings());
        var targetAtE6 = new Position(CreateKings(), enPassantTarget: new Square(44));
        var targetAtD6 = new Position(CreateKings(), enPassantTarget: new Square(43));

        Assert.NotEqual(withoutTarget.Hash, targetAtE6.Hash);
        Assert.NotEqual(targetAtE6.Hash, targetAtD6.Hash);
    }

    [Fact]
    public void Hash_IgnoresMoveCounters()
    {
        var first = new Position(CreateKings(), halfmoveClock: 0, fullmoveNumber: 1);
        var second = new Position(CreateKings(), halfmoveClock: 99, fullmoveNumber: 42);

        Assert.NotEqual(first, second);
        Assert.Equal(first.Hash, second.Hash);
    }

    [Fact]
    public void Compute_DoesNotMutatePositionState()
    {
        var position = CreateGoldenPosition();
        var pieces = position.Pieces;
        var sideToMove = position.SideToMove;
        var castlingRights = position.CastlingRights;
        var enPassantTarget = position.EnPassantTarget;
        var halfmoveClock = position.HalfmoveClock;
        var fullmoveNumber = position.FullmoveNumber;
        var hash = position.Hash;
        var history = position.PositionHistory.ToArray();

        var computedHash = PositionHash.Compute(position);

        Assert.Equal(pieces, position.Pieces);
        Assert.Equal(sideToMove, position.SideToMove);
        Assert.Equal(castlingRights, position.CastlingRights);
        Assert.Equal(enPassantTarget, position.EnPassantTarget);
        Assert.Equal(halfmoveClock, position.HalfmoveClock);
        Assert.Equal(fullmoveNumber, position.FullmoveNumber);
        Assert.Equal(hash, position.Hash);
        Assert.Equal(history, position.PositionHistory);
        Assert.Equal(hash, computedHash);
    }

    [Fact]
    public void Compute_RejectsNullPosition()
    {
        Assert.Throws<ArgumentNullException>(() => PositionHash.Compute(null!));
    }

    private static Position CreateGoldenPosition() =>
        new(
            new PieceBitboards(
                whitePawns: 1UL << 8,
                whiteKing: 1UL << 4,
                blackKnights: 1UL << 57,
                blackKing: 1UL << 60),
            sideToMove: Color.Black,
            castlingRights: CastlingRights.WhiteKingSide | CastlingRights.BlackQueenSide,
            enPassantTarget: new Square(20),
            halfmoveClock: 17,
            fullmoveNumber: 9);

    private static PieceBitboards CreateKings(ulong whitePawns = 0) =>
        new(whitePawns: whitePawns, whiteKing: 1UL, blackKing: 1UL << 63);
}
