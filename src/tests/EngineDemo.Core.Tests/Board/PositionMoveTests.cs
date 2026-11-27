using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class PositionMoveTests
{
    [Fact]
    public void Constructor_SeedsPositionHistoryWithInitialHash()
    {
        var position = CreatePosition(
            new PieceBitboards(whiteKing: Mask("e1"), blackKing: Mask("e8")));

        Assert.Equal(new[] { position.Hash }, position.PositionHistory);
    }

    [Fact]
    public void MakeMove_UpdatesQuietMoveStateAndHistory()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whiteKnights: Mask("g1"),
                whiteKing: Mask("e1"),
                blackKing: Mask("e8")),
            enPassantTarget: Square.Parse("d6"),
            halfmoveClock: 4,
            fullmoveNumber: 3);
        var previousHash = position.Hash;

        var undoState = position.MakeMove(new Move(Square.Parse("g1"), Square.Parse("f3")));

        Assert.Equal(0UL, position.Pieces.WhiteKnights & Mask("g1"));
        Assert.NotEqual(0UL, position.Pieces.WhiteKnights & Mask("f3"));
        Assert.Equal(Color.Black, position.SideToMove);
        Assert.Equal(5, position.HalfmoveClock);
        Assert.Equal(3, position.FullmoveNumber);
        Assert.Null(position.EnPassantTarget);
        Assert.Equal(previousHash, undoState.PreviousHash);
        Assert.Equal(position.Hash, PositionHash.Compute(position));
        Assert.Equal(new[] { previousHash, position.Hash }, position.PositionHistory);
        Assert.Equal(position.Pieces.WhiteOccupancy, position.WhiteOccupancy);
        Assert.Equal(position.Pieces.BlackOccupancy, position.BlackOccupancy);
        Assert.Equal(position.WhiteOccupancy | position.BlackOccupancy, position.TotalOccupancy);
    }

    [Theory]
    [InlineData(Color.White, "e2", "e4", "e3")]
    [InlineData(Color.Black, "e7", "e5", "e6")]
    public void MakeMove_PawnDoublePushSetsEnPassantTargetAndResetsClock(
        Color sideToMove,
        string from,
        string to,
        string enPassantTarget)
    {
        var pawnSquare = Square.Parse(from);
        var pieces = sideToMove == Color.White
            ? new PieceBitboards(whitePawns: Mask(from), whiteKing: Mask("a1"), blackKing: Mask("h8"))
            : new PieceBitboards(blackPawns: Mask(from), whiteKing: Mask("a1"), blackKing: Mask("h8"));
        var position = CreatePosition(
            pieces,
            sideToMove: sideToMove,
            halfmoveClock: 26,
            fullmoveNumber: 8);

        var undoState = position.MakeMove(
            new Move(pawnSquare, Square.Parse(to), flags: MoveFlags.DoublePawnPush));

        Assert.Equal(Square.Parse(enPassantTarget), position.EnPassantTarget);
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(sideToMove == Color.White ? Color.Black : Color.White, position.SideToMove);
        Assert.Equal(sideToMove == Color.Black ? 9 : 8, position.FullmoveNumber);
        Assert.Equal(position.Hash, PositionHash.Compute(position));
        Assert.Equal(position.Hash, position.PositionHistory[^1]);
        Assert.Equal(sideToMove, undoState.PreviousSideToMove);
    }

    [Fact]
    public void MakeMove_CaptureStoresCapturedPieceAndResetsClock()
    {
        var capturedPiece = new Piece(Color.Black, PieceType.Knight);
        var position = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("a2"),
                whiteKing: Mask("e1"),
                blackKnights: Mask("a7"),
                blackKing: Mask("e8")),
            halfmoveClock: 22,
            fullmoveNumber: 5);

        var undoState = position.MakeMove(
            new Move(Square.Parse("a2"), Square.Parse("a7"), flags: MoveFlags.Capture));

        Assert.Equal(capturedPiece, undoState.CapturedPiece);
        Assert.Equal(Square.Parse("a7"), undoState.CapturedSquare);
        Assert.Equal(0UL, position.Pieces.BlackKnights);
        Assert.NotEqual(0UL, position.Pieces.WhiteRooks & Mask("a7"));
        Assert.Equal(0, position.HalfmoveClock);
        Assert.Equal(5, position.FullmoveNumber);
        Assert.Equal(position.Hash, PositionHash.Compute(position));
        Assert.Equal(position.Pieces.WhiteOccupancy, position.WhiteOccupancy);
        Assert.Equal(position.Pieces.BlackOccupancy, position.BlackOccupancy);
        Assert.Equal(position.Pieces.TotalOccupancy, position.TotalOccupancy);
    }

    [Fact]
    public void MakeMove_BlackMoveAdvancesFullmoveNumber()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whiteKing: Mask("a1"),
                blackKnights: Mask("g8"),
                blackKing: Mask("h8")),
            sideToMove: Color.Black,
            halfmoveClock: 7,
            fullmoveNumber: 12);

        position.MakeMove(new Move(Square.Parse("g8"), Square.Parse("f6")));

        Assert.Equal(Color.White, position.SideToMove);
        Assert.Equal(13, position.FullmoveNumber);
        Assert.Equal(8, position.HalfmoveClock);
    }

    [Fact]
    public void MakeMove_ClearsCastlingRightsForKingRookMovesAndHomeRookCapture()
    {
        var whiteKingRights = CastlingRights.WhiteKingSide | CastlingRights.WhiteQueenSide;
        var kingPosition = CreatePosition(
            new PieceBitboards(whiteKing: Mask("e1"), blackKing: Mask("e8")),
            castlingRights: whiteKingRights);
        kingPosition.MakeMove(new Move(Square.Parse("e1"), Square.Parse("e2")));
        Assert.Equal(CastlingRights.None, kingPosition.CastlingRights & whiteKingRights);

        var rookPosition = CreatePosition(
            new PieceBitboards(whiteRooks: Mask("a1"), whiteKing: Mask("e1"), blackKing: Mask("e8")),
            castlingRights: whiteKingRights);
        rookPosition.MakeMove(new Move(Square.Parse("a1"), Square.Parse("a2")));
        Assert.Equal(CastlingRights.WhiteKingSide, rookPosition.CastlingRights);

        var blackKingSide = CastlingRights.BlackKingSide;
        var capturePosition = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("h1"),
                whiteKing: Mask("a1"),
                blackRooks: Mask("h8"),
                blackKing: Mask("e8")),
            castlingRights: blackKingSide);
        capturePosition.MakeMove(
            new Move(Square.Parse("h1"), Square.Parse("h8"), flags: MoveFlags.Capture));
        Assert.Equal(CastlingRights.None, capturePosition.CastlingRights & blackKingSide);
    }

    [Fact]
    public void UnmakeMove_RestoresQuietMoveAndPositionHistory()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whitePawns: Mask("e2"),
                blackPawns: Mask("d7"),
                whiteKing: Mask("e1"),
                blackKing: Mask("e8")),
            castlingRights: CastlingRights.WhiteKingSide | CastlingRights.BlackQueenSide,
            halfmoveClock: 0,
            fullmoveNumber: 1);
        var before = Snapshot(position);
        var whiteMove = new Move(Square.Parse("e2"), Square.Parse("e4"), flags: MoveFlags.DoublePawnPush);
        var blackMove = new Move(Square.Parse("d7"), Square.Parse("d5"), flags: MoveFlags.DoublePawnPush);

        var whiteUndo = position.MakeMove(whiteMove);
        var blackUndo = position.MakeMove(blackMove);
        position.UnmakeMove(blackMove, blackUndo);
        position.UnmakeMove(whiteMove, whiteUndo);

        AssertMatches(before, position);
    }

    [Fact]
    public void UnmakeMove_RestoresCaptureAndExactHash()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("a2"),
                whiteKing: Mask("e1"),
                blackKnights: Mask("a7"),
                blackKing: Mask("e8")),
            castlingRights: CastlingRights.BlackKingSide,
            enPassantTarget: Square.Parse("d6"),
            halfmoveClock: 19,
            fullmoveNumber: 4);
        var before = Snapshot(position);
        var move = new Move(Square.Parse("a2"), Square.Parse("a7"), flags: MoveFlags.Capture);

        var undoState = position.MakeMove(move);
        position.UnmakeMove(move, undoState);

        AssertMatches(before, position);
        Assert.Equal(position.Hash, PositionHash.Compute(position));
    }

    [Fact]
    public void UnmakeMove_RestoresCastlingRightsAfterKingMove()
    {
        var rights = CastlingRights.WhiteKingSide | CastlingRights.WhiteQueenSide;
        var position = CreatePosition(
            new PieceBitboards(whiteKing: Mask("e1"), blackKing: Mask("e8")),
            castlingRights: rights);
        var move = new Move(Square.Parse("e1"), Square.Parse("e2"));

        var undoState = position.MakeMove(move);
        Assert.Equal(CastlingRights.None, position.CastlingRights & rights);
        position.UnmakeMove(move, undoState);

        Assert.Equal(rights, position.CastlingRights);
    }

    [Fact]
    public void MakeMove_RejectsEmptyOrWrongSideSourceWithoutChangingState()
    {
        var emptySourcePosition = CreatePosition(
            new PieceBitboards(whiteKing: Mask("e1"), blackKing: Mask("e8")));
        AssertRejectedWithoutMutation(
            emptySourcePosition,
            () => emptySourcePosition.MakeMove(new Move(Square.Parse("b1"), Square.Parse("c3"))),
            typeof(InvalidOperationException));

        var wrongSidePosition = CreatePosition(
            new PieceBitboards(
                whiteKing: Mask("e1"),
                blackKnights: Mask("g8"),
                blackKing: Mask("e8")));
        AssertRejectedWithoutMutation(
            wrongSidePosition,
            () => wrongSidePosition.MakeMove(new Move(Square.Parse("g8"), Square.Parse("f6"))),
            typeof(InvalidOperationException));
    }

    [Fact]
    public void MakeMove_RejectsCaptureFlagMismatchWithoutChangingState()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("a2"),
                whiteKing: Mask("e1"),
                blackKnights: Mask("a7"),
                blackKing: Mask("e8")));

        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(new Move(Square.Parse("a2"), Square.Parse("a7"))),
            typeof(InvalidOperationException));
        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(
                new Move(Square.Parse("a2"), Square.Parse("a3"), flags: MoveFlags.Capture)),
            typeof(InvalidOperationException));
    }

    [Fact]
    public void MakeMove_RejectsOwnPieceAndKingCapturesWithoutChangingState()
    {
        var ownPiecePosition = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("a2"),
                whiteKnights: Mask("a3"),
                whiteKing: Mask("e1"),
                blackKing: Mask("e8")));
        AssertRejectedWithoutMutation(
            ownPiecePosition,
            () => ownPiecePosition.MakeMove(
                new Move(Square.Parse("a2"), Square.Parse("a3"), flags: MoveFlags.Capture)),
            typeof(InvalidOperationException));

        var kingCapturePosition = CreatePosition(
            new PieceBitboards(
                whiteRooks: Mask("e2"),
                whiteKing: Mask("a1"),
                blackKing: Mask("e8")));
        AssertRejectedWithoutMutation(
            kingCapturePosition,
            () => kingCapturePosition.MakeMove(
                new Move(Square.Parse("e2"), Square.Parse("e8"), flags: MoveFlags.Capture)),
            typeof(InvalidOperationException));
    }

    [Fact]
    public void MakeMove_RejectsUnsupportedSpecialMovesWithoutChangingState()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whitePawns: Mask("e7"),
                whiteKing: Mask("e1"),
                blackKing: Mask("e8")));

        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(
                new Move(Square.Parse("e1"), Square.Parse("g1"), flags: MoveFlags.KingSideCastle)),
            typeof(NotSupportedException));
        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(
                new Move(
                    Square.Parse("e5"),
                    Square.Parse("d6"),
                    flags: MoveFlags.Capture | MoveFlags.EnPassant)),
            typeof(NotSupportedException));
        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(
                new Move(Square.Parse("e7"), Square.Parse("e8"), promotion: PieceType.Queen)),
            typeof(NotSupportedException));
    }

    [Fact]
    public void MakeMove_RejectsInvalidPawnTransitionsWithoutChangingState()
    {
        var position = CreatePosition(
            new PieceBitboards(
                whitePawns: Mask("e2"),
                whiteKing: Mask("a1"),
                blackKing: Mask("h8")));

        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(new Move(Square.Parse("e2"), Square.Parse("e4"))),
            typeof(InvalidOperationException));
        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(
                new Move(Square.Parse("e2"), Square.Parse("e5"), flags: MoveFlags.DoublePawnPush)),
            typeof(InvalidOperationException));
        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(new Move(Square.Parse("e2"), Square.Parse("e8"))),
            typeof(NotSupportedException));
    }

    [Fact]
    public void MakeMove_ClockOverflowDoesNotPartiallyMutatePosition()
    {
        var position = CreatePosition(
            new PieceBitboards(whiteKnights: Mask("g1"), whiteKing: Mask("e1"), blackKing: Mask("e8")),
            halfmoveClock: int.MaxValue);

        AssertRejectedWithoutMutation(
            position,
            () => position.MakeMove(new Move(Square.Parse("g1"), Square.Parse("f3"))),
            typeof(OverflowException));
    }

    [Fact]
    public void UnmakeMove_RejectsInvalidUndoStateWithoutChangingPosition()
    {
        var position = CreatePosition(
            new PieceBitboards(whiteKnights: Mask("g1"), whiteKing: Mask("e1"), blackKing: Mask("e8")));
        var move = new Move(Square.Parse("g1"), Square.Parse("f3"));
        var undoState = position.MakeMove(move);
        var before = Snapshot(position);

        Assert.Throws<ArgumentException>(() => position.UnmakeMove(move, default));
        AssertMatches(before, position);

        var mismatchedMove = new Move(Square.Parse("g1"), Square.Parse("h3"));
        Assert.Throws<InvalidOperationException>(() => position.UnmakeMove(mismatchedMove, undoState));
        AssertMatches(before, position);

        var mismatchedUndoState = new UndoState(
            capturedPiece: null,
            capturedSquare: null,
            previousSideToMove: Color.White,
            previousCastlingRights: CastlingRights.None,
            previousEnPassantTarget: null,
            previousHalfmoveClock: 1,
            previousFullmoveNumber: 1,
            previousHash: undoState.PreviousHash);
        Assert.Throws<InvalidOperationException>(() => position.UnmakeMove(move, mismatchedUndoState));
        AssertMatches(before, position);
    }

    private static Position CreatePosition(
        PieceBitboards pieces,
        Color sideToMove = Color.White,
        CastlingRights castlingRights = CastlingRights.None,
        Square? enPassantTarget = null,
        int halfmoveClock = 0,
        int fullmoveNumber = 1) =>
        new(pieces, sideToMove, castlingRights, enPassantTarget, halfmoveClock, fullmoveNumber);

    private static ulong Mask(string coordinate) => 1UL << Square.Parse(coordinate).Index;

    private static PositionSnapshot Snapshot(Position position) =>
        new(
            position.Pieces,
            position.SideToMove,
            position.CastlingRights,
            position.EnPassantTarget,
            position.HalfmoveClock,
            position.FullmoveNumber,
            position.Hash,
            position.PositionHistory.ToArray());

    private static void AssertMatches(PositionSnapshot expected, Position actual)
    {
        Assert.Equal(expected.Pieces, actual.Pieces);
        Assert.Equal(expected.SideToMove, actual.SideToMove);
        Assert.Equal(expected.CastlingRights, actual.CastlingRights);
        Assert.Equal(expected.EnPassantTarget, actual.EnPassantTarget);
        Assert.Equal(expected.HalfmoveClock, actual.HalfmoveClock);
        Assert.Equal(expected.FullmoveNumber, actual.FullmoveNumber);
        Assert.Equal(expected.Hash, actual.Hash);
        Assert.Equal(expected.PositionHistory, actual.PositionHistory);
        Assert.Equal(expected.Pieces.WhiteOccupancy, actual.WhiteOccupancy);
        Assert.Equal(expected.Pieces.BlackOccupancy, actual.BlackOccupancy);
        Assert.Equal(expected.Pieces.TotalOccupancy, actual.TotalOccupancy);
    }

    private static void AssertRejectedWithoutMutation(
        Position position,
        Action action,
        Type exceptionType)
    {
        var before = Snapshot(position);

        var exception = Record.Exception(action);

        Assert.NotNull(exception);
        Assert.Equal(exceptionType, exception.GetType());
        AssertMatches(before, position);
    }

    private sealed record PositionSnapshot(
        PieceBitboards Pieces,
        Color SideToMove,
        CastlingRights CastlingRights,
        Square? EnPassantTarget,
        int HalfmoveClock,
        int FullmoveNumber,
        ulong Hash,
        ulong[] PositionHistory);
}
