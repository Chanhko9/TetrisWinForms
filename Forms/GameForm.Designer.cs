using TetrisWinForms.Controls;
using TetrisWinForms.Core;
using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class GameForm
{
    private readonly BoardCanvas _canvas = new();
    private readonly TetrominoPreview _nextPreview = new();
    private readonly TetrominoPreview _holdPreview = new();
    private readonly EventBanner _eventBanner = new();
    private readonly Label _scoreValue = new();
    private readonly Label _levelValue = new();
    private readonly Label _linesValue = new();
    private readonly Label _comboValue = new();
    private readonly Label _b2bValue = new();
    private readonly Panel _skillPanel = new();
    private readonly Label _skillTitle = new();
    private readonly Label _skillDescription = new();
    private readonly Panel _sidePanel = new();
    private readonly Panel _volumeBar = new();
    private readonly Panel _volumeFill = new();
    private readonly Label _volumeValue = new();
    private readonly Button _volumeMinusButton = new();
    private readonly Button _volumePlusButton = new();
    private readonly Button _volumeMuteButton = new();
    private Button _pauseButton = null!;
    private Button _backButton = null!;
    private Panel _boardHost = null!;

    private void InitializeComponent()
    {
        Text = $"TETRIS - {_profile.DisplayName}";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 820);
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.FromArgb(9, 12, 24);
        BackgroundImage = ImageManager.Get(_theme.BackgroundImage);
        BackgroundImageLayout = ImageLayout.Stretch;
        KeyPreview = true;

        _boardHost = new Panel { BackColor = Color.Black };
        Controls.Add(_boardHost);
        _boardHost.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, _theme.Accent), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, _boardHost.Width - 1, _boardHost.Height - 1);
        };

        _canvas.Dock = DockStyle.Fill;
        _boardHost.Controls.Add(_canvas);

        _sidePanel.BackColor = Color.FromArgb(216, 10, 14, 30);
        Controls.Add(_sidePanel);

        BuildInfoPanel();
        BuildVolumeControl();

        _eventBanner.Size = new Size(440, 90);
        Controls.Add(_eventBanner);
        _eventBanner.BringToFront();
        LayoutGameUi();
    }

    private void BuildInfoPanel()
    {
        _sidePanel.Controls.Add(MakeTitle("NEXT", 38, 30));
        _nextPreview.Location = new Point(38, 66);
        _nextPreview.Size = new Size(210, 160);
        _sidePanel.Controls.Add(_nextPreview);

        _sidePanel.Controls.Add(MakeTitle("HOLD [C]", 280, 30));
        _holdPreview.Location = new Point(280, 66);
        _holdPreview.Size = new Size(210, 160);
        _holdPreview.EmptyText = "EMPTY";
        _sidePanel.Controls.Add(_holdPreview);

        _sidePanel.Controls.Add(MakeTitle("SCORE", 510, 30));
        SetupScoreValue(_scoreValue, 500, 60);
        _sidePanel.Controls.Add(_scoreValue);

        _sidePanel.Controls.Add(MakeTitle("LEVEL", 38, 248));
        SetupValueLabel(_levelValue, 38, 286, Color.White, 28);
        _sidePanel.Controls.Add(_levelValue);

        _sidePanel.Controls.Add(MakeTitle("LINES", 170, 248));
        SetupValueLabel(_linesValue, 170, 286, Color.White, 28);
        _sidePanel.Controls.Add(_linesValue);

        _sidePanel.Controls.Add(MakeTitle("COMBO", 310, 248));
        SetupValueLabel(_comboValue, 310, 286, Color.DeepSkyBlue, 28);
        _sidePanel.Controls.Add(_comboValue);

        _sidePanel.Controls.Add(MakeTitle("B2B", 470, 248));
        SetupValueLabel(_b2bValue, 470, 290, Color.Violet, 22);
        _sidePanel.Controls.Add(_b2bValue);

        _skillPanel.Location = new Point(28, 360);
        _skillPanel.Size = new Size(624, 92);
        _skillPanel.BackColor = Color.FromArgb(105, 8, 12, 28);
        _skillPanel.Visible = _mode == GameMode.ArcaneChaos;
        _sidePanel.Controls.Add(_skillPanel);

        _skillTitle.AutoSize = true;
        _skillTitle.MaximumSize = new Size(596, 0);
        _skillTitle.Location = new Point(12, 10);
        _skillTitle.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        _skillTitle.ForeColor = Color.Violet;
        _skillTitle.BackColor = Color.Transparent;
        _skillTitle.Visible = true;
        _skillPanel.Controls.Add(_skillTitle);

        _skillDescription.AutoSize = true;
        _skillDescription.MaximumSize = new Size(596, 0);
        _skillDescription.Location = new Point(12, 44);
        _skillDescription.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        _skillDescription.ForeColor = Color.Violet;
        _skillDescription.BackColor = Color.Transparent;
        _skillDescription.Visible = true;
        _skillPanel.Controls.Add(_skillDescription);

        _pauseButton = CreateButton("PAUSE / RESUME  [P]", 38, 548, 280);
        _pauseButton.Height = 56;
        _pauseButton.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        _pauseButton.Click += (_, _) => TogglePause();
        _sidePanel.Controls.Add(_pauseButton);

        _backButton = CreateButton("BACK TO MODES", 338, 548, 280);
        _backButton.Height = 56;
        _backButton.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        _backButton.Click += (_, _) => Close();
        _sidePanel.Controls.Add(_backButton);
    }

    private void BuildVolumeControl()
    {
        _sidePanel.Controls.Add(MakeTitle("VOLUME", 38, 444));

        _volumeBar.Size = new Size(370, 16);
        _volumeBar.Location = new Point(38, 482);
        _volumeBar.BackColor = Color.FromArgb(45, 58, 92);
        _volumeBar.Cursor = Cursors.Hand;
        _volumeBar.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(90, 135, 215), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, _volumeBar.Width - 1, _volumeBar.Height - 1);
        };
        _volumeBar.MouseDown += (_, e) => SetGameVolumeFromMouse(e.X);
        _volumeBar.MouseMove += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                SetGameVolumeFromMouse(e.X);
        };
        _volumeBar.Controls.Add(_volumeFill);
        _sidePanel.Controls.Add(_volumeBar);

        _volumeFill.BackColor = _theme.Accent;
        _volumeFill.Location = Point.Empty;
        _volumeFill.Height = _volumeBar.Height;

        _volumeValue.ForeColor = Color.White;
        _volumeValue.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        _volumeValue.AutoSize = false;
        _volumeValue.Size = new Size(72, 30);
        _volumeValue.TextAlign = ContentAlignment.MiddleCenter;
        _volumeValue.Location = new Point(416, 474);
        _volumeValue.BackColor = Color.Transparent;
        _sidePanel.Controls.Add(_volumeValue);

        ConfigureVolumeButton(_volumeMinusButton, "−", 494);
        _volumeMinusButton.Click += (_, _) => AdjustGameVolume(-0.10f);
        _sidePanel.Controls.Add(_volumeMinusButton);

        ConfigureVolumeButton(_volumePlusButton, "+", 544);
        _volumePlusButton.Click += (_, _) => AdjustGameVolume(0.10f);
        _sidePanel.Controls.Add(_volumePlusButton);

        ConfigureVolumeButton(_volumeMuteButton, "M", 594);
        _volumeMuteButton.Click += (_, _) =>
        {
            AudioManager.Instance.ToggleMute();
            RefreshGameVolumeUi();
        };
        _sidePanel.Controls.Add(_volumeMuteButton);

        RefreshGameVolumeUi();
    }

    private void ConfigureVolumeButton(Button button, string text, int x)
    {
        button.Text = text;
        button.Location = new Point(x, 468);
        button.Size = new Size(42, 40);
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.FromArgb(27, 35, 63);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.TabStop = false;
        button.FlatAppearance.BorderColor = _theme.Accent;
        button.FlatAppearance.BorderSize = 1;
    }

    private void LayoutGameUi()
    {
        int boardHeight = Math.Min(ClientSize.Height - 4, 1080);
        int boardWidth = boardHeight / 2;
        int sideWidth = 720;
        int gap = 18;
        int totalWidth = boardWidth + gap + sideWidth;
        int startX = Math.Max(2, (ClientSize.Width - totalWidth) / 2);
        int top = Math.Max(2, (ClientSize.Height - boardHeight) / 2);

        _boardHost.Size = new Size(boardWidth, boardHeight);
        _boardHost.Location = new Point(startX, top);

        _sidePanel.Size = new Size(sideWidth, 650);
        _sidePanel.Location = new Point(_boardHost.Right + gap, top + Math.Max(0, (boardHeight - _sidePanel.Height) / 2));

        _eventBanner.Location = new Point(
            _boardHost.Left + Math.Max(12, (_boardHost.Width - _eventBanner.Width) / 2),
            _boardHost.Top + (_boardHost.Height / 2) - 45);
    }

    private static Label MakeTitle(string text, int x, int y) => new()
    {
        Text = text,
        ForeColor = Color.Silver,
        Font = new Font("Segoe UI", 13, FontStyle.Bold),
        AutoSize = true,
        Location = new Point(x, y),
        BackColor = Color.Transparent
    };

    private static void SetupValueLabel(Label label, int x, int y, Color color, float fontSize)
    {
        label.Text = "0";
        label.ForeColor = color;
        label.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
        label.AutoSize = true;
        label.Location = new Point(x, y);
        label.BackColor = Color.Transparent;
    }

    private static void SetupScoreValue(Label label, int x, int y)
    {
        label.Text = "0";
        label.ForeColor = Color.Gold;
        label.Font = new Font("Segoe UI", 36, FontStyle.Bold);
        label.AutoSize = false;
        label.Size = new Size(165, 92);
        label.TextAlign = ContentAlignment.TopCenter;
        label.Location = new Point(x, y);
        label.BackColor = Color.Transparent;
        label.Padding = new Padding(0, 2, 0, 0);
    }

    private static Button CreateButton(string text, int x, int y, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 39, 67),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TabStop = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(70, 120, 190);
        button.MouseEnter += (_, _) => button.BackColor = Color.FromArgb(43, 55, 90);
        button.MouseLeave += (_, _) => button.BackColor = Color.FromArgb(30, 39, 67);
        return button;
    }
}
