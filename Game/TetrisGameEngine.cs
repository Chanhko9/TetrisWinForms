using TetrisWinForms.Core;
using TetrisWinForms.Game.SpecialItems;

namespace TetrisWinForms.Game;

public sealed class TetrisGameEngine
{
    private readonly Random _random;
    private PieceBag _bag;
    private int _comboStreak;
    private bool _backToBackTetris;

    public GameMode Mode { get; }
    public BoardModel Board { get; } = new();
    public ActivePiece CurrentPiece { get; private set; } = null!;
    public TetrominoType NextPieceType { get; private set; }
    public TetrominoType? HeldPieceType { get; private set; }
    public bool CanHold { get; private set; } = true;
    public GravityDirection Gravity { get; private set; } = GravityDirection.Down;
    public int Score { get; private set; }
    public int Lines { get; private set; }
    public int Level => Math.Max(1, Lines / 10 + 1);
    public int PiecesLocked { get; private set; }
    public bool IsGameOver { get; private set; }
    public SpecialItemType? CurrentRelic { get; private set; }
    public int ComboStreak => _comboStreak;
    public bool BackToBackActive => _backToBackTetris;

    public int DropIntervalMs => Math.Max(90, 720 - (Level - 1) * 55);

    public TetrisGameEngine(GameMode mode, Random? random = null)
    {
        Mode = mode;
        _random = random ?? new Random();
        _bag = new PieceBag(_random);
        Reset();
    }

    public void Reset()
    {
        Board.Clear();
        Score = 0;
        Lines = 0;
        PiecesLocked = 0;
        IsGameOver = false;
        Gravity = GravityDirection.Down;
        HeldPieceType = null;
        CanHold = true;
        _comboStreak = 0;
        _backToBackTetris = false;
        CurrentRelic = Mode == GameMode.ArcaneChaos ? RandomRelic() : null;
        _bag = new PieceBag(_random);
        NextPieceType = _bag.Next();
        SpawnNextPiece();
    }

    public bool TryMoveSide(int direction)
    {
        if (IsGameOver || direction == 0) return false;

        (int dr, int dc) = Gravity switch
        {
            GravityDirection.Down or GravityDirection.Up => (0, Math.Sign(direction)),
            GravityDirection.Right or GravityDirection.Left => (Math.Sign(direction), 0),
            _ => (0, Math.Sign(direction))
        };

        return TryMove(dr, dc);
    }

    public bool HoldCurrentPiece()
    {
        if (IsGameOver || !CanHold) return false;

        TetrominoType current = CurrentPiece.Type;
        if (HeldPieceType is null)
        {
            HeldPieceType = current;
            SpawnNextPiece();
        }
        else
        {
            TetrominoType swap = HeldPieceType.Value;
            HeldPieceType = current;
            CurrentPiece = CreateSpawnPiece(swap);
        }

        CanHold = false;
        if (!Board.CanPlace(CurrentPiece))
            IsGameOver = true;

        return true;
    }

    public GameStepResult SoftDrop()
    {
        if (IsGameOver) return new(GameOver: true);

        (int dr, int dc) = GravityVector();
        if (TryMove(dr, dc))
            return new(Combo: _comboStreak);

        return LockCurrentPiece();
    }

    public GameStepResult Tick()
    {
        if (IsGameOver) return new(GameOver: true);

        (int dr, int dc) = GravityVector();
        if (TryMove(dr, dc))
            return new(Combo: _comboStreak);

        return LockCurrentPiece();
    }

    public GameStepResult HardDrop()
    {
        if (IsGameOver) return new(GameOver: true);

        (int dr, int dc) = GravityVector();

        while (TryMove(dr, dc))
        {
            // Hard drop changes position only. Scoring is based on cleared lines.
        }

        return LockCurrentPiece();
    }

