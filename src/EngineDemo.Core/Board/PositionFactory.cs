namespace EngineDemo.Core.Board;

public static class PositionFactory
{
    private const CastlingRights AllCastlingRights =
        CastlingRights.WhiteKingSide
        | CastlingRights.WhiteQueenSide
        | CastlingRights.BlackKingSide
        | CastlingRights.BlackQueenSide;

    public static Position CreateStandardInitialPosition() =>
        new(
            new PieceBitboards(
                whitePawns: 0x000000000000FF00UL,
                whiteKnights: 0x0000000000000042UL,
                whiteBishops: 0x0000000000000024UL,
                whiteRooks: 0x0000000000000081UL,
                whiteQueens: 0x0000000000000008UL,
                whiteKing: 0x0000000000000010UL,
                blackPawns: 0x00FF000000000000UL,
                blackKnights: 0x4200000000000000UL,
                blackBishops: 0x2400000000000000UL,
                blackRooks: 0x8100000000000000UL,
                blackQueens: 0x0800000000000000UL,
                blackKing: 0x1000000000000000UL),
            sideToMove: Color.White,
            castlingRights: AllCastlingRights,
            enPassantTarget: null,
            halfmoveClock: 0,
            fullmoveNumber: 1);
}
