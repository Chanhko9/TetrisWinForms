namespace TetrisWinForms.Forms;

public sealed class DuelResultForm : Form
{
    public bool Rematch { get; private set; }

    public DuelResultForm(string player1, int score1, string player2, int score2)
    {
        Text = "Duel Result";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 470);
        BackColor = Color.FromArgb(10, 14, 29);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        string result = score1 == score2
            ? "DRAW"
            : score1 > score2 ? $"{player1.ToUpperInvariant()} WINS" : $"{player2.ToUpperInvariant()} WINS";

        Controls.Add(new Label
        {
            Text = "TIME UP!",
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(720, 54),
            Location = new Point(20, 30)
        });

        Controls.Add(new Label
        {
            Text = result,
            ForeColor = score1 == score2 ? Color.White : Color.DeepSkyBlue,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(720, 66),
            Location = new Point(20, 86)
        });

        AddScoreCard(player1, score1, 70, Color.DeepSkyBlue);
        AddScoreCard(player2, score2, 405, Color.OrangeRed);

        var rematch = MakeButton("REMATCH", 165, 385, 190);
        rematch.Click += (_, _) => { Rematch = true; Close(); };
        Controls.Add(rematch);

        var back = MakeButton("BACK TO MODES", 405, 385, 190);
        back.Click += (_, _) => Close();
        Controls.Add(back);
    }

    private void AddScoreCard(string name, int score, int x, Color accent)
    {
        var panel = new Panel
        {
            Location = new Point(x, 180),
            Size = new Size(285, 155),
            BackColor = Color.FromArgb(20, 26, 47)
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(accent, 2);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        Controls.Add(panel);

        panel.Controls.Add(new Label
        {
            Text = name,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(265, 34),
            Location = new Point(10, 14)
        });

        panel.Controls.Add(new Label
        {
            Text = score.ToString("N0"),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 34, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(265, 70),
            Location = new Point(10, 54)
        });
    }

    private static Button MakeButton(string text, int x, int y, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 48),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(31, 42, 74),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            TabStop = false
        };
        button.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        button.FlatAppearance.BorderSize = 2;
        return button;
    }
}
