namespace EngineDemo.Core.Board;

public readonly record struct Square
{
    private readonly byte _encodedIndex;

    public Square(int index)
    {
        if ((uint)index >= 64)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "A square index must be between 0 and 63.");
        }

        _encodedIndex = (byte)(index + 1);
    }

    public bool IsValid => _encodedIndex != 0;

    public int Index => IsValid
        ? _encodedIndex - 1
        : throw new InvalidOperationException("The default square value is invalid.");

    public int File => Index % 8;

    public int Rank => Index / 8;

    public static Square Parse(string? coordinate)
    {
        if (TryParse(coordinate, out var square))
        {
            return square;
        }

        throw new FormatException("A square coordinate must be lowercase and between a1 and h8.");
    }

    public static bool TryParse(string? coordinate, out Square square)
    {
        square = default;

        if (coordinate is null || coordinate.Length != 2)
        {
            return false;
        }

        var file = coordinate[0] - 'a';
        var rank = coordinate[1] - '1';

        if ((uint)file >= 8 || (uint)rank >= 8)
        {
            return false;
        }

        square = new Square(rank * 8 + file);
        return true;
    }

    public override string ToString()
    {
        var index = Index;
        var file = (char)('a' + index % 8);
        var rank = (char)('1' + index / 8);
        return $"{file}{rank}";
    }
}
