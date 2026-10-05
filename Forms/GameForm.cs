using TetrisWinForms.Controls;
using TetrisWinForms.Core;
using TetrisWinForms.Effects;
using TetrisWinForms.Game;
using TetrisWinForms.Game.SpecialItems;
using TetrisWinForms.Managers;
using TetrisWinForms.Models;

namespace TetrisWinForms.Forms;

public sealed partial class GameForm : Form
{
    private readonly GameMode _mode;
    private readonly GameModeProfile _profile;
    private readonly ModeVisualTheme _theme;
    private readonly Random _random = new();
    private readonly ScreenShakeService _shake = new();
    private readonly System.Windows.Forms.Timer _dropTimer = new();
    private readonly System.Windows.Forms.Timer _modeTimer = new();

    private TetrisGameEngine _engine;
    private string _playerName = "Player";
    private int _elapsedSeconds;
    private int _nextGravityShiftAt = 18;
    private int _nextTideRiseAt = 16;
    private bool _paused;
    private bool _endingGame;
    private bool _countdownActive;
    private DateTime _fogRevealUntil = DateTime.MinValue;
    private int _gravityIndex;
    private int _lastCountdownShown = -1;

    private static readonly GravityDirection[] GravityCycle =
    [
        GravityDirection.Down,
        GravityDirection.Right,
        GravityDirection.Up,
        GravityDirection.Left
    ];

    public GameForm(GameMode mode)
    {
        _mode = mode;
        _profile = GameModeProfile.Get(mode);
        _theme = ModeVisualTheme.Get(mode);
        _engine = new TetrisGameEngine(mode, _random);

        InitializeComponent();
        ConfigureTimers();
        Resize += (_, _) => LayoutGameUi();

        Shown += (_, _) => StartFirstSession();
        FormClosed += (_, _) =>
        {
            _dropTimer.Stop();
            _modeTimer.Stop();
            AudioManager.Instance.StopMusic();
        };
        KeyDown += GameForm_KeyDown;
    }

    private void SetGameVolumeFromMouse(int x)
    {
        float ratio = Math.Clamp(x / (float)Math.Max(1, _volumeBar.Width), 0f, 1f);
        PreferencesManager.SetMasterVolume(ratio);
        RefreshGameVolumeUi();
        Focus();
    }

    private void AdjustGameVolume(float delta)
    {
        float next = Math.Clamp(PreferencesManager.Current.MasterVolume + delta, 0f, 1f);
        PreferencesManager.SetMasterVolume(next);
        RefreshGameVolumeUi();
        Focus();
    }

    private void RefreshGameVolumeUi()
    {
        float value = AudioManager.Instance.IsMuted ? 0f : PreferencesManager.Current.MasterVolume;
        _volumeFill.Width = Math.Max(0, (int)Math.Round(_volumeBar.Width * value));
        _volumeValue.Text = $"{(int)Math.Round(value * 100)}%";
        _volumeBar.Invalidate();
    }

    private void ConfigureTimers()
    {
        _dropTimer.Interval = _engine.DropIntervalMs;
        _dropTimer.Tick += (_, _) =>
        {
            if (_paused || _endingGame || _countdownActive) return;
            HandleStep(_engine.Tick());
        };

        _modeTimer.Interval = 1000;
        _modeTimer.Tick += async (_, _) =>
        {
            if (_paused || _endingGame || _countdownActive) return;
            _elapsedSeconds++;
            await ProcessModeClockAsync();
            RefreshScene();
        };
    }

    private void StartFirstSession()
    {
        using var dialog = new PlayerNameDialog(_playerName);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            Close();
            return;
        }

