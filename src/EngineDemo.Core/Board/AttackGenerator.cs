using System.Numerics;

namespace EngineDemo.Core.Board;

public static class AttackGenerator
{
    private static readonly (int File, int Rank)[] RookDirections =
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    private static readonly (int File, int Rank)[] BishopDirections =
    {
        (1, 1), (1, -1), (-1, 1), (-1, -1)
    };

    private static readonly (int File, int Rank)[] KnightOffsets =
    {
        (1, 2), (2, 1), (2, -1), (1, -2),
        (-1, -2), (-2, -1), (-2, 1), (-1, 2)
    };

    private static readonly (int File, int Rank)[] KingOffsets =
    {
        (1, 0), (1, 1), (0, 1), (-1, 1),
        (-1, 0), (-1, -1), (0, -1), (1, -1)
    };

    private static readonly ulong[] RookMagics =
    {
        0x0080028120504000UL, 0x3080200080400010UL, 0x0100082001001040UL, 0x1100200410000900UL,
        0x8580080004009A80UL, 0x4080040080010200UL, 0x0580010021800200UL, 0x8100020088502100UL,
        0x0901801040006082UL, 0x2140802000804004UL, 0x1108801000200084UL, 0x0000800800100080UL,
        0x00060008A0120004UL, 0x005A008802001004UL, 0x0010808001005200UL, 0x4D01000080420100UL,
        0x0040008000402090UL, 0x0000888040002000UL, 0x0002130020010140UL, 0x1000220042001008UL,
        0x0108004040040200UL, 0x0420808004000200UL, 0x2000040002015830UL, 0x0000020004006081UL,
        0x8480400280008420UL, 0x02A1020200208040UL, 0x0000200080100080UL, 0x00020072002218C0UL,
        0x2080040080800800UL, 0x0808020080040080UL, 0xA080014400300802UL, 0x0000008200004421UL,
        0x0040004030800080UL, 0x8020100041400129UL, 0x0000401101002002UL, 0x0801812802801000UL,
        0x0209000801000412UL, 0x0410040080800200UL, 0x0830104204000108UL, 0x8814004402000081UL,
        0x9800400080208000UL, 0x2000201000424008UL, 0x84D0002000808019UL, 0x6000400A00220010UL,
        0x2084080004008080UL, 0x0000020004008080UL, 0x0000421001040008UL, 0x1020209410420001UL,
        0x0C00208100420200UL, 0xC000200880400880UL, 0x0000801000200080UL, 0x9010000800108080UL,
        0x000A080080040080UL, 0x0A02008002040080UL, 0x002001A802100400UL, 0x00020C0090630600UL,
        0x1001006241188001UL, 0x0040108820400101UL, 0x00A10041908C2001UL, 0x0010012004081101UL,
        0x0022002008100402UL, 0x0221001A04000829UL, 0x2001000C008A0017UL, 0x0500040080210042UL
    };

    private static readonly ulong[] BishopMagics =
    {
        0x00A0440100490200UL, 0x2104510802009004UL, 0x0004011C01004000UL, 0x00082084210081D0UL,
        0x0001104004000100UL, 0x1002111008042000UL, 0x0A80A21520200084UL, 0x0084C14410A01022UL,
        0x000110610A0C0048UL, 0x0000204800810040UL, 0x4008102080A10400UL, 0x4020843502004186UL,
        0x1000011040204011UL, 0x0000143004104C04UL, 0x8212808208A00400UL, 0x0400002088086820UL,
        0x0612800404080824UL, 0x8210800212022408UL, 0x0404008204081200UL, 0x00482B4404009001UL,
        0x2004000582A04140UL, 0x10B100888080C000UL, 0x5011008088011010UL, 0x0D81060084010102UL,
        0x601010C08420C201UL, 0x0002102008014801UL, 0x000990000A040010UL, 0x8098080090220020UL,
        0x1081010020104000UL, 0x0010010000208810UL, 0xE00344000204A400UL, 0x0041130120404800UL,
        0x0010048401200908UL, 0x0140820842101000UL, 0x4004010100308404UL, 0x04004009A0920200UL,
        0x28040100100C0040UL, 0x880110008001006AUL, 0x0444280083004440UL, 0x0008053500024040UL,
        0x408A022005102000UL, 0xC3020C8298002002UL, 0x0004A020B0004800UL, 0x0002206018000100UL,
        0x0010B02010400200UL, 0x0088200042800410UL, 0x0008300402610489UL, 0x004948009B020080UL,
        0xC010A80802501008UL, 0x0410220802082200UL, 0x8004404200D00210UL, 0x0600042508480000UL,
        0x042200C002820808UL, 0x9080400881010202UL, 0x0241880200C20012UL, 0x40051802040E2000UL,
        0x0001040084010808UL, 0x0008002202100400UL, 0x4000A00494008810UL, 0x2000001000840408UL,
        0x06080201C0082200UL, 0x80081020A4210205UL, 0x111040B06C050042UL, 0x0048100080841080UL
    };

