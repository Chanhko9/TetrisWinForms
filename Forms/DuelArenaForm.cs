using TetrisWinForms.Controls;
using TetrisWinForms.Core;
using TetrisWinForms.Game;
using TetrisWinForms.Managers;
using TetrisWinForms.Models;
using System.Runtime.InteropServices;

namespace TetrisWinForms.Forms;

public sealed partial class DuelArenaForm : Form
{
    private readonly Random _random1 = new();
    private readonly Random _random2 = new();
    private readonly TetrisGameEngine _engine1;
    private readonly TetrisGameEngine _engine2;
    private readonly System.Windows.Forms.Timer _dropTimer1 = new();
    private readonly System.Windows.Forms.Timer _dropTimer2 = new();
    private readonly System.Windows.Forms.Timer _matchTimer = new();
    private readonly System.Windows.Forms.Timer _inputTimer = new();
    private readonly DuelKeyMessageFilter _keyFilter;
    private readonly HashSet<Keys> _previousKeys = new();
    private DateTime _lastP1Horizontal = DateTime.MinValue;
    private DateTime _lastP1SoftDrop = DateTime.MinValue;
    private DateTime _lastP1MessageInput = DateTime.MinValue;

    private string _player1Name = "Player 1";
    private string _player2Name = "Player 2";
    private int _remainingSeconds = 300;
    private bool _paused;
    private bool _countdownActive;
    private bool _ending;
    private bool _player1Out;
    private bool _player2Out;


    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool IsKeyDown(Keys key) => (GetAsyncKeyState((int)key) & 0x8000) != 0;
    private static bool IsVkDown(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    public DuelArenaForm()
    {
        _engine1 = new TetrisGameEngine(GameMode.DuelArena, _random1);
        _engine2 = new TetrisGameEngine(GameMode.DuelArena, _random2);

        InitializeComponent();
        _keyFilter = new DuelKeyMessageFilter(this);
        Application.AddMessageFilter(_keyFilter);
        ConfigureTimers();
        Resize += (_, _) => LayoutArena();
        Shown += (_, _) => StartFirstMatch();
        FormClosed += (_, _) =>
        {
            StopTimers();
            Application.RemoveMessageFilter(_keyFilter);
            AudioManager.Instance.StopMusic();
        };
    }

    private void ConfigureTimers()
    {
        _dropTimer1.Interval = _engine1.DropIntervalMs;
        _dropTimer1.Tick += (_, _) =>
        {
            if (_paused || _countdownActive || _ending || _player1Out) return;
            HandleResult(1, _engine1.Tick());
        };

        _dropTimer2.Interval = _engine2.DropIntervalMs;
        _dropTimer2.Tick += (_, _) =>
        {
            if (_paused || _countdownActive || _ending || _player2Out) return;
            HandleResult(2, _engine2.Tick());
        };

        _matchTimer.Interval = 1000;
        _matchTimer.Tick += (_, _) =>
        {
            if (_paused || _countdownActive || _ending) return;
            _remainingSeconds--;
            UpdateTimerLabel();
            if (_remainingSeconds <= 0)
                EndMatch();
        };

        // Player 1 uses direct Windows keyboard polling so WASD is independent of WinForms focus.
        _inputTimer.Interval = 30;
        _inputTimer.Tick += (_, _) => PollPlayer1Keyboard();
    }

    private void StartFirstMatch()
    {
        using var dialog = new DuelNameDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            Close();
            return;
        }

        _player1Name = dialog.Player1Name;
        _player2Name = dialog.Player2Name;
        StartMatch();
    }

    private void StartMatch()
    {
        StopTimers();
        _engine1.Reset();
        _engine2.Reset();
        _remainingSeconds = 300;
        _paused = false;
        _countdownActive = true;
        _ending = false;
        _player1Out = false;
        _player2Out = false;
        _previousKeys.Clear();
        _lastP1Horizontal = DateTime.MinValue;
        _lastP1SoftDrop = DateTime.MinValue;
        _lastP1MessageInput = DateTime.MinValue;

        _player1Label.Text = _player1Name;
        _player2Label.Text = _player2Name;
        _dropTimer1.Interval = _engine1.DropIntervalMs;
        _dropTimer2.Interval = _engine2.DropIntervalMs;
        UpdateTimerLabel();
        RefreshScene();

        AudioManager.Instance.PlayMusic("basic_theme.mp3");
        _ = RunCountdownAsync();
    }

    private async Task RunCountdownAsync()
    {
        for (int i = 3; i >= 1; i--)
        {
            if (IsDisposed || _ending) return;
            ShowBothOverlay(i.ToString(), string.Empty);
            AudioManager.Instance.PlaySfx("warning_tick.wav", 0.55f);
            await Task.Delay(650);
        }

        if (IsDisposed || _ending) return;
        ShowBothOverlay("GO!", string.Empty);
        AudioManager.Instance.PlaySfx("hard_drop.wav", 0.4f);
        await Task.Delay(450);

        if (IsDisposed || _ending) return;
        _countdownActive = false;
        _board1.ShowOverlay = false;
        _board2.ShowOverlay = false;
        _dropTimer1.Start();
        _dropTimer2.Start();
        _matchTimer.Start();
        _inputTimer.Start();
        RefreshScene();
        Activate();
        ActiveControl = null;
        Focus();
    }


