using TetrisWinForms;
using TetrisWinForms.Core;
using TetrisWinForms.Managers;
using TetrisWinForms.Models;

namespace TetrisWinForms.Forms;

public sealed class GameOverForm : Form
{
    public bool PlayAgain { get; private set; }

    public GameOverForm(ScoreRecord current)
    {
        Text = "Game Over";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(920, 650);
        BackColor = Color.FromArgb(11, 14, 28);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi(current);
    }

    private void BuildUi(ScoreRecord current)
    {
        ModeVisualTheme theme = ModeVisualTheme.Get(current.Mode);

        var shell = new Panel
        {
            Location = new Point(28, 22),
            Size = new Size(864, 596),
            BackColor = Color.FromArgb(18, 22, 40)
        };
        shell.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(170, theme.Accent), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, shell.Width - 1, shell.Height - 1);
        };
        Controls.Add(shell);

        var title = new Label
        {
            Text = "GAME OVER",
            ForeColor = theme.Danger,
            Font = new Font("Segoe UI", 29, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(shell.Width, 82),
            Location = new Point(0, 12),
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };
        shell.Controls.Add(title);

        var scorePanel = new Panel
        {
            Size = new Size(250, 118),
            Location = new Point((shell.Width - 250) / 2, 98),
            BackColor = Color.FromArgb(10, 14, 28)
        };
        shell.Controls.Add(scorePanel);

        var scoreValue = new Label
        {
            Text = current.Score.ToString("N0"),
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 30, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(250, 84),
            Location = new Point(0, 0),
            BackColor = Color.Transparent
        };
        scorePanel.Controls.Add(scoreValue);

        scorePanel.Controls.Add(new Label
        {
            Text = "SCORE",
            ForeColor = Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(250, 28),
            Location = new Point(0, 86),
            BackColor = Color.Transparent
        });

        List<ScoreRecord> allModeScores = ScoreManager.LoadAll()
            .Where(x => x.Mode == current.Mode)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.PlayedAt)
            .ToList();

        int currentRank = allModeScores.FindIndex(x =>
            x.PlayerName == current.PlayerName &&
            x.Score == current.Score &&
            Math.Abs((x.PlayedAt - current.PlayedAt).TotalSeconds) < 2) + 1;

        if (currentRank <= 0)
            currentRank = allModeScores.Count(x => x.Score > current.Score) + 1;

        shell.Controls.Add(new Label
        {
            Text = currentRank == 1 ? "★ NEW MODE BEST ★" : $"MODE RANK  #{currentRank}",
            ForeColor = currentRank == 1 ? Color.Gold : theme.Accent,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(shell.Width, 28),
            Location = new Point(0, 226),
            BackColor = Color.Transparent
        });

        var grid = new DataGridView
        {
            Location = new Point(42, 264),
            Size = new Size(780, 190),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.FromArgb(17, 21, 38),
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ColumnHeadersHeight = 34
        };
        grid.DefaultCellStyle.BackColor = Color.FromArgb(23, 28, 49);
        grid.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 73, 119);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 39, 68);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;

        grid.Columns.Add("Rank", "#");
        grid.Columns.Add("Player", "PLAYER");
        grid.Columns.Add("Score", "SCORE");
        grid.Columns.Add("Mode", "MODE");
        grid.Columns.Add("Time", "PLAYED AT");
        grid.Columns[0].FillWeight = 35;
        grid.Columns[1].FillWeight = 110;
        grid.Columns[2].FillWeight = 75;
        grid.Columns[3].FillWeight = 120;
        grid.Columns[4].FillWeight = 120;

        List<ScoreRecord> scores = allModeScores.Take(10).ToList();
        for (int i = 0; i < scores.Count; i++)
        {
            ScoreRecord row = scores[i];
            int index = grid.Rows.Add(
                i + 1,
                row.PlayerName,
                row.Score.ToString("N0"),
                GameModeProfile.Get(row.Mode).DisplayName,
                row.PlayedAt.ToString("dd/MM/yyyy HH:mm"));

            if (row.PlayerName == current.PlayerName &&
                row.Score == current.Score &&
                Math.Abs((row.PlayedAt - current.PlayedAt).TotalSeconds) < 2)
            {
                grid.Rows[index].DefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                grid.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(75, 61, 18);
                grid.Rows[index].DefaultCellStyle.SelectionBackColor = Color.FromArgb(110, 88, 25);
                grid.Rows[index].DefaultCellStyle.ForeColor = Color.Gold;
            }
        }
        shell.Controls.Add(grid);

        Button replay = CreateButton("PLAY AGAIN", 190);
        replay.Location = new Point(190, 506);
        replay.Click += (_, _) =>
        {
            PlayAgain = true;
            Close();
        };
        shell.Controls.Add(replay);

        Button back = CreateButton("BACK TO MENU", 190);
        back.Location = new Point(484, 506);
        back.Click += (_, _) => Close();
        shell.Controls.Add(back);
    }

    private static Button CreateButton(string text, int width)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(width, 48),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(32, 42, 76),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(78, 128, 204);
        button.FlatAppearance.BorderSize = 2;
        return button;
    }
}
