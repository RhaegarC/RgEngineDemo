using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class AttackGeneratorTests
{
    private static readonly (int File, int Rank)[] RookDirections =
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    private static readonly (int File, int Rank)[] BishopDirections =
    {
        (1, 1), (1, -1), (-1, 1), (-1, -1)
    };

    [Fact]
    public void PawnAttacks_RespectColorAndBoardEdges()
    {
        Assert.Equal(BitboardOf(34, 36), AttackGenerator.PawnAttacks(Color.White, new Square(27)));
        Assert.Equal(BitboardOf(18, 20), AttackGenerator.PawnAttacks(Color.Black, new Square(27)));
        Assert.Equal(BitboardOf(57), AttackGenerator.PawnAttacks(Color.White, new Square(48)));
        Assert.Equal(BitboardOf(5, 7), AttackGenerator.PawnAttacks(Color.Black, new Square(14)));
    }

    [Fact]
    public void KnightAttacks_RespectBoardEdgesAndCenter()
    {
        Assert.Equal(
            BitboardOf(10, 12, 17, 21, 33, 37, 42, 44),
            AttackGenerator.KnightAttacks(new Square(27)));
        Assert.Equal(BitboardOf(10, 17), AttackGenerator.KnightAttacks(new Square(0)));
        Assert.Equal(BitboardOf(46, 53), AttackGenerator.KnightAttacks(new Square(63)));
    }

    [Fact]
    public void KingAttacks_RespectBoardEdgesAndCenter()
    {
        Assert.Equal(BitboardOf(18, 19, 20, 26, 28, 34, 35, 36), AttackGenerator.KingAttacks(new Square(27)));
        Assert.Equal(BitboardOf(1, 8, 9), AttackGenerator.KingAttacks(new Square(0)));
        Assert.Equal(BitboardOf(54, 55, 62), AttackGenerator.KingAttacks(new Square(63)));
    }

    [Fact]
    public void RookAttacks_IncludeTheFirstBlockerAndStopBeyondIt()
    {
        var occupancy = BitboardOf(2, 3, 16, 24);

        Assert.Equal(BitboardOf(1, 2, 8, 16), AttackGenerator.RookAttacks(new Square(0), occupancy));
    }

    [Fact]
    public void BishopAttacks_IncludeTheFirstBlockerAndContinueOnOtherRays()
    {
        var occupancy = BitboardOf(45);

        Assert.Equal(
            BitboardOf(0, 6, 9, 13, 18, 20, 34, 36, 41, 45, 48),
            AttackGenerator.BishopAttacks(new Square(27), occupancy));
    }

    [Fact]
    public void QueenAttacks_AreTheUnionOfRookAndBishopAttacks()
    {
        const int squareIndex = 27;
        var occupancy = BitboardOf(45, 29, 12, 35);

        Assert.Equal(
            AttackGenerator.RookAttacks(new Square(squareIndex), occupancy)
            | AttackGenerator.BishopAttacks(new Square(squareIndex), occupancy),
            AttackGenerator.QueenAttacks(new Square(squareIndex), occupancy));
    }

    [Theory]
    [InlineData(PieceType.Pawn, 27, 36)]
    [InlineData(PieceType.Knight, 27, 44)]
    [InlineData(PieceType.Bishop, 27, 54)]
    [InlineData(PieceType.Rook, 27, 59)]
    [InlineData(PieceType.Queen, 27, 63)]
    [InlineData(PieceType.King, 27, 36)]
    public void IsSquareAttacked_RecognizesEachWhitePieceType(PieceType type, int from, int target)
    {
        var pieces = WhiteAttackerPosition(type, from, target);
        var position = new Position(pieces);

        Assert.True(AttackGenerator.IsSquareAttacked(position, new Square(target), Color.White));
    }

    [Fact]
    public void IsSquareAttacked_UsesTheRequestedAttackingColor()
    {
        var pieces = new PieceBitboards(
            whiteKing: BitboardOf(28),
            blackPawns: BitboardOf(35),
            blackKing: BitboardOf(56));
        var position = new Position(pieces);

        Assert.True(AttackGenerator.IsSquareAttacked(position, new Square(28), Color.Black));
        Assert.False(AttackGenerator.IsSquareAttacked(position, new Square(28), Color.White));

        var sliderPosition = new Position(new PieceBitboards(
            whiteKing: BitboardOf(3),
            blackRooks: BitboardOf(27),
            blackKing: BitboardOf(63)));

        Assert.True(AttackGenerator.IsSquareAttacked(sliderPosition, new Square(3), Color.Black));
    }

    [Fact]
    public void IsSquareAttacked_CountsAttacksFromPinnedPieces()
    {
        var pieces = new PieceBitboards(
            whiteRooks: BitboardOf(4),
            whiteKing: BitboardOf(0),
            blackRooks: BitboardOf(52),
            blackKing: BitboardOf(60));
        var position = new Position(pieces);

        Assert.True(AttackGenerator.IsSquareAttacked(position, new Square(44), Color.Black));
    }

    [Fact]
    public void AttackQueries_RejectInvalidSquaresAndColors()
    {
        Assert.Throws<ArgumentException>(() => AttackGenerator.KnightAttacks(default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AttackGenerator.PawnAttacks((Color)2, new Square(0)));
        Assert.Throws<ArgumentException>(() =>
            AttackGenerator.IsSquareAttacked(new Position(new PieceBitboards(
                whiteKing: BitboardOf(0),
                blackKing: BitboardOf(63))), default, Color.White));
    }

    [Fact]
    public void SlidingAttacks_MatchRayTracingForEveryRelevantOccupancySubset()
    {
        for (var squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            AssertSlidingAttacksMatchReference(squareIndex, RookDirections, AttackGenerator.RookAttacks);
            AssertSlidingAttacksMatchReference(squareIndex, BishopDirections, AttackGenerator.BishopAttacks);
        }
    }

    private static void AssertSlidingAttacksMatchReference(
        int squareIndex,
        (int File, int Rank)[] directions,
        Func<Square, ulong, ulong> getAttacks)
    {
        var blockerMask = RelevantBlockers(squareIndex, directions);
        var occupancy = 0UL;

        do
        {
            var square = new Square(squareIndex);
            var expected = RayAttacks(squareIndex, occupancy, directions);
            var actual = getAttacks(square, occupancy);

            Assert.True(
                expected == actual,
                $"Attack mismatch at {square}, occupancy 0x{occupancy:X16}: expected 0x{expected:X16}, got 0x{actual:X16}.");

            occupancy = (occupancy - blockerMask) & blockerMask;
        }
        while (occupancy != 0);
    }

    private static ulong RelevantBlockers(int squareIndex, (int File, int Rank)[] directions)
    {
        var file = squareIndex % 8;
        var rank = squareIndex / 8;
        var mask = 0UL;

        foreach (var (fileDelta, rankDelta) in directions)
        {
            var currentFile = file + fileDelta;
            var currentRank = rank + rankDelta;
            while (Inside(currentFile, currentRank))
            {
                var nextFile = currentFile + fileDelta;
                var nextRank = currentRank + rankDelta;
                if (!Inside(nextFile, nextRank))
                {
                    break;
                }

                mask |= 1UL << (currentRank * 8 + currentFile);
                currentFile = nextFile;
                currentRank = nextRank;
            }
        }

        return mask;
    }

    private static ulong RayAttacks(
        int squareIndex,
        ulong occupancy,
        (int File, int Rank)[] directions)
    {
        var file = squareIndex % 8;
        var rank = squareIndex / 8;
        var attacks = 0UL;

        foreach (var (fileDelta, rankDelta) in directions)
        {
            var targetFile = file + fileDelta;
            var targetRank = rank + rankDelta;
            while (Inside(targetFile, targetRank))
            {
                var target = 1UL << (targetRank * 8 + targetFile);
                attacks |= target;
                if ((occupancy & target) != 0)
                {
                    break;
                }

                targetFile += fileDelta;
                targetRank += rankDelta;
            }
        }

        return attacks;
    }

    private static bool Inside(int file, int rank) => (uint)file < 8 && (uint)rank < 8;

    private static PieceBitboards WhiteAttackerPosition(PieceType type, int from, int target)
    {
        var whitePawns = type == PieceType.Pawn ? BitboardOf(from) : 0UL;
        var whiteKnights = type == PieceType.Knight ? BitboardOf(from) : 0UL;
        var whiteBishops = type == PieceType.Bishop ? BitboardOf(from) : 0UL;
        var whiteRooks = type == PieceType.Rook ? BitboardOf(from) : 0UL;
        var whiteQueens = type == PieceType.Queen ? BitboardOf(from) : 0UL;
        var whiteKing = type == PieceType.King ? BitboardOf(from) : BitboardOf(0);
        var blackKing = type == PieceType.King ? BitboardOf(target) : BitboardOf(63);

        return new PieceBitboards(
            whitePawns,
            whiteKnights,
            whiteBishops,
            whiteRooks,
            whiteQueens,
            whiteKing,
            blackKing: blackKing);
    }

    private static ulong BitboardOf(params int[] squares)
    {
        var result = 0UL;

        foreach (var square in squares)
        {
            result = Bitboard.Set(result, new Square(square));
        }

        return result;
    }
}
