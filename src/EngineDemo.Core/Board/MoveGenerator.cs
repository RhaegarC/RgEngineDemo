namespace EngineDemo.Core.Board;

public static class MoveGenerator
{
    public static int GeneratePseudoLegalMoves(Position position, Span<Move> destination)
    {
        ArgumentNullException.ThrowIfNull(position);

        var color = position.SideToMove;
        var ownOccupancy = color == Color.White ? position.WhiteOccupancy : position.BlackOccupancy;
        var enemyOccupancy = color == Color.White ? position.BlackOccupancy : position.WhiteOccupancy;
        var enemyKing = color == Color.White ? position.Pieces.BlackKing : position.Pieces.WhiteKing;
        var forbiddenTargets = ownOccupancy | enemyKing;
        var pieces = position.Pieces;
        var occupied = position.TotalOccupancy;
        var moveCount = 0;

        GeneratePawnMoves(
            GetPawns(pieces, color),
            color,
            occupied,
            enemyOccupancy,
            enemyKing,
            destination,
            ref moveCount);

        GeneratePieceMoves(
            GetKnights(pieces, color),
            PieceType.Knight,
            enemyOccupancy,
            forbiddenTargets,
            occupied,
            destination,
            ref moveCount);
        GeneratePieceMoves(
            GetBishops(pieces, color),
            PieceType.Bishop,
            enemyOccupancy,
            forbiddenTargets,
            occupied,
            destination,
            ref moveCount);
        GeneratePieceMoves(
            GetRooks(pieces, color),
            PieceType.Rook,
            enemyOccupancy,
            forbiddenTargets,
            occupied,
            destination,
            ref moveCount);
        GeneratePieceMoves(
            GetQueens(pieces, color),
            PieceType.Queen,
            enemyOccupancy,
            forbiddenTargets,
            occupied,
            destination,
            ref moveCount);
        GeneratePieceMoves(
            GetKing(pieces, color),
            PieceType.King,
            enemyOccupancy,
            forbiddenTargets,
            occupied,
            destination,
            ref moveCount);

        return moveCount;
    }

    private static void GeneratePawnMoves(
        ulong pawns,
        Color color,
        ulong occupied,
        ulong enemyOccupancy,
        ulong enemyKing,
        Span<Move> destination,
        ref int moveCount)
    {
        var direction = color == Color.White ? 8 : -8;
        var startingRank = color == Color.White ? 1 : 6;
        var promotionRank = color == Color.White ? 7 : 0;
        var promotionRankMask = 0xFFUL << (promotionRank * 8);

        while (pawns != 0)
        {
            var fromIndex = System.Numerics.BitOperations.TrailingZeroCount(pawns);
            var from = new Square(fromIndex);
            pawns &= pawns - 1;

            var targets = AttackGenerator.PawnAttacks(color, from)
                & enemyOccupancy
                & ~enemyKing
                & ~promotionRankMask;
            var singlePushIndex = fromIndex + direction;
            if ((uint)singlePushIndex < 64)
            {
                var singlePushMask = 1UL << singlePushIndex;
                if ((occupied & singlePushMask) == 0
                    && singlePushIndex / 8 != promotionRank)
                {
                    targets |= singlePushMask;

                    var doublePushIndex = singlePushIndex + direction;
                    if (from.Rank == startingRank
                        && (uint)doublePushIndex < 64
                        && doublePushIndex / 8 != promotionRank
                        && (occupied & (1UL << doublePushIndex)) == 0)
                    {
                        targets |= 1UL << doublePushIndex;
                    }
                }
            }

            while (targets != 0)
            {
                var toIndex = System.Numerics.BitOperations.TrailingZeroCount(targets);
                var toMask = 1UL << toIndex;
                targets &= targets - 1;

                var flags = (enemyOccupancy & toMask) != 0
                    ? MoveFlags.Capture
                    : Math.Abs(toIndex - fromIndex) == 16
                        ? MoveFlags.DoublePawnPush
                        : MoveFlags.None;
                AddMove(destination, ref moveCount, new Move(from, new Square(toIndex), flags: flags));
            }
        }
    }

    private static void GeneratePieceMoves(
        ulong pieces,
        PieceType pieceType,
        ulong enemyOccupancy,
        ulong forbiddenTargets,
        ulong occupied,
        Span<Move> destination,
        ref int moveCount)
    {
        while (pieces != 0)
        {
            var fromIndex = System.Numerics.BitOperations.TrailingZeroCount(pieces);
            var from = new Square(fromIndex);
            pieces &= pieces - 1;

            var attacks = pieceType switch
            {
                PieceType.Knight => AttackGenerator.KnightAttacks(from),
                PieceType.Bishop => AttackGenerator.BishopAttacks(from, occupied),
                PieceType.Rook => AttackGenerator.RookAttacks(from, occupied),
                PieceType.Queen => AttackGenerator.QueenAttacks(from, occupied),
                PieceType.King => AttackGenerator.KingAttacks(from),
                _ => throw new ArgumentOutOfRangeException(nameof(pieceType), pieceType, "Piece type is not supported.")
            };
            var targets = attacks & ~forbiddenTargets;
            while (targets != 0)
            {
                var toIndex = System.Numerics.BitOperations.TrailingZeroCount(targets);
                var toMask = 1UL << toIndex;
                targets &= targets - 1;
                var flags = (enemyOccupancy & toMask) != 0 ? MoveFlags.Capture : MoveFlags.None;
                AddMove(destination, ref moveCount, new Move(from, new Square(toIndex), flags: flags));
            }
        }
    }

    private static void AddMove(Span<Move> destination, ref int moveCount, Move move)
    {
        if (moveCount == destination.Length)
        {
            throw new ArgumentException("Destination span is too small for all pseudo-legal moves.", nameof(destination));
        }

        destination[moveCount++] = move;
    }

    private static ulong GetPawns(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhitePawns : pieces.BlackPawns;

    private static ulong GetKnights(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhiteKnights : pieces.BlackKnights;

    private static ulong GetBishops(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhiteBishops : pieces.BlackBishops;

    private static ulong GetRooks(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhiteRooks : pieces.BlackRooks;

    private static ulong GetQueens(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhiteQueens : pieces.BlackQueens;

    private static ulong GetKing(PieceBitboards pieces, Color color) =>
        color == Color.White ? pieces.WhiteKing : pieces.BlackKing;
}
