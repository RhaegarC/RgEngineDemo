namespace EngineDemo.Core.Board;

public enum Color : byte
{
    White,
    Black
}

public enum PieceType : byte
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public readonly record struct Piece
{
    public Piece(Color color, PieceType type)
    {
        if (color is not Color.White and not Color.Black)
        {
            throw new ArgumentOutOfRangeException(nameof(color), color, "Color must be White or Black.");
        }

        if (type is not PieceType.Pawn
            and not PieceType.Knight
            and not PieceType.Bishop
            and not PieceType.Rook
            and not PieceType.Queen
            and not PieceType.King)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Piece type is not supported.");
        }

        Color = color;
        Type = type;
    }

    public Color Color { get; }

    public PieceType Type { get; }

    public char ToFenSymbol() => (Color, Type) switch
    {
        (Color.White, PieceType.Pawn) => 'P',
        (Color.White, PieceType.Knight) => 'N',
        (Color.White, PieceType.Bishop) => 'B',
        (Color.White, PieceType.Rook) => 'R',
        (Color.White, PieceType.Queen) => 'Q',
        (Color.White, PieceType.King) => 'K',
        (Color.Black, PieceType.Pawn) => 'p',
        (Color.Black, PieceType.Knight) => 'n',
        (Color.Black, PieceType.Bishop) => 'b',
        (Color.Black, PieceType.Rook) => 'r',
        (Color.Black, PieceType.Queen) => 'q',
        (Color.Black, PieceType.King) => 'k',
        _ => throw new InvalidOperationException("Piece contains an unsupported color or type.")
    };

    public static Piece FromFenSymbol(char symbol) => symbol switch
    {
        'P' => new Piece(Color.White, PieceType.Pawn),
        'N' => new Piece(Color.White, PieceType.Knight),
        'B' => new Piece(Color.White, PieceType.Bishop),
        'R' => new Piece(Color.White, PieceType.Rook),
        'Q' => new Piece(Color.White, PieceType.Queen),
        'K' => new Piece(Color.White, PieceType.King),
        'p' => new Piece(Color.Black, PieceType.Pawn),
        'n' => new Piece(Color.Black, PieceType.Knight),
        'b' => new Piece(Color.Black, PieceType.Bishop),
        'r' => new Piece(Color.Black, PieceType.Rook),
        'q' => new Piece(Color.Black, PieceType.Queen),
        'k' => new Piece(Color.Black, PieceType.King),
        _ => throw new FormatException($"'{symbol}' is not a standard FEN piece symbol.")
    };
}