    private static readonly ulong[][] PawnAttackMasks = BuildPawnAttackMasks();
    private static readonly ulong[] KnightAttackMasks = BuildLeaperAttackMasks(KnightOffsets);
    private static readonly ulong[] KingAttackMasks = BuildLeaperAttackMasks(KingOffsets);
    private static readonly MagicAttackTable RookTable = new(RookDirections, RookMagics);
    private static readonly MagicAttackTable BishopTable = new(BishopDirections, BishopMagics);

    public static ulong PawnAttacks(Color color, Square square)
    {
        ValidateColor(color);
        return PawnAttackMasks[(int)color][GetSquareIndex(square)];
    }

    public static ulong KnightAttacks(Square square) => KnightAttackMasks[GetSquareIndex(square)];

    public static ulong KingAttacks(Square square) => KingAttackMasks[GetSquareIndex(square)];

    public static ulong BishopAttacks(Square square, ulong occupied) =>
        BishopTable.GetAttacks(square, occupied);

    public static ulong RookAttacks(Square square, ulong occupied) =>
        RookTable.GetAttacks(square, occupied);

    public static ulong QueenAttacks(Square square, ulong occupied) =>
        BishopAttacks(square, occupied) | RookAttacks(square, occupied);

    public static bool IsSquareAttacked(Position position, Square square, Color attackingColor)
    {
        ArgumentNullException.ThrowIfNull(position);
        ValidateColor(attackingColor);
        GetSquareIndex(square);

        var pieces = position.Pieces;
        var occupied = position.TotalOccupancy;
        var queens = GetQueens(pieces, attackingColor);

        return (PawnAttacks(OppositeColor(attackingColor), square) & GetPawns(pieces, attackingColor)) != 0
            || (KnightAttacks(square) & GetKnights(pieces, attackingColor)) != 0
            || (BishopAttacks(square, occupied) & (GetBishops(pieces, attackingColor) | queens)) != 0
            || (RookAttacks(square, occupied) & (GetRooks(pieces, attackingColor) | queens)) != 0
            || (KingAttacks(square) & GetKing(pieces, attackingColor)) != 0;
    }

    private static ulong[][] BuildPawnAttackMasks()
    {
        var attacks = new[] { new ulong[64], new ulong[64] };

        for (var square = 0; square < 64; square++)
        {
            var file = square % 8;
            var rank = square / 8;

            for (var color = 0; color < attacks.Length; color++)
            {
                var rankDelta = color == (int)Color.White ? 1 : -1;
                var targetRank = rank + rankDelta;
                if ((uint)targetRank >= 8)
                {
                    continue;
                }

                if (file > 0)
                {
                    attacks[color][square] |= 1UL << (targetRank * 8 + file - 1);
                }

                if (file < 7)
                {
                    attacks[color][square] |= 1UL << (targetRank * 8 + file + 1);
                }
            }
        }

        return attacks;
    }

