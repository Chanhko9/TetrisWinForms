using TetrisWinForms.Core;
using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class ModeSelectForm
{
    private readonly Label _title = new();
    private readonly List<Panel> _cards = new();
    private readonly Button _back = new();

    private void InitializeComponent()
    {
        Text = "Select Mode";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 820);
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(8, 10, 22);
        BackgroundImage = ImageManager.Get("bg_zero_g_rift.png");
        BackgroundImageLayout = ImageLayout.Stretch;

        _title.Text = "SELECT MODE";
        _title.ForeColor = Color.White;
        _title.Font = new Font("Segoe UI", 40, FontStyle.Bold);
        _title.AutoSize = false;
        _title.Height = 82;
        _title.BackColor = Color.Transparent;
        _title.TextAlign = ContentAlignment.MiddleCenter;
        Controls.Add(_title);

        foreach (GameModeProfile profile in GameModeProfile.All.Values
                     .OrderBy(profile => DifficultyRank(profile.Difficulty)))
        {
            Panel card = CreateModeCard(profile);
            _cards.Add(card);
            Controls.Add(card);
        }

        _back.Text = "BACK";
        _back.Size = new Size(190, 54);
        _back.FlatStyle = FlatStyle.Flat;
        _back.BackColor = Color.FromArgb(23, 30, 55);
        _back.ForeColor = Color.White;
        _back.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        _back.Cursor = Cursors.Hand;
        _back.TabStop = false;
        _back.FlatAppearance.BorderColor = Color.FromArgb(70, 120, 190);
        _back.FlatAppearance.BorderSize = 2;
        _back.Click += (_, _) => Close();
        Controls.Add(_back);

        LayoutUi();
    }

    private void LayoutUi()
    {
        _title.SetBounds(0, 10, ClientSize.Width, 82);

        int cardWidth = Math.Min(1420, ClientSize.Width - 120);
        int cardHeight = 104;
        int gap = 12;
        int totalCardsHeight = _cards.Count * cardHeight + (_cards.Count - 1) * gap;
        int blockHeight = totalCardsHeight + 72;
        int y = Math.Max(98, (ClientSize.Height - blockHeight) / 2 + 10);
        int x = (ClientSize.Width - cardWidth) / 2;

        foreach (Panel card in _cards)
        {
            card.SetBounds(x, y, cardWidth, cardHeight);
            y += cardHeight + gap;
        }

        _back.Location = new Point((ClientSize.Width - _back.Width) / 2, y + 10);
    }

    private Panel CreateModeCard(GameModeProfile profile)
    {
        ModeVisualTheme theme = ModeVisualTheme.Get(profile.Mode);
        var panel = new Panel
        {
            BackColor = Color.FromArgb(20, 25, 46),
            Cursor = Cursors.Hand,
            Tag = profile.Mode
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(210, theme.Accent), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };

        var difficulty = new Label
        {
            Text = profile.Difficulty,
            ForeColor = DifficultyColor(profile.Difficulty),
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(126, 42),
            Location = new Point(16, 31),
            BackColor = Color.FromArgb(30, 38, 68)
        };
        panel.Controls.Add(difficulty);

        var picture = new PictureBox
        {
            Image = ImageManager.Get(theme.ThumbnailImage),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(10, 14, 28),
            Size = new Size(220, 78),
            Location = new Point(158, 13)
        };
        panel.Controls.Add(picture);

        panel.Controls.Add(new Label
        {
            Text = profile.DisplayName,
            ForeColor = theme.Accent,
            Font = new Font("Segoe UI", 23f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(412, 31),
            BackColor = Color.Transparent
        });

        var play = new Button
        {
            Text = "PLAY",
            Size = new Size(172, 54),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(26, 36, 64),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        play.FlatAppearance.BorderColor = theme.Accent;
        play.FlatAppearance.BorderSize = 2;
        play.Location = new Point(panel.Width - 198, 25);
        play.Click += (_, _) => Launch(profile.Mode);
        play.MouseEnter += (_, _) => play.BackColor = Color.FromArgb(46, 58, 92);
        play.MouseLeave += (_, _) => play.BackColor = Color.FromArgb(26, 36, 64);
        panel.Controls.Add(play);

        panel.Resize += (_, _) => play.Location = new Point(panel.Width - 198, 25);
        panel.DoubleClick += (_, _) => Launch(profile.Mode);
        picture.DoubleClick += (_, _) => Launch(profile.Mode);
        return panel;
    }

    private static int DifficultyRank(string difficulty)
        => difficulty switch
        {
            "EASY" => 0,
            "EASY+" => 1,
            "MEDIUM+" => 2,
            "HARD" => 3,
            "HARD+" => 4,
            "EXTREME" => 5,
            _ => 99
        };

    private static Color DifficultyColor(string difficulty)
        => difficulty switch
        {
            "EASY" => Color.LightGreen,
            "EASY+" => Color.PaleGreen,
            "MEDIUM+" => Color.Gold,
            "HARD" => Color.Orange,
            "HARD+" => Color.OrangeRed,
            "EXTREME" => Color.HotPink,
            _ => Color.White
        };
}