    public bool RotateClockwise()
    {
        if (IsGameOver || CurrentPiece.Type is TetrominoType.O or TetrominoType.Plus)
            return !IsGameOver;

        int oldRotation = CurrentPiece.Rotation;
        int nextRotation = (oldRotation + 1) % 4;

        (int dr, int dc)[] kicks =
        [
            (0, 0),
            (0, -1), (0, 1),
            (-1, 0), (1, 0),
            (0, -2), (0, 2),
            (-2, 0), (2, 0)
        ];

        CurrentPiece.Rotation = nextRotation;
        foreach ((int dr, int dc) in kicks)
        {
            CurrentPiece.Row += dr;
            CurrentPiece.Col += dc;

            if (Board.CanPlace(CurrentPiece))
                return true;

            CurrentPiece.Row -= dr;
            CurrentPiece.Col -= dc;
        }

        CurrentPiece.Rotation = oldRotation;
        return false;
    }

    public ActivePiece GetGhostPiece()
    {
        ActivePiece ghost = CurrentPiece.Copy();
        (int dr, int dc) = GravityVector();

        while (true)
        {
            ghost.Row += dr;
            ghost.Col += dc;
            if (Board.CanPlace(ghost)) continue;
            ghost.Row -= dr;
            ghost.Col -= dc;
            return ghost;
        }
    }

    public SpecialItemResult? ActivateRelic()
    {
        if (Mode != GameMode.ArcaneChaos || CurrentRelic is null || IsGameOver)
            return null;

        ActivePiece target = GetGhostPiece();
        List<(int Row, int Col)> cells = target.Cells().ToList();
        int row = (int)Math.Round(cells.Average(x => x.Row));
        int col = (int)Math.Round(cells.Average(x => x.Col));

        SpecialItemType relic = CurrentRelic.Value;
        SpecialItemResult result = SpecialItemEngine.Apply(Board, relic, row, col);
        CurrentRelic = null;

        Board.Compact(GravityDirection.Down);
        return result;
    }

    public GameStepResult ShiftGravity(GravityDirection newGravity)
    {
        if (Mode != GameMode.ZeroGRift || IsGameOver || newGravity == Gravity)
            return new(GameOver: IsGameOver, Combo: _comboStreak);

        Gravity = newGravity;
        Board.Compact(Gravity);

        CurrentPiece = CreateSpawnPiece(CurrentPiece.Type);
        if (!Board.CanPlace(CurrentPiece))
        {
            IsGameOver = true;
            return new(GameOver: true, Combo: _comboStreak);
        }

        int cleared = Board.ClearCompletedLines(Gravity);
        ScoreOutcome scoring = cleared > 0
            ? ApplyLineScore(cleared)
            : new ScoreOutcome(0, 0, 0, _comboStreak);
        return new(
            LinesCleared: cleared,
            ScoreDelta: scoring.Total,
            Combo: scoring.Combo,
            BackToBackBonus: scoring.BackToBackBonus > 0,
            ComboBonus: scoring.ComboBonus,
            BackToBackBonusScore: scoring.BackToBackBonus);
    }

    public GameStepResult RaiseCursedFloor()
    {
        if (Mode != GameMode.CursedTide || IsGameOver)
            return new(GameOver: IsGameOver, Combo: _comboStreak);

        bool overflow = Board.RiseGarbageRow(_random);

        // The rising floor pushes the active piece upward instead of killing the run
        // just because the new water row overlaps it. Only a real top overflow or a
        // blocked respawn ends the game.
        if (!Board.CanPlace(CurrentPiece))
        {
            bool recovered = false;
            for (int i = 0; i < 3; i++)
            {
                CurrentPiece.Row--;
                if (Board.CanPlace(CurrentPiece))
                {
                    recovered = true;
                    break;
                }
            }

            if (!recovered)
            {
                CurrentPiece = CreateSpawnPiece(CurrentPiece.Type);
                recovered = Board.CanPlace(CurrentPiece);
            }

            if (!recovered)
                overflow = true;
        }

        if (overflow)
        {
            IsGameOver = true;
            return new(GameOver: true, Combo: _comboStreak);
        }

        return new(Combo: _comboStreak);
    }

    private bool TryMove(int dr, int dc)
    {
        CurrentPiece.Row += dr;
        CurrentPiece.Col += dc;

        if (Board.CanPlace(CurrentPiece))
            return true;

        CurrentPiece.Row -= dr;
        CurrentPiece.Col -= dc;
        return false;
    }

