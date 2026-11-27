using System.Globalization;
using System.Text;

namespace EngineDemo.Core.Board;

public static class FenSerializer
{
    public static string Serialize(Position position)
    {
        ArgumentNullException.ThrowIfNull(position);

        var builder = new StringBuilder();
        AppendPiecePlacement(builder, position.Pieces);
        builder.Append(' ')
            .Append(position.SideToMove switch
            {
                Color.White => 'w',
                Color.Black => 'b',
                _ => throw new InvalidOperationException("Position contains an unsupported side to move.")
            })
            .Append(' ')
            .Append(SerializeCastlingRights(position.CastlingRights))
            .Append(' ')
            .Append(position.EnPassantTarget?.ToString() ?? "-")
            .Append(' ')
            .Append(position.HalfmoveClock.ToString(CultureInfo.InvariantCulture))
            .Append(' ')
            .Append(position.FullmoveNumber.ToString(CultureInfo.InvariantCulture));

        return builder.ToString();
    }

    private static void AppendPiecePlacement(StringBuilder builder, PieceBitboards pieces)
    {
        for (var rank = 7; rank >= 0; rank--)
        {
            if (rank < 7)
            {
                builder.Append('/');
            }

            var emptySquares = 0;
            for (var file = 0; file < 8; file++)
            {
                var square = new Square((rank * 8) + file);
                var piece = GetPieceAt(pieces, square);
                if (piece.HasValue)
                {
                    if (emptySquares > 0)
                    {
                        builder.Append(emptySquares);
                        emptySquares = 0;
                    }

                    builder.Append(piece.Value.ToFenSymbol());
                }
                else
                {
                    emptySquares++;
                }
            }

            if (emptySquares > 0)
            {
                builder.Append(emptySquares);
            }
        }
    }

    private static Piece? GetPieceAt(PieceBitboards pieces, Square square)
    {
        if (Bitboard.IsSet(pieces.WhitePawns, square)) return new Piece(Color.White, PieceType.Pawn);
        if (Bitboard.IsSet(pieces.WhiteKnights, square)) return new Piece(Color.White, PieceType.Knight);
        if (Bitboard.IsSet(pieces.WhiteBishops, square)) return new Piece(Color.White, PieceType.Bishop);
        if (Bitboard.IsSet(pieces.WhiteRooks, square)) return new Piece(Color.White, PieceType.Rook);
        if (Bitboard.IsSet(pieces.WhiteQueens, square)) return new Piece(Color.White, PieceType.Queen);
        if (Bitboard.IsSet(pieces.WhiteKing, square)) return new Piece(Color.White, PieceType.King);
        if (Bitboard.IsSet(pieces.BlackPawns, square)) return new Piece(Color.Black, PieceType.Pawn);
        if (Bitboard.IsSet(pieces.BlackKnights, square)) return new Piece(Color.Black, PieceType.Knight);
        if (Bitboard.IsSet(pieces.BlackBishops, square)) return new Piece(Color.Black, PieceType.Bishop);
        if (Bitboard.IsSet(pieces.BlackRooks, square)) return new Piece(Color.Black, PieceType.Rook);
        if (Bitboard.IsSet(pieces.BlackQueens, square)) return new Piece(Color.Black, PieceType.Queen);
        if (Bitboard.IsSet(pieces.BlackKing, square)) return new Piece(Color.Black, PieceType.King);

        return null;
    }

    private static string SerializeCastlingRights(CastlingRights rights)
    {
        if (rights == CastlingRights.None)
        {
            return "-";
        }

        var builder = new StringBuilder(4);
        if ((rights & CastlingRights.WhiteKingSide) != 0) builder.Append('K');
        if ((rights & CastlingRights.WhiteQueenSide) != 0) builder.Append('Q');
        if ((rights & CastlingRights.BlackKingSide) != 0) builder.Append('k');
        if ((rights & CastlingRights.BlackQueenSide) != 0) builder.Append('q');
        return builder.ToString();
    }
}
