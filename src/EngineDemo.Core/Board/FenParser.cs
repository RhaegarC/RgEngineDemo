using System.Globalization;

namespace EngineDemo.Core.Board;

public static class FenParser
{
    public static Position Parse(string fen)
    {
        ArgumentNullException.ThrowIfNull(fen);

        var fields = fen.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 6)
        {
            throw Invalid("FEN", "exactly six fields are required.");
        }

        var pieces = ParsePiecePlacement(fields[0]);
        var sideToMove = ParseSideToMove(fields[1]);
        var castlingRights = ParseCastlingRights(fields[2]);
        var enPassantTarget = ParseEnPassantTarget(fields[3], sideToMove);
        var halfmoveClock = ParseCounter(fields[4], "halfmove clock", 0);
        var fullmoveNumber = ParseCounter(fields[5], "fullmove number", 1);

        try
        {
            return new Position(
                pieces,
                sideToMove,
                castlingRights,
                enPassantTarget,
                halfmoveClock,
                fullmoveNumber);
        }
        catch (ArgumentException exception)
        {
            throw Invalid("position", exception.Message, exception);
        }
    }

    private static PieceBitboards ParsePiecePlacement(string placement)
    {
        var ranks = placement.Split('/');
        if (ranks.Length != 8)
        {
            throw Invalid("piece placement", "exactly eight ranks are required.");
        }

        var bitboards = new ulong[12];

        for (var rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
        {
            var fileIndex = 0;
            foreach (var symbol in ranks[rankIndex])
            {
                if (symbol is >= '1' and <= '8')
                {
                    fileIndex += symbol - '0';
                    if (fileIndex > 8)
                    {
                        throw Invalid("piece placement", $"rank {8 - rankIndex} expands beyond eight squares.");
                    }

                    continue;
                }

                Piece piece;
                try
                {
                    piece = Piece.FromFenSymbol(symbol);
                }
                catch (FormatException exception)
                {
                    throw Invalid("piece placement", $"'{symbol}' is not a standard FEN piece symbol.", exception);
                }

                if (fileIndex >= 8)
                {
                    throw Invalid("piece placement", $"rank {8 - rankIndex} expands beyond eight squares.");
                }

                var bitboardIndex = (piece.Color == Color.White ? 0 : 6) + (int)piece.Type;
                var squareIndex = ((7 - rankIndex) * 8) + fileIndex;
                bitboards[bitboardIndex] |= 1UL << squareIndex;
                fileIndex++;
            }

            if (fileIndex != 8)
            {
                throw Invalid("piece placement", $"rank {8 - rankIndex} expands to {fileIndex} squares instead of eight.");
            }
        }

        return new PieceBitboards(
            whitePawns: bitboards[0],
            whiteKnights: bitboards[1],
            whiteBishops: bitboards[2],
            whiteRooks: bitboards[3],
            whiteQueens: bitboards[4],
            whiteKing: bitboards[5],
            blackPawns: bitboards[6],
            blackKnights: bitboards[7],
            blackBishops: bitboards[8],
            blackRooks: bitboards[9],
            blackQueens: bitboards[10],
            blackKing: bitboards[11]);
    }

    private static Color ParseSideToMove(string field) => field switch
    {
        "w" => Color.White,
        "b" => Color.Black,
        _ => throw Invalid("side to move", "only 'w' or 'b' is accepted.")
    };

    private static CastlingRights ParseCastlingRights(string field)
    {
        if (field == "-")
        {
            return CastlingRights.None;
        }

        var rights = CastlingRights.None;
        foreach (var symbol in field)
        {
            var right = symbol switch
            {
                'K' => CastlingRights.WhiteKingSide,
                'Q' => CastlingRights.WhiteQueenSide,
                'k' => CastlingRights.BlackKingSide,
                'q' => CastlingRights.BlackQueenSide,
                _ => throw Invalid("castling rights", $"'{symbol}' is not a supported castling symbol.")
            };

            if ((rights & right) != CastlingRights.None)
            {
                throw Invalid("castling rights", $"'{symbol}' is duplicated.");
            }

            rights |= right;
        }

        return rights;
    }

    private static Square? ParseEnPassantTarget(string field, Color sideToMove)
    {
        if (field == "-")
        {
            return null;
        }

        if (!Square.TryParse(field, out var target))
        {
            throw Invalid("en-passant target", "a lowercase coordinate between a1 and h8 is required.");
        }

        var expectedRank = sideToMove == Color.White ? 5 : 2;
        if (target.Rank != expectedRank)
        {
            var rank = sideToMove == Color.White ? 6 : 3;
            throw Invalid("en-passant target", $"rank {rank} is required for the side to move.");
        }

        return target;
    }

    private static int ParseCounter(string field, string name, int minimum)
    {
        if (!int.TryParse(field, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            || value < minimum)
        {
            throw Invalid(name, $"a decimal integer of at least {minimum} is required.");
        }

        return value;
    }

    private static FormatException Invalid(string field, string detail, Exception? innerException = null) =>
        new($"Invalid FEN {field}: {detail}", innerException);
}
