using System.Runtime.CompilerServices;
using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class MoveTests
{
    [Fact]
    public void DefaultMove_IsInvalid()
    {
        Assert.False(default(Move).IsValid);
    }

    [Fact]
    public void Move_IsCompactValueType()
    {
        Assert.True(typeof(Move).IsValueType);
        Assert.InRange(Unsafe.SizeOf<Move>(), 1, 8);
    }

    [Fact]
    public void Constructor_PreservesOrdinaryMoveValues()
    {
        var from = new Square(12);
        var to = new Square(28);
        var move = new Move(from, to);

        Assert.True(move.IsValid);
        Assert.Equal(from, move.From);
        Assert.Equal(to, move.To);
        Assert.Null(move.Promotion);
        Assert.Equal(MoveFlags.None, move.Flags);
    }

    [Theory]
    [InlineData(MoveFlags.Capture)]
    [InlineData(MoveFlags.DoublePawnPush)]
    [InlineData(MoveFlags.Capture | MoveFlags.EnPassant)]
    [InlineData(MoveFlags.KingSideCastle)]
    [InlineData(MoveFlags.QueenSideCastle)]
    public void Constructor_AcceptsSupportedSpecialMoveFlags(MoveFlags flags)
    {
        var move = new Move(new Square(12), new Square(20), flags: flags);

        Assert.True(move.IsValid);
        Assert.Equal(flags, move.Flags);
    }

    [Theory]
    [InlineData(PieceType.Knight)]
    [InlineData(PieceType.Bishop)]
    [InlineData(PieceType.Rook)]
    [InlineData(PieceType.Queen)]
    public void Constructor_AcceptsEachPromotionPiece(PieceType promotion)
    {
        var move = new Move(new Square(48), new Square(56), promotion: promotion);

        Assert.True(move.IsValid);
        Assert.Equal(promotion, move.Promotion);
    }

    [Fact]
    public void Constructor_AcceptsCapturePromotion()
    {
        var move = new Move(
            new Square(48),
            new Square(57),
            promotion: PieceType.Queen,
            flags: MoveFlags.Capture);

        Assert.True(move.IsValid);
        Assert.Equal(PieceType.Queen, move.Promotion);
        Assert.Equal(MoveFlags.Capture, move.Flags);
    }

    [Fact]
    public void Constructor_RejectsInvalidOrIdenticalSquares()
    {
        Assert.Throws<ArgumentException>(() => new Move(default, new Square(1)));
        Assert.Throws<ArgumentException>(() => new Move(new Square(1), default));
        Assert.Throws<ArgumentException>(() => new Move(new Square(1), new Square(1)));
    }

    [Theory]
    [InlineData(PieceType.Pawn)]
    [InlineData(PieceType.King)]
    [InlineData((PieceType)6)]
    public void Constructor_RejectsInvalidPromotionPieces(PieceType promotion)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Move(new Square(48), new Square(56), promotion: promotion));
    }

    [Fact]
    public void Constructor_RejectsUnknownFlags()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Move(new Square(12), new Square(20), flags: (MoveFlags)32));
    }

    [Theory]
    [InlineData(MoveFlags.EnPassant)]
    [InlineData(MoveFlags.KingSideCastle | MoveFlags.QueenSideCastle)]
    [InlineData(MoveFlags.Capture | MoveFlags.KingSideCastle)]
    [InlineData(MoveFlags.DoublePawnPush | MoveFlags.Capture)]
    public void Constructor_RejectsIncompatibleFlags(MoveFlags flags)
    {
        Assert.Throws<ArgumentException>(() =>
            new Move(new Square(12), new Square(20), flags: flags));
    }

    [Fact]
    public void Constructor_RejectsPromotionCombinedWithDoublePawnPush()
    {
        Assert.Throws<ArgumentException>(() =>
            new Move(
                new Square(48),
                new Square(56),
                promotion: PieceType.Queen,
                flags: MoveFlags.DoublePawnPush));
    }
}