    private GameStepResult LockCurrentPiece()
    {
        Board.LockPiece(CurrentPiece);
        PiecesLocked++;
        CanHold = true;

        int cleared = Board.ClearCompletedLines(Gravity);
        ScoreOutcome scoring = ApplyLineScore(cleared);
        SpecialItemType? granted = null;

        if (Mode == GameMode.ArcaneChaos && CurrentRelic is null && PiecesLocked % 6 == 0)
        {
            CurrentRelic = RandomRelic();
            granted = CurrentRelic;
        }

        SpawnNextPiece();
        if (!Board.CanPlace(CurrentPiece))
        {
            IsGameOver = true;
            return new(
                true,
                cleared,
                scoring.Total,
                true,
                granted,
                scoring.Combo,
                scoring.BackToBackBonus > 0,
                scoring.ComboBonus,
                scoring.BackToBackBonus);
        }

        return new(
            true,
            cleared,
            scoring.Total,
            false,
            granted,
            scoring.Combo,
            scoring.BackToBackBonus > 0,
            scoring.ComboBonus,
            scoring.BackToBackBonus);
    }

    private ScoreOutcome ApplyLineScore(int cleared)
    {
        if (cleared <= 0)
        {
            _comboStreak = 0;
            return new ScoreOutcome(0, 0, 0, _comboStreak);
        }

        _comboStreak++;

        // Simple coursework scoring:
        // 1 cleared row = 1 base point.
        // Combo bonus = consecutive-clear streak - 1.
        // Back-to-Back bonus = +2 when a Tetris (4-line clear) follows another Tetris.
        int baseScore = cleared;

        bool isTetris = cleared >= 4;
        int backToBackBonus = isTetris && _backToBackTetris ? 2 : 0;

        int comboBonus = _comboStreak > 1
            ? _comboStreak - 1
            : 0;

        int gained = baseScore + comboBonus + backToBackBonus;
        Score += gained;
        Lines += cleared;

        if (isTetris)
            _backToBackTetris = true;
        else
            _backToBackTetris = false;

        return new ScoreOutcome(gained, comboBonus, backToBackBonus, _comboStreak);
    }

    private void SpawnNextPiece()
    {
        TetrominoType type = NextPieceType;
        NextPieceType = _bag.Next();
        CurrentPiece = CreateSpawnPiece(type);
    }

    private ActivePiece CreateSpawnPiece(TetrominoType type)
    {
        IReadOnlyList<BlockOffset> cells = TetrominoShapes.Get(type, 0);
        int minRow = cells.Min(x => x.Row);
        int maxRow = cells.Max(x => x.Row);
        int minCol = cells.Min(x => x.Col);
        int maxCol = cells.Max(x => x.Col);
        int height = maxRow - minRow + 1;
        int width = maxCol - minCol + 1;

        int centeredRow = (BoardModel.Rows - height) / 2 - minRow;
        int centeredCol = (BoardModel.Columns - width) / 2 - minCol;

        return Gravity switch
        {
            GravityDirection.Down => new ActivePiece(type, -minRow, centeredCol),
            GravityDirection.Up => new ActivePiece(type, BoardModel.Rows - height - minRow, centeredCol),
            GravityDirection.Right => new ActivePiece(type, centeredRow, -minCol),
            GravityDirection.Left => new ActivePiece(type, centeredRow, BoardModel.Columns - width - minCol),
            _ => new ActivePiece(type, -minRow, centeredCol)
        };
    }

    private (int Dr, int Dc) GravityVector()
        => Gravity switch
        {
            GravityDirection.Down => (1, 0),
            GravityDirection.Right => (0, 1),
            GravityDirection.Up => (-1, 0),
            GravityDirection.Left => (0, -1),
            _ => (1, 0)
        };

    private SpecialItemType RandomRelic()
    {
        SpecialItemType[] values = Enum.GetValues<SpecialItemType>();
        return values[_random.Next(values.Length)];
    }

    private readonly record struct ScoreOutcome(int Total, int ComboBonus, int BackToBackBonus, int Combo);
}
