using System.Globalization;
using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class FenSerializerTests
{
    [Fact]
    public void Serialize_StandardInitialPosition_ReturnsCanonicalFen()
    {
        var position = PositionFactory.CreateStandardInitialPosition();

        var fen = FenSerializer.Serialize(position);

        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", fen);
        Assert.Equal(6, fen.Split(' ').Length);
    }

    [Fact]
    public void Serialize_SparsePosition_CompressesEmptySquaresAndPreservesOrientation()
    {
        var position = FenParser.Parse("r3k2r/8/8/8/8/8/8/R3K2R b QkKq e3 12 34");

        var fen = FenSerializer.Serialize(position);

        Assert.Equal("r3k2r/8/8/8/8/8/8/R3K2R b KQkq e3 12 34", fen);
    }

    [Theory]
    [InlineData("QkKq", "KQkq")]
    [InlineData("qK", "Kq")]
    [InlineData("-", "-")]
    public void Serialize_CastlingRights_UsesCanonicalOrder(string rights, string expectedRights)
    {
        var position = FenParser.Parse($"4k3/8/8/8/8/8/8/4K3 w {rights} - 0 1");

        var fen = FenSerializer.Serialize(position);

        Assert.Equal($"4k3/8/8/8/8/8/8/4K3 w {expectedRights} - 0 1", fen);
    }

    [Theory]
    [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
    [InlineData("4k3/8/8/3pP3/8/8/8/4K3 b Kq e3 7 42")]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - e6 1234 5678")]
    public void Serialize_RoundTripsPositionAndHash(string sourceFen)
    {
        var position = FenParser.Parse(sourceFen);

        var fen = FenSerializer.Serialize(position);
        var roundTripped = FenParser.Parse(fen);

        Assert.Equal(position, roundTripped);
        Assert.Equal(position.Hash, roundTripped.Hash);
        Assert.Equal(fen, FenSerializer.Serialize(position));
    }

    [Fact]
    public void Serialize_UsesInvariantCounterFormatting()
    {
        var position = FenParser.Parse("4k3/8/8/8/8/8/8/4K3 w - - 1234 5678");
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-EG");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ar-EG");

            var fen = FenSerializer.Serialize(position);

            Assert.Equal("4k3/8/8/8/8/8/8/4K3 w - - 1234 5678", fen);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void Serialize_RejectsNullPosition()
    {
        Assert.Throws<ArgumentNullException>(() => FenSerializer.Serialize(null!));
    }
}
