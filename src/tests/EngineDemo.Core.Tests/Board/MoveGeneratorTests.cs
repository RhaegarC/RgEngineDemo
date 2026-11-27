using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class MoveGeneratorTests
{
    [Fact]
    public void GeneratePseudoLegalMoves_InitialPositionHasTwentyDeterministicallyOrderedMoves()
    {
        var position = PositionFactory.CreateStandardInitialPosition();
        Move[] expected =
        [
            new Move(Square.Parse("a2"), Square.Parse("a3")),
            new Move(Square.Parse("a2"), Square.Parse("a4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("b2"), Square.Parse("b3")),
            new Move(Square.Parse("b2"), Square.Parse("b4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("c2"), Square.Parse("c3")),
            new Move(Square.Parse("c2"), Square.Parse("c4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("d2"), Square.Parse("d3")),
            new Move(Square.Parse("d2"), Square.Parse("d4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("e2"), Square.Parse("e3")),
            new Move(Square.Parse("e2"), Square.Parse("e4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("f2"), Square.Parse("f3")),
            new Move(Square.Parse("f2"), Square.Parse("f4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("g2"), Square.Parse("g3")),
            new Move(Square.Parse("g2"), Square.Parse("g4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("h2"), Square.Parse("h3")),
            new Move(Square.Parse("h2"), Square.Parse("h4"), flags: MoveFlags.DoublePawnPush),
            new Move(Square.Parse("b1"), Square.Parse("a3")),
            new Move(Square.Parse("b1"), Square.Parse("c3")),
            new Move(Square.Parse("g1"), Square.Parse("f3")),
            new Move(Square.Parse("g1"), Square.Parse("h3"))
        ];

        var first = Generate(position);
        var second = Generate(position);

        Assert.Equal(expected, first);
        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(PieceType.Knight)]
    [InlineData(PieceType.Bishop)]
    [InlineData(PieceType.Rook)]
    [InlineData(PieceType.Queen)]
    [InlineData(PieceType.King)]
    public void GeneratePseudoLegalMoves_NonPawnTargetsMatchAttackMasks(PieceType pieceType)
    {
        var from = Square.Parse("d4");
        var whiteKing = pieceType == PieceType.King ? from : Square.Parse("e1");
        var sourceMask = 1UL << from.Index;
        var pieces = pieceType switch
        {
            PieceType.Knight => new PieceBitboards(
                whiteKnights: sourceMask,
                whiteKing: 1UL << whiteKing.Index,
                blackKing: Mask("h8")),
            PieceType.Bishop => new PieceBitboards(
                whiteBishops: sourceMask,
                whiteKing: 1UL << whiteKing.Index,
                blackKing: Mask("h8")),
            PieceType.Rook => new PieceBitboards(
                whiteRooks: sourceMask,
                whiteKing: 1UL << whiteKing.Index,
                blackKing: Mask("h8")),
            PieceType.Queen => new PieceBitboards(
                whiteQueens: sourceMask,
                whiteKing: 1UL << whiteKing.Index,
                blackKing: Mask("h8")),
            PieceType.King => new PieceBitboards(
                whiteKing: sourceMask,
                blackKing: Mask("h8")),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType))
        };
        var position = new Position(pieces);
        var attacks = pieceType switch
        {
            PieceType.Knight => AttackGenerator.KnightAttacks(from),
            PieceType.Bishop => AttackGenerator.BishopAttacks(from, position.TotalOccupancy),
            PieceType.Rook => AttackGenerator.RookAttacks(from, position.TotalOccupancy),
            PieceType.Queen => AttackGenerator.QueenAttacks(from, position.TotalOccupancy),
            PieceType.King => AttackGenerator.KingAttacks(from),
            _ => throw new ArgumentOutOfRangeException(nameof(pieceType))
        };
        var expected = Bitboard.EnumerateSquares(
            attacks & ~position.WhiteOccupancy & ~position.Pieces.BlackKing).ToArray();
        var actual = Generate(position)
            .Where(move => move.From == from)
            .Select(move => move.To)
            .ToArray();

        Assert.Equal(expected, actual);
        Assert.All(actual, square => Assert.InRange(square.Index, 0, 63));
    }

    [Fact]
    public void GeneratePseudoLegalMoves_SlidersStopAtBlockersAndMarkCaptures()
    {
        var from = Square.Parse("d4");
        var position = new Position(
            new PieceBitboards(
                whiteKnights: Mask("d6"),
                whiteRooks: Mask("d4"),
                whiteKing: Mask("a1"),
                blackBishops: Mask("f4"),
                blackKing: Mask("h8")));

        var rookMoves = Generate(position)
            .Where(move => move.From == from)
            .ToArray();

        Assert.Equal(
            new[]
            {
                Square.Parse("d1"),
                Square.Parse("d2"),
                Square.Parse("d3"),
                Square.Parse("a4"),
                Square.Parse("b4"),
                Square.Parse("c4"),
                Square.Parse("e4"),
                Square.Parse("f4"),
                Square.Parse("d5")
            },
            rookMoves.Select(move => move.To));
        Assert.Equal(MoveFlags.Capture, Assert.Single(rookMoves, move => move.To == Square.Parse("f4")).Flags);
        Assert.All(rookMoves.Where(move => move.To != Square.Parse("f4")), move =>
            Assert.Equal(MoveFlags.None, move.Flags));
    }

    [Fact]
    public void GeneratePseudoLegalMoves_DoesNotCaptureTheOpposingKing()
    {
        var position = new Position(
            new PieceBitboards(
                whiteRooks: Mask("d4"),
                whiteKing: Mask("a1"),
                blackKing: Mask("d8")));

        var moves = Generate(position);

        Assert.Contains(moves, move => move.From == Square.Parse("d4") && move.To == Square.Parse("d7"));
        Assert.DoesNotContain(moves, move => move.To == Square.Parse("d8"));
    }

    [Fact]
    public void GeneratePseudoLegalMoves_PawnsRespectBlockersAndOmitPromotionMoves()
    {
        var position = new Position(
            new PieceBitboards(
                whitePawns: Mask("e2") | Mask("d4") | Mask("g7"),
                whiteKnights: Mask("e3"),
                whiteKing: Mask("a1"),
                blackBishops: Mask("e5"),
                blackRooks: Mask("c5") | Mask("h8"),
                blackKing: Mask("a8")));

        var moves = Generate(position);

        var d4Moves = moves.Where(move => move.From == Square.Parse("d4"));

        Assert.Equal(
            new[]
            {
                new Move(Square.Parse("d4"), Square.Parse("c5"), flags: MoveFlags.Capture),
                new Move(Square.Parse("d4"), Square.Parse("d5")),
                new Move(Square.Parse("d4"), Square.Parse("e5"), flags: MoveFlags.Capture)
            },
            d4Moves);
        Assert.DoesNotContain(moves, move => move.From == Square.Parse("e2"));
        Assert.DoesNotContain(moves, move => move.From == Square.Parse("g7"));
    }

    [Fact]
    public void GeneratePseudoLegalMoves_DoublePushRequiresBothSquaresToBeClear()
    {
        var position = new Position(
            new PieceBitboards(
                whitePawns: Mask("e2"),
                whiteKing: Mask("a1"),
                blackBishops: Mask("e4"),
                blackKing: Mask("h8")));

        var pawnMoves = Generate(position)
            .Where(move => move.From == Square.Parse("e2"))
            .ToArray();

        Assert.Equal(new[] { new Move(Square.Parse("e2"), Square.Parse("e3")) }, pawnMoves);
    }

    [Fact]
    public void GeneratePseudoLegalMoves_BlackPawnsMoveAndCaptureInDestinationOrder()
    {
        var position = new Position(
            new PieceBitboards(
                whiteRooks: Mask("c6"),
                whiteKing: Mask("a1"),
                blackPawns: Mask("d7"),
                blackKing: Mask("h8")),
            sideToMove: Color.Black);

        var pawnMoves = Generate(position)
            .Where(move => move.From == Square.Parse("d7"))
            .ToArray();

        Assert.Equal(
            new[]
            {
                new Move(Square.Parse("d7"), Square.Parse("d5"), flags: MoveFlags.DoublePawnPush),
                new Move(Square.Parse("d7"), Square.Parse("c6"), flags: MoveFlags.Capture),
                new Move(Square.Parse("d7"), Square.Parse("d6"))
            },
            pawnMoves);
    }

    [Fact]
    public void GeneratePseudoLegalMoves_ThrowsWhenDestinationSpanIsTooSmall()
    {
        var position = PositionFactory.CreateStandardInitialPosition();
        var tooSmall = new Move[1];

        Assert.Throws<ArgumentException>(() =>
            MoveGenerator.GeneratePseudoLegalMoves(position, tooSmall));
    }

    [Fact]
    public void GeneratePseudoLegalMoves_RejectsNullPosition()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MoveGenerator.GeneratePseudoLegalMoves(null!, new Move[1]));
    }

    private static Move[] Generate(Position position)
    {
        var buffer = new Move[256];
        var count = MoveGenerator.GeneratePseudoLegalMoves(position, buffer);
        return buffer[..count];
    }

    private static ulong Mask(string coordinate) => 1UL << Square.Parse(coordinate).Index;
}
