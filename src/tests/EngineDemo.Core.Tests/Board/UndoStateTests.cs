using EngineDemo.Core.Board;

namespace EngineDemo.Core.Tests.Board;

public sealed class UndoStateTests
{
    [Fact]
    public void DefaultUndoState_IsInvalid()
    {
        Assert.False(default(UndoState).IsValid);
    }

    [Fact]
    public void Constructor_PreservesAllPreviousPositionMetadata()
    {
        var capturedSquare = new Square(35);
        var enPassantTarget = new Square(43);
        var capturedPiece = new Piece(Color.Black, PieceType.Pawn);
        var undoState = new UndoState(
            capturedPiece,
            capturedSquare,
            previousSideToMove: Color.Black,
            previousCastlingRights: CastlingRights.WhiteKingSide | CastlingRights.BlackQueenSide,
            previousEnPassantTarget: enPassantTarget,
            previousHalfmoveClock: 17,
            previousFullmoveNumber: 23,
            previousHash: 0x1234UL);

        Assert.True(undoState.IsValid);
        Assert.Equal(capturedPiece, undoState.CapturedPiece);
        Assert.Equal(capturedSquare, undoState.CapturedSquare);
        Assert.Equal(Color.Black, undoState.PreviousSideToMove);
        Assert.Equal(
            CastlingRights.WhiteKingSide | CastlingRights.BlackQueenSide,
            undoState.PreviousCastlingRights);
        Assert.Equal(enPassantTarget, undoState.PreviousEnPassantTarget);
        Assert.Equal(17, undoState.PreviousHalfmoveClock);
        Assert.Equal(23, undoState.PreviousFullmoveNumber);
        Assert.Equal(0x1234UL, undoState.PreviousHash);
    }

    [Fact]
    public void Constructor_AcceptsNoCaptureAndNoEnPassantTarget()
    {
        var undoState = new UndoState(
            capturedPiece: null,
            capturedSquare: null,
            previousSideToMove: Color.White,
            previousCastlingRights: CastlingRights.None,
            previousEnPassantTarget: null,
            previousHalfmoveClock: 0,
            previousFullmoveNumber: 1,
            previousHash: 0UL);

        Assert.True(undoState.IsValid);
        Assert.Null(undoState.CapturedPiece);
        Assert.Null(undoState.CapturedSquare);
        Assert.Null(undoState.PreviousEnPassantTarget);
    }

    [Fact]
    public void Constructor_RequiresCapturedPieceAndSquareTogether()
    {
        Assert.Throws<ArgumentException>(() =>
            new UndoState(
                new Piece(Color.Black, PieceType.Knight),
                capturedSquare: null,
                Color.White,
                CastlingRights.None,
                previousEnPassantTarget: null,
                previousHalfmoveClock: 0,
                previousFullmoveNumber: 1,
                previousHash: 0UL));

        Assert.Throws<ArgumentException>(() =>
            new UndoState(
                capturedPiece: null,
                new Square(20),
                Color.White,
                CastlingRights.None,
                previousEnPassantTarget: null,
                previousHalfmoveClock: 0,
                previousFullmoveNumber: 1,
                previousHash: 0UL));
    }

    [Fact]
    public void Constructor_RejectsInvalidCapturedAndEnPassantSquares()
    {
        Assert.Throws<ArgumentException>(() =>
            new UndoState(
                new Piece(Color.Black, PieceType.Pawn),
                default(Square),
                Color.White,
                CastlingRights.None,
                previousEnPassantTarget: null,
                previousHalfmoveClock: 0,
                previousFullmoveNumber: 1,
                previousHash: 0UL));

        Assert.Throws<ArgumentException>(() =>
            new UndoState(
                null,
                null,
                Color.White,
                CastlingRights.None,
                previousEnPassantTarget: default(Square),
                previousHalfmoveClock: 0,
                previousFullmoveNumber: 1,
                previousHash: 0UL));
    }

    [Fact]
    public void Constructor_RejectsInvalidSideAndCastlingRights()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UndoState(null, null, (Color)2, CastlingRights.None, null, 0, 1, 0UL));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UndoState(null, null, Color.White, (CastlingRights)16, null, 0, 1, 0UL));
    }

    [Theory]
    [InlineData(-1)]
    public void Constructor_RejectsNegativeHalfmoveClock(int halfmoveClock)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UndoState(null, null, Color.White, CastlingRights.None, null, halfmoveClock, 1, 0UL));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsFullmoveNumbersBelowOne(int fullmoveNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UndoState(null, null, Color.White, CastlingRights.None, null, 0, fullmoveNumber, 0UL));
    }
}
