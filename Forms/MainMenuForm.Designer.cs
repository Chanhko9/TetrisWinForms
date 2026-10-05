using TetrisWinForms.Controls;
using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class MainMenuForm
{
    private readonly Panel _menuPanel = new();
    private readonly TetrisLogoControl _logo = new();
    private readonly Panel _audioPanel = new();
    private readonly Panel _volumeBar = new();
    private readonly Panel _volumeFill = new();
    private readonly Label _volumeValue = new();
    private readonly Button _playButton = new();
    private readonly Button _historyButton = new();
    private readonly Button _howToButton = new();
    private readonly Button _exitButton = new();
    private readonly Button _minusButton = new();
    private readonly Button _plusButton = new();
    private readonly Button _muteButton = new();
    private readonly Label _groupInfoLabel = new();

    private void InitializeComponent()
    {
        Text = "TETRIS";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 820);
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(10, 12, 26);
        BackgroundImage = ImageManager.Get("bg_basic.png");
        BackgroundImageLayout = ImageLayout.Stretch;

        _menuPanel.BackColor = Color.FromArgb(158, 5, 9, 23);
        Controls.Add(_menuPanel);

        _logo.AccentColor = Color.DeepSkyBlue;
        _menuPanel.Controls.Add(_logo);

        ConfigureMenuButton(_playButton, "PLAY", Color.DeepSkyBlue);
        _playButton.Click += (_, _) => OpenModeSelect();
        _menuPanel.Controls.Add(_playButton);

        ConfigureMenuButton(_historyButton, "SCORE HISTORY", Color.MediumPurple);
        _historyButton.Click += (_, _) => OpenScoreHistory();
        _menuPanel.Controls.Add(_historyButton);

        ConfigureMenuButton(_howToButton, "HOW TO PLAY", Color.CadetBlue);
        _howToButton.Click += (_, _) => OpenHowToPlay();
        _menuPanel.Controls.Add(_howToButton);

        ConfigureMenuButton(_exitButton, "EXIT", Color.IndianRed);
        _exitButton.Click += (_, _) => Close();
        _menuPanel.Controls.Add(_exitButton);


        _groupInfoLabel.Text = "Sunrise-MSang_VMinh_PNam_MQuân";
        _groupInfoLabel.ForeColor = Color.FromArgb(155, 185, 210);
        _groupInfoLabel.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        _groupInfoLabel.AutoSize = false;
        _groupInfoLabel.TextAlign = ContentAlignment.MiddleCenter;
        _groupInfoLabel.BackColor = Color.Transparent;
        _menuPanel.Controls.Add(_groupInfoLabel);

        _audioPanel.BackColor = Color.FromArgb(198, 12, 18, 35);
        _menuPanel.Controls.Add(_audioPanel);

        _audioPanel.Controls.Add(new Label
        {
            Text = "VOLUME",
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(26, 28),
            BackColor = Color.Transparent
        });

        _volumeBar.Height = 16;
        _volumeBar.BackColor = Color.FromArgb(43, 55, 87);
        _volumeBar.Cursor = Cursors.Hand;
        _volumeBar.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(90, 135, 215), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, _volumeBar.Width - 1, _volumeBar.Height - 1);
        };
        _volumeBar.MouseDown += (_, e) => SetVolumeFromMouse(e.X);
        _volumeBar.MouseMove += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                SetVolumeFromMouse(e.X);
        };
        _volumeBar.Controls.Add(_volumeFill);
        _audioPanel.Controls.Add(_volumeBar);

        _volumeFill.BackColor = Color.DeepSkyBlue;
        _volumeFill.Location = Point.Empty;
        _volumeFill.Height = _volumeBar.Height;

        _volumeValue.ForeColor = Color.White;
        _volumeValue.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        _volumeValue.AutoSize = false;
        _volumeValue.Size = new Size(74, 30);
        _volumeValue.TextAlign = ContentAlignment.MiddleCenter;
        _audioPanel.Controls.Add(_volumeValue);

        ConfigureMiniButton(_minusButton, "−");
        _minusButton.Click += (_, _) => AdjustVolume(-0.10f);
        _audioPanel.Controls.Add(_minusButton);

        ConfigureMiniButton(_plusButton, "+");
        _plusButton.Click += (_, _) => AdjustVolume(0.10f);
        _audioPanel.Controls.Add(_plusButton);

        ConfigureMiniButton(_muteButton, "M");
        _muteButton.Click += (_, _) =>
        {
            AudioManager.Instance.ToggleMute();
            RefreshVolumeUi();
        };
        _audioPanel.Controls.Add(_muteButton);

        LayoutUi();
        RefreshVolumeUi();
    }

    private void LayoutUi()
    {
        int panelWidth = Math.Min(1040, ClientSize.Width - 160);
        int panelHeight = Math.Min(880, ClientSize.Height - 80);
        _menuPanel.Size = new Size(panelWidth, panelHeight);
        _menuPanel.Location = new Point((ClientSize.Width - panelWidth) / 2, (ClientSize.Height - panelHeight) / 2);

        _logo.SetBounds(80, 18, panelWidth - 160, 155);

        int buttonWidth = Math.Min(560, panelWidth - 260);
        int buttonX = (panelWidth - buttonWidth) / 2;
        int startY = 205;
        int gap = 84;
        Button[] buttons = [_playButton, _historyButton, _howToButton, _exitButton];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Size = new Size(buttonWidth, 68);
            buttons[i].Location = new Point(buttonX, startY + i * gap);
        }

        _groupInfoLabel.SetBounds(0, startY + buttons.Length * gap + 8, panelWidth, 30);

        _audioPanel.Size = new Size(Math.Min(860, panelWidth - 110), 90);
        _audioPanel.Location = new Point((panelWidth - _audioPanel.Width) / 2, panelHeight - 120);

        _volumeBar.Location = new Point(170, 35);
        _volumeBar.Width = Math.Max(330, _audioPanel.Width - 500);
        _volumeValue.Location = new Point(_volumeBar.Right + 18, 28);
        _minusButton.Location = new Point(_audioPanel.Width - 162, 24);
        _plusButton.Location = new Point(_audioPanel.Width - 108, 24);
        _muteButton.Location = new Point(_audioPanel.Width - 54, 24);
        RefreshVolumeUi();
    }

    private static void ConfigureMiniButton(Button button, string text)
    {
        button.Text = text;
        button.Size = new Size(42, 40);
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.FromArgb(27, 35, 63);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.TabStop = false;
        button.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        button.FlatAppearance.BorderSize = 1;
    }

    private static void ConfigureMenuButton(Button button, string text, Color accent)
    {
        button.Text = text;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Color.FromArgb(20, 28, 52);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.TabStop = false;
        button.FlatAppearance.BorderColor = accent;
        button.FlatAppearance.BorderSize = 2;
        button.MouseEnter += (_, _) => button.BackColor = Color.FromArgb(43, 55, 90);
        button.MouseLeave += (_, _) => button.BackColor = Color.FromArgb(20, 28, 52);
    }
}