    private void PollPlayer1Keyboard()
    {
        if (_ending || _countdownActive || _paused || _player1Out || !Visible)
            return;

        // If normal key messages are arriving, they are the primary input path.
        // Polling only becomes a fallback when Windows/IME/control focus swallows them.
        if ((DateTime.UtcNow - _lastP1MessageInput).TotalMilliseconds < 140)
            return;

        DateTime now = DateTime.UtcNow;

        // Raw Windows virtual-key codes: A=0x41, D=0x44, W=0x57, S=0x53.
        // Number-row fallback: 4/6 move, 8 rotate, 5 drop, 0 hard drop, 1 hold.
        bool left = IsVkDown(0x41) || IsVkDown(0x34);
        bool right = IsVkDown(0x44) || IsVkDown(0x36);
        if ((left || right) && (now - _lastP1Horizontal).TotalMilliseconds >= 80)
        {
            if (left && !right) _engine1.TryMoveSide(-1);
            else if (right && !left) _engine1.TryMoveSide(1);
            _lastP1Horizontal = now;
            RefreshScene();
        }

        bool softDrop = IsVkDown(0x53) || IsVkDown(0x35);
        if (softDrop && (now - _lastP1SoftDrop).TotalMilliseconds >= 50)
        {
            HandleResult(1, _engine1.SoftDrop());
            _lastP1SoftDrop = now;
        }

        HandleP1EdgeVk(0x57, 0x38, Keys.W, Keys.D8, () =>
        {
            if (_engine1.RotateClockwise())
                AudioManager.Instance.PlaySfx("rotate.wav", 0.35f);
            RefreshScene();
        });

        HandleP1EdgeVk(0x20, 0x30, Keys.Space, Keys.D0, () =>
        {
            HandleResult(1, _engine1.HardDrop());
            AudioManager.Instance.PlaySfx("hard_drop.wav", 0.4f);
        });

        HandleP1EdgeVk(0x43, 0x31, Keys.C, Keys.D1, () =>
        {
            _engine1.HoldCurrentPiece();
            RefreshScene();
        });

        foreach (Keys key in new[] { Keys.W, Keys.D8, Keys.Space, Keys.D0, Keys.C, Keys.D1 })
        {
            if (IsKeyDown(key)) _previousKeys.Add(key);
            else _previousKeys.Remove(key);
        }
    }

    private void HandleP1EdgeVk(int primaryVk, int fallbackVk, Keys primaryKey, Keys fallbackKey, Action action)
    {
        bool down = IsVkDown(primaryVk) || IsVkDown(fallbackVk);
        bool wasDown = _previousKeys.Contains(primaryKey) || _previousKeys.Contains(fallbackKey);
        if (down && !wasDown)
            action();
    }

    private bool HandleInput(Keys key)
    {
        if (_ending || _countdownActive) return false;

        if (key == Keys.P)
        {
            TogglePause();
            return true;
        }

        if (_paused) return false;

        // Player 1: WASD + direct number-row fallback.
        switch (key)
        {
            case Keys.A when !_player1Out:
            case Keys.D4 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                _engine1.TryMoveSide(-1);
                RefreshScene();
                return true;

            case Keys.D when !_player1Out:
            case Keys.D6 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                _engine1.TryMoveSide(1);
                RefreshScene();
                return true;

            case Keys.W when !_player1Out:
            case Keys.D8 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                if (_engine1.RotateClockwise())
                    AudioManager.Instance.PlaySfx("rotate.wav", 0.35f);
                RefreshScene();
                return true;

            case Keys.S when !_player1Out:
            case Keys.D5 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                HandleResult(1, _engine1.SoftDrop());
                return true;

            case Keys.Space when !_player1Out:
            case Keys.D0 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                HandleResult(1, _engine1.HardDrop());
                AudioManager.Instance.PlaySfx("hard_drop.wav", 0.4f);
                return true;

            case Keys.C when !_player1Out:
            case Keys.D1 when !_player1Out:
                _lastP1MessageInput = DateTime.UtcNow;
                _engine1.HoldCurrentPiece();
                RefreshScene();
                return true;

            // Player 2
            case Keys.Left when !_player2Out:
                _engine2.TryMoveSide(-1);
                RefreshScene();
                return true;
            case Keys.Right when !_player2Out:
                _engine2.TryMoveSide(1);
                RefreshScene();
                return true;
            case Keys.Up when !_player2Out:
                if (_engine2.RotateClockwise()) AudioManager.Instance.PlaySfx("rotate.wav", 0.35f);
                RefreshScene();
                return true;
            case Keys.Down when !_player2Out:
                HandleResult(2, _engine2.SoftDrop());
                return true;
            case Keys.Enter when !_player2Out:
                HandleResult(2, _engine2.HardDrop());
                AudioManager.Instance.PlaySfx("hard_drop.wav", 0.4f);
                return true;
            case Keys.M when !_player2Out:
                _engine2.HoldCurrentPiece();
                RefreshScene();
                return true;
        }

        return false;
    }