    private static ulong[] BuildLeaperAttackMasks((int File, int Rank)[] offsets)
    {
        var attacks = new ulong[64];

        for (var square = 0; square < 64; square++)
        {
            var file = square % 8;
            var rank = square / 8;

            foreach (var (fileDelta, rankDelta) in offsets)
            {
                var targetFile = file + fileDelta;
                var targetRank = rank + rankDelta;
                if ((uint)targetFile < 8 && (uint)targetRank < 8)
                {
                    attacks[square] |= 1UL << (targetRank * 8 + targetFile);
                }
            }
        }

        return attacks;
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

    private static Color OppositeColor(Color color) => color == Color.White ? Color.Black : Color.White;

    private static int GetSquareIndex(Square square)
    {
        if (!square.IsValid)
        {
            throw new ArgumentException("Square must be valid.", nameof(square));
        }

        return square.Index;
    }

    private static void ValidateColor(Color color)
    {
        if (color is not Color.White and not Color.Black)
        {
            throw new ArgumentOutOfRangeException(nameof(color), color, "Color must be White or Black.");
        }
    }

    private sealed class MagicAttackTable
    {
        private readonly ulong[] _attacks;
        private readonly Entry[] _entries;

        public MagicAttackTable((int File, int Rank)[] directions, ulong[] magics)
        {
            _entries = new Entry[64];
            var totalAttacks = 0;

            for (var square = 0; square < 64; square++)
            {
                var mask = BuildRelevantOccupancyMask(square, directions);
                var bits = BitOperations.PopCount(mask);
                var tableSize = 1 << bits;
                _entries[square] = new Entry(mask, magics[square], 64 - bits, totalAttacks, tableSize);
                totalAttacks += tableSize;
            }

            _attacks = new ulong[totalAttacks];

            for (var square = 0; square < 64; square++)
            {
                InitializeSquare(square, directions, _entries[square]);
            }
        }

        public ulong GetAttacks(Square square, ulong occupied)
        {
            var entry = _entries[GetSquareIndex(square)];
            var blockers = occupied & entry.Mask;
            var index = (int)(unchecked(blockers * entry.Magic) >> entry.Shift);
            return _attacks[entry.Offset + index];
        }

        private void InitializeSquare(
            int square,
            (int File, int Rank)[] directions,
            Entry entry)
        {
            var written = new bool[entry.TableSize];
            var occupancy = 0UL;

            do
            {
                var index = (int)(unchecked(occupancy * entry.Magic) >> entry.Shift);
                var attacks = CalculateSlidingAttacks(square, occupancy, directions);
                var tableIndex = entry.Offset + index;

                if (written[index] && _attacks[tableIndex] != attacks)
                {
                    throw new InvalidOperationException($"Invalid sliding attack magic for square {square}.");
                }

                written[index] = true;
                _attacks[tableIndex] = attacks;
                occupancy = (occupancy - entry.Mask) & entry.Mask;
            }
            while (occupancy != 0);
        }

        private static ulong BuildRelevantOccupancyMask(
            int square,
            (int File, int Rank)[] directions)
        {
            var file = square % 8;
            var rank = square / 8;
            var mask = 0UL;

            foreach (var (fileDelta, rankDelta) in directions)
            {
                var targetFile = file + fileDelta;
                var targetRank = rank + rankDelta;

                while (InsideBoard(targetFile, targetRank))
                {
                    var nextFile = targetFile + fileDelta;
                    var nextRank = targetRank + rankDelta;
                    if (!InsideBoard(nextFile, nextRank))
                    {
                        break;
                    }

                    mask |= 1UL << (targetRank * 8 + targetFile);
                    targetFile = nextFile;
                    targetRank = nextRank;
                }
            }

            return mask;
        }

        private static ulong CalculateSlidingAttacks(
            int square,
            ulong occupied,
            (int File, int Rank)[] directions)
        {
            var file = square % 8;
            var rank = square / 8;
            var attacks = 0UL;

            foreach (var (fileDelta, rankDelta) in directions)
            {
                var targetFile = file + fileDelta;
                var targetRank = rank + rankDelta;

                while (InsideBoard(targetFile, targetRank))
                {
                    var target = 1UL << (targetRank * 8 + targetFile);
                    attacks |= target;
                    if ((occupied & target) != 0)
                    {
                        break;
                    }

                    targetFile += fileDelta;
                    targetRank += rankDelta;
                }
            }

            return attacks;
        }

        private static bool InsideBoard(int file, int rank) =>
            (uint)file < 8 && (uint)rank < 8;

        private readonly record struct Entry(
            ulong Mask,
            ulong Magic,
            int Shift,
            int Offset,
            int TableSize);
    }
}