        _playerName = dialog.PlayerName;
        StartSession();
    }

    private void StartSession()
    {
        _dropTimer.Stop();
        _modeTimer.Stop();
        _engine.Reset();
        _elapsedSeconds = 0;
        _nextGravityShiftAt = 18;
        _nextTideRiseAt = 16;
        _gravityIndex = 0;
        _lastCountdownShown = -1;
        _paused = false;
        _endingGame = false;
        _countdownActive = true;
        _fogRevealUntil = DateTime.MinValue;

        _dropTimer.Interval = _engine.DropIntervalMs;

        AudioManager.Instance.PlayMusic(_profile.MusicFile);
        RefreshScene();
        _ = RunStartCountdownAsync();
    }

    private async Task RunStartCountdownAsync()
    {
        for (int count = 3; count >= 1; count--)
        {
            if (IsDisposed || Disposing || _endingGame) return;
            _canvas.ShowOverlay = true;
            _canvas.OverlayTitle = count.ToString();
            _canvas.OverlaySubtitle = _profile.DisplayName;
            _canvas.Invalidate();
            AudioManager.Instance.PlaySfx("warning_tick.wav", 0.55f);
            await Task.Delay(650);
        }

        if (IsDisposed || Disposing || _endingGame) return;
        _canvas.ShowOverlay = true;
        _canvas.OverlayTitle = "GO!";
        _canvas.OverlaySubtitle = string.Empty;
        _canvas.Invalidate();
        AudioManager.Instance.PlaySfx("hard_drop.wav", 0.4f);
        await Task.Delay(450);

        if (IsDisposed || Disposing || _endingGame) return;
        _countdownActive = false;
        _canvas.ShowOverlay = false;
        _dropTimer.Start();
        _modeTimer.Start();
        RefreshScene();
        Focus();
    }

    private async Task ProcessModeClockAsync()
    {
        ShowUpcomingModeWarning();

        switch (_mode)
        {
            case GameMode.ZeroGRift:
                if (_elapsedSeconds >= _nextGravityShiftAt)
                {
                    _gravityIndex = (_gravityIndex + 1) % GravityCycle.Length;
                    GravityDirection direction = GravityCycle[_gravityIndex];
                    GameStepResult result = _engine.ShiftGravity(direction);
                    _nextGravityShiftAt += 18;

                    AudioManager.Instance.PlaySfx("gravity_shift.wav");
                    _eventBanner.ShowEvent("GRAVITY SHIFT!", $"NEW GRAVITY  {GravityArrow(direction)}", _theme.Accent, 1300);
                    _canvas.TriggerBoardPulse(_theme.Accent, 145);
                    _lastCountdownShown = -1;

                    if (PreferencesManager.Current.ScreenShakeEnabled)
                    {
                        await _shake.ShakeAsync(
                            _boardHost,
                            9,
                            320,
                            direction is GravityDirection.Left or GravityDirection.Right
                                ? ShakeAxis.Horizontal
                                : ShakeAxis.Vertical);
                    }

                    HandleStep(result, allowModeMessageOverride: false);
                }
                break;

            case GameMode.CursedTide:
                if (_elapsedSeconds >= _nextTideRiseAt)
                {
                    GameStepResult result = _engine.RaiseCursedFloor();
                    _nextTideRiseAt += Math.Max(9, 16 - _engine.Level);
                    AudioManager.Instance.PlaySfx("floor_rise.wav");
                    _eventBanner.ShowEvent("CURSED TIDE!", "THE FLOOR HAS RISEN", _theme.Danger, 1250);
                    _canvas.TriggerBoardPulse(_theme.Danger, 135);
                    _lastCountdownShown = -1;

                    if (PreferencesManager.Current.ScreenShakeEnabled)
                        await _shake.ShakeAsync(_boardHost, 8, 300, ShakeAxis.Vertical);

                    HandleStep(result, allowModeMessageOverride: false);
                }
                break;

            case GameMode.PhantomFog:
                _canvas.FogRevealed = DateTime.Now < _fogRevealUntil;
                break;
        }
    }

    private void GameForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (HandleKeyInput(e.KeyCode))
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private bool HoldPiece()
    {
        if (!_engine.HoldCurrentPiece())
            return false;

        AudioManager.Instance.PlaySfx("rotate.wav", 0.45f);

        if (_engine.IsGameOver)
            EndGame();

        return true;
    }

    private async Task ActivateRelicAsync()
    {
        SpecialItemResult? result = _engine.ActivateRelic();
        if (result is null)
            return;

        AudioManager.Instance.PlaySfx(result.Definition.SfxFile);
        Color relicColor = RelicColor(result.Definition.Type);
        _eventBanner.ShowEvent(result.Definition.DisplayName.ToUpperInvariant(), $"{result.CellsCleared} CELLS DESTROYED", relicColor, 1050);

        if (PreferencesManager.Current.ParticlesEnabled)
            _canvas.TriggerBurst(result.CenterRow, result.CenterCol, relicColor, 24 + result.Definition.ShakeIntensity * 2, 120 + result.Definition.ShakeIntensity * 4);
        else
            _canvas.TriggerBoardPulse(relicColor, 110);

        RefreshScene();

        if (PreferencesManager.Current.ScreenShakeEnabled)
        {
            await _shake.ShakeAsync(
                _boardHost,
                result.Definition.ShakeIntensity,
                result.Definition.ShakeDurationMs,
                result.Definition.ShakeAxis);
        }
    }

    private void HandleStep(GameStepResult result, bool allowModeMessageOverride = true)
    {
        if (result.LinesCleared > 0)
        {
            AudioManager.Instance.PlaySfx("line_clear.wav");
            _canvas.TriggerBoardPulse(_theme.Accent, result.LinesCleared >= 4 ? 120 : 70);
            _canvas.TriggerLineClear(result.LinesCleared, _engine.Gravity, _theme.Accent);

            if (result.BackToBackBonus)
                _eventBanner.ShowEvent("BACK-TO-BACK!", $"TETRIS CHAIN • +{result.ScoreDelta:N0}", Color.Gold, 1150);
            else if (result.Combo >= 3)
                _eventBanner.ShowEvent($"COMBO x{result.Combo}", $"+{result.ScoreDelta:N0} SCORE", _theme.Accent, 900);
            else if (result.LinesCleared >= 4)
                _eventBanner.ShowEvent("TETRIS!", $"+{result.ScoreDelta:N0} SCORE", _theme.Accent, 950);

            if (_mode == GameMode.PhantomFog)
            {
                _fogRevealUntil = DateTime.Now.AddSeconds(2.5);
                _canvas.FogRevealed = true;
                AudioManager.Instance.PlaySfx("fog_reveal.wav", 0.7f);
                _eventBanner.ShowEvent("VISION RESTORED", "THE FOG RETREATS FOR A MOMENT", _theme.Accent, 1000);
            }
        }
        else if (result.RelicGranted is not null)
        {
            SpecialItemDefinition definition = SpecialItemDefinition.All[result.RelicGranted.Value];
            AudioManager.Instance.PlaySfx("relic_ready.wav", 0.8f);
            _eventBanner.ShowEvent("RELIC READY", definition.DisplayName.ToUpperInvariant() + "  •  PRESS F", RelicColor(definition.Type), 1200);
        }

        _dropTimer.Interval = _engine.DropIntervalMs;
        RefreshScene();

        if (result.GameOver || _engine.IsGameOver)
            EndGame();
    }

    private async void EndGame()
    {
        if (_endingGame) return;
        _endingGame = true;
        _countdownActive = false;
        _dropTimer.Stop();
        _modeTimer.Stop();
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySfx("game_over.wav");

        _canvas.ShowOverlay = true;
        _canvas.OverlayTitle = "GAME OVER";
        _canvas.OverlaySubtitle = $"SCORE  {_engine.Score:N0}";
        _canvas.Invalidate();

        var record = new ScoreRecord(_playerName, _engine.Score, _mode, DateTime.Now);
        ScoreManager.Save(record);

        _eventBanner.ShowEvent("RUN COMPLETE", $"SCORE  {_engine.Score:N0}", _theme.Danger, 700);
        await Task.Delay(700);
        if (IsDisposed || Disposing) return;

        using var resultForm = new GameOverForm(record);
        resultForm.ShowDialog(this);

        if (resultForm.PlayAgain)
            StartSession();
        else
            Close();
    }

    private void TogglePause()
    {
        if (_endingGame || _countdownActive) return;

        _paused = !_paused;
        if (_paused)
        {
            _dropTimer.Stop();
            _modeTimer.Stop();
            _canvas.ShowOverlay = true;
            _canvas.OverlayTitle = "PAUSED";
            _canvas.OverlaySubtitle = "PRESS P TO RESUME";
        }
        else
        {
            _dropTimer.Start();
            _modeTimer.Start();
            _canvas.ShowOverlay = false;
        }

        RefreshScene();
    }

    private void RefreshScene()
    {
        _canvas.Board = _engine.Board;
        _canvas.ActivePiece = _engine.IsGameOver ? null : _engine.CurrentPiece;
        _canvas.GhostPiece = !_engine.IsGameOver && PreferencesManager.Current.GhostPieceEnabled ? _engine.GetGhostPiece() : null;
        _canvas.Gravity = _engine.Gravity;
        _canvas.Mode = _mode;
        _canvas.ModeEventSeconds = ModeEventSeconds();
        _canvas.FogEnabled = _mode == GameMode.PhantomFog;
        _canvas.FogRows = Math.Clamp(4 + _elapsedSeconds / 24 + (_engine.Level - 1), 4, 11);
        _canvas.FogRevealed = DateTime.Now < _fogRevealUntil;

        if (_paused)
        {
            _canvas.ShowOverlay = true;
            _canvas.OverlayTitle = "PAUSED";
            _canvas.OverlaySubtitle = "PRESS P TO RESUME";
        }
        else if (!_countdownActive && !_endingGame)
        {
            _canvas.ShowOverlay = false;
        }

        _canvas.Invalidate();

        _nextPreview.PieceType = _engine.NextPieceType;
        _nextPreview.Invalidate();
        _holdPreview.PieceType = _engine.HeldPieceType;
        _holdPreview.BackColor = _engine.CanHold ? Color.FromArgb(16, 20, 36) : Color.FromArgb(35, 20, 28);
        _holdPreview.Invalidate();

        _levelValue.Text = _engine.Level.ToString();
        _linesValue.Text = _engine.Lines.ToString();
        _comboValue.Text = _engine.ComboStreak > 1 ? $"x{_engine.ComboStreak}" : "—";
        _b2bValue.Text = _engine.BackToBackActive ? "ON" : "—";
        _b2bValue.ForeColor = _engine.BackToBackActive ? Color.Gold : Color.Violet;
        UpdateScorePresentation();
        UpdateArcaneSkillInfo();
    }


    private void UpdateArcaneSkillInfo()
    {
        if (_mode != GameMode.ArcaneChaos)
        {
            _skillPanel.Visible = false;
            return;
        }

        _skillPanel.Visible = true;

        if (_engine.CurrentRelic is null)
        {
            int remaining = Math.Max(0, 6 - (_engine.PiecesLocked % 6));
            _skillTitle.Text = "RELIC: RECHARGING";
            _skillTitle.ForeColor = Color.Silver;
            _skillDescription.Text = $"{remaining} LOCK(S) LEFT";
            _skillDescription.ForeColor = Color.Silver;
            return;
        }

        SpecialItemDefinition definition = SpecialItemDefinition.All[_engine.CurrentRelic.Value];
        Color relicColor = RelicColor(definition.Type);
        _skillTitle.Text = $"RELIC: {definition.DisplayName.ToUpperInvariant()}  [F]";
        _skillTitle.ForeColor = relicColor;
        _skillDescription.Text = definition.Description;
        _skillDescription.ForeColor = relicColor;
    }

    private void UpdateScorePresentation()
    {
        string scoreText = _engine.Score.ToString("N0");
        _scoreValue.Text = scoreText;
        float size = scoreText.Length switch
        {
            <= 5 => 40f,
            <= 7 => 34f,
            <= 9 => 28f,
            _ => 23f
        };
        if (Math.Abs(_scoreValue.Font.Size - size) > 0.1f)
            _scoreValue.Font = new Font("Segoe UI", size, FontStyle.Bold);
    }

    private void ShowUpcomingModeWarning()
    {
        int remaining = ModeEventSeconds();
        if (remaining is < 1 or > 3)
        {
            if (remaining > 3) _lastCountdownShown = -1;
            return;
        }

        if (_lastCountdownShown == remaining) return;
        _lastCountdownShown = remaining;

        string title = _mode == GameMode.ZeroGRift ? "RIFT INCOMING" : "TIDE RISING";
        string subtitle = $"{remaining}...";
        Color color = _mode == GameMode.ZeroGRift ? _theme.Accent : _theme.Danger;
        _eventBanner.ShowEvent(title, subtitle, color, 760);
        AudioManager.Instance.PlaySfx("warning_tick.wav", 0.55f);
    }

    private int ModeEventSeconds() => _mode switch
    {
        GameMode.ZeroGRift => Math.Max(0, _nextGravityShiftAt - _elapsedSeconds),
        GameMode.CursedTide => Math.Max(0, _nextTideRiseAt - _elapsedSeconds),
        _ => int.MaxValue
    };

    private static Color RelicColor(SpecialItemType type) => type switch
    {
        SpecialItemType.RuneBomb => Color.MediumPurple,
        SpecialItemType.DragonBreath => Color.OrangeRed,
        SpecialItemType.ThunderSpear => Color.DeepSkyBlue,
        SpecialItemType.VoidCross => Color.HotPink,
        SpecialItemType.PrismRelic => Color.Cyan,
        SpecialItemType.MeteorRelic => Color.Gold,
        SpecialItemType.BlackHole => Color.DarkViolet,
        SpecialItemType.PhoenixSigil => Color.OrangeRed,
        SpecialItemType.ChaosDice => Color.LimeGreen,
        _ => Color.White
    };

    private static string GravityArrow(GravityDirection direction) => direction switch
    {
        GravityDirection.Down => "↓",
        GravityDirection.Right => "→",
        GravityDirection.Up => "↑",
        GravityDirection.Left => "←",
        _ => "↓"
    };

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        Keys keyCode = keyData & Keys.KeyCode;
        if (HandleKeyInput(keyCode))
            return true;

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool HandleKeyInput(Keys keyCode)
    {
        if (_endingGame || _countdownActive) return false;

        if (keyCode is Keys.P or Keys.Escape)
        {
            TogglePause();
            return true;
        }

        if (_paused) return false;

        bool changed = false;
        switch (keyCode)
        {
            case Keys.A:
            case Keys.Left:
                changed = _engine.TryMoveSide(-1);
                break;
            case Keys.D:
            case Keys.Right:
                changed = _engine.TryMoveSide(1);
                break;
            case Keys.W:
            case Keys.Up:
                changed = _engine.RotateClockwise();
                if (changed) AudioManager.Instance.PlaySfx("rotate.wav", 0.55f);
                break;
            case Keys.S:
            case Keys.Down:
                HandleStep(_engine.SoftDrop());
                changed = true;
                break;
            case Keys.Space:
                GameStepResult hardDrop = _engine.HardDrop();
                _canvas.TriggerImpact(_engine.Gravity, _theme.Accent);
                AudioManager.Instance.PlaySfx("hard_drop.wav", 0.7f);
                HandleStep(hardDrop);
                changed = true;
                break;
            case Keys.C:
                changed = HoldPiece();
                break;
            case Keys.F when _mode == GameMode.ArcaneChaos:
                _ = ActivateRelicAsync();
                changed = true;
                break;
        }

        if (changed)
        {
            RefreshScene();
            return true;
        }

        return false;
    }

}