    private void HandleResult(int player, GameStepResult result)
    {
        TetrisGameEngine engine = player == 1 ? _engine1 : _engine2;
        System.Windows.Forms.Timer timer = player == 1 ? _dropTimer1 : _dropTimer2;
        BoardCanvas board = player == 1 ? _board1 : _board2;

        if (result.LinesCleared > 0)
        {
            board.TriggerLineClear(result.LinesCleared, GravityDirection.Down,
                player == 1 ? Color.DeepSkyBlue : Color.OrangeRed);
            AudioManager.Instance.PlaySfx("line_clear.wav", 0.5f);
        }

        timer.Interval = engine.DropIntervalMs;

        if (result.GameOver || engine.IsGameOver)
        {
            if (player == 1)
            {
                _player1Out = true;
                _dropTimer1.Stop();
            }
            else
            {
                _player2Out = true;
                _dropTimer2.Stop();
            }

            board.ShowOverlay = true;
            board.OverlayTitle = "TOP OUT";
            board.OverlaySubtitle = "WAIT FOR TIME UP";

            if (_player1Out && _player2Out)
                EndMatch();
        }

        RefreshScene();
    }

    private void RefreshScene()
    {
        BindBoard(_board1, _engine1, _player1Out);
        BindBoard(_board2, _engine2, _player2Out);

        _next1.PieceType = _engine1.NextPieceType;
        _hold1.PieceType = _engine1.HeldPieceType;
        _next2.PieceType = _engine2.NextPieceType;
        _hold2.PieceType = _engine2.HeldPieceType;
        _next1.Invalidate();
        _hold1.Invalidate();
        _next2.Invalidate();
        _hold2.Invalidate();

        _score1.Text = _engine1.Score.ToString("N0");
        _score2.Text = _engine2.Score.ToString("N0");
    }

    private static void BindBoard(BoardCanvas board, TetrisGameEngine engine, bool topOut)
    {
        board.Board = engine.Board;
        board.ActivePiece = topOut ? null : engine.CurrentPiece;
        board.GhostPiece = topOut ? null : engine.GetGhostPiece();
        board.Gravity = GravityDirection.Down;
        board.Mode = GameMode.DuelArena;
        if (!topOut && !board.ShowOverlay)
            board.ShowOverlay = false;
        board.Invalidate();
    }

    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused)
        {
            _dropTimer1.Stop();
            _dropTimer2.Stop();
            _matchTimer.Stop();
            ShowBothOverlay("PAUSED", "PRESS P TO RESUME");
        }
        else
        {
            if (!_player1Out) _dropTimer1.Start();
            if (!_player2Out) _dropTimer2.Start();
            _matchTimer.Start();
            _inputTimer.Start();
            _board1.ShowOverlay = _player1Out;
            _board2.ShowOverlay = _player2Out;
        }
        RefreshScene();
    }

    private void ShowBothOverlay(string title, string subtitle)
    {
        foreach (BoardCanvas board in new[] { _board1, _board2 })
        {
            board.ShowOverlay = true;
            board.OverlayTitle = title;
            board.OverlaySubtitle = subtitle;
            board.Invalidate();
        }
    }

    private void UpdateTimerLabel()
    {
        int minutes = Math.Max(0, _remainingSeconds) / 60;
        int seconds = Math.Max(0, _remainingSeconds) % 60;
        _timerLabel.Text = $"{minutes:00}:{seconds:00}";
        _timerLabel.ForeColor = _remainingSeconds <= 30 ? Color.OrangeRed : Color.Gold;
    }

    private void StopTimers()
    {
        _dropTimer1.Stop();
        _dropTimer2.Stop();
        _matchTimer.Stop();
        _inputTimer.Stop();
    }

    private void EndMatch()
    {
        if (_ending) return;
        _ending = true;
        StopTimers();
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySfx("game_over.wav", 0.65f);

        DateTime now = DateTime.Now;
        ScoreManager.Save(new ScoreRecord(_player1Name, _engine1.Score, GameMode.DuelArena, now));
        ScoreManager.Save(new ScoreRecord(_player2Name, _engine2.Score, GameMode.DuelArena, now));

        using var result = new DuelResultForm(_player1Name, _engine1.Score, _player2Name, _engine2.Score);
        result.ShowDialog(this);

        if (result.Rematch)
            StartMatch();
        else
            Close();
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (HandleInput(key))
            return true;

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private sealed class DuelKeyMessageFilter : IMessageFilter
    {
        private readonly DuelArenaForm _owner;

        public DuelKeyMessageFilter(DuelArenaForm owner)
        {
            _owner = owner;
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;

            if ((m.Msg != WM_KEYDOWN && m.Msg != WM_SYSKEYDOWN) || Form.ActiveForm != _owner)
                return false;

            Keys key = (Keys)(int)m.WParam;
            return _owner.HandleInput(key);
        }
    }

}
