namespace EngineDemo.Core.Board;

[Flags]
public enum MoveFlags : byte
{
    None = 0,
    Capture = 1,
    DoublePawnPush = 2,
    EnPassant = 4,
    KingSideCastle = 8,
    QueenSideCastle = 16
}

/// <summary>
/// A compact, immutable description of a chess move. The default value is invalid.
/// </summary>
public readonly record struct Move
{
    private const MoveFlags CastleFlags = MoveFlags.KingSideCastle | MoveFlags.QueenSideCastle;
    private const MoveFlags SupportedFlags =
        MoveFlags.Capture
        | MoveFlags.DoublePawnPush
        | MoveFlags.EnPassant
        | CastleFlags;

    public Move(
        Square from,
        Square to,
        PieceType? promotion = null,
        MoveFlags flags = MoveFlags.None)
    {
        if (!from.IsValid)
        {
            throw new ArgumentException("Move source must be a valid square.", nameof(from));
        }

        if (!to.IsValid)
        {
            throw new ArgumentException("Move destination must be a valid square.", nameof(to));
        }

        if (from == to)
        {
            throw new ArgumentException("Move source and destination must differ.", nameof(to));
        }

        if (!IsValidPromotion(promotion))
        {
            throw new ArgumentOutOfRangeException(
                nameof(promotion),
                promotion,
                "Promotion must be a knight, bishop, rook, or queen.");
        }

        if ((flags & ~SupportedFlags) != MoveFlags.None)
        {
            throw new ArgumentOutOfRangeException(nameof(flags), flags, "Move flags contain unsupported values.");
        }

        if (!AreFlagsCompatible(flags, promotion))
        {
            throw new ArgumentException("Move flags and promotion piece are incompatible.", nameof(flags));
        }

        From = from;
        To = to;
        Promotion = promotion;
        Flags = flags;
    }

    public Square From { get; }

    public Square To { get; }

    public PieceType? Promotion { get; }

    public MoveFlags Flags { get; }

    public bool IsValid =>
        From.IsValid
        && To.IsValid
        && From != To
        && IsValidPromotion(Promotion)
        && (Flags & ~SupportedFlags) == MoveFlags.None
        && AreFlagsCompatible(Flags, Promotion);

    private static bool IsValidPromotion(PieceType? promotion) =>
        !promotion.HasValue
        || promotion.Value is PieceType.Knight
            or PieceType.Bishop
            or PieceType.Rook
            or PieceType.Queen;

    private static bool AreFlagsCompatible(MoveFlags flags, PieceType? promotion)
    {
        var castleFlags = flags & CastleFlags;
        var isCastle = castleFlags != MoveFlags.None;

        if (castleFlags == CastleFlags)
        {
            return false;
        }

        if (isCastle && (flags != castleFlags || promotion.HasValue))
        {
            return false;
        }

        if ((flags & MoveFlags.EnPassant) != 0
            && ((flags & MoveFlags.Capture) == 0 || promotion.HasValue))
        {
            return false;
        }

        if ((flags & MoveFlags.DoublePawnPush) != 0
            && ((flags & (MoveFlags.Capture | MoveFlags.EnPassant | CastleFlags)) != 0
                || promotion.HasValue))
        {
            return false;
        }

        return true;
    }
}
