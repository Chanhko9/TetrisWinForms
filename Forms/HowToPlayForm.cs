namespace TetrisWinForms.Forms;

public sealed class HowToPlayForm : Form
{
    public HowToPlayForm()
    {
        Text = "TETRIS - How To Play";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1040, 790);
        BackColor = Color.FromArgb(9, 13, 27);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
    }

    private void BuildUi()
    {
        Controls.Add(new Label
        {
            Text = "HOW TO PLAY",
            ForeColor = Color.DeepSkyBlue,
            Font = new Font("Segoe UI", 30, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(42, 22)
        });

        Controls.Add(new Label
        {
            Text = "CONTROLS",
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(48, 96)
        });

        Controls.Add(new Label
        {
            Text =
                "A / D or ← / →   Move\n" +
                "W / ↑            Rotate\n" +
                "S / ↓            Soft drop\n" +
                "SPACE            Hard drop\n" +
                "C                Hold piece\n" +
                "P / ESC          Pause\n" +
                "F                Use Arcane relic",
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Consolas", 11f, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(390, 240),
            Location = new Point(50, 130)
        });

        Controls.Add(new Label
        {
            Text = "6 MODES",
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(475, 96)
        });

        string[] modeNames =
        [
            "BASIC",
            "ARCANE CHAOS",
            "PHANTOM FOG",
            "ZERO-G RIFT",
            "CURSED TIDE",
            "DUEL ARENA"
        ];

        string[] modeDescriptions =
        [
            "Classic Tetris with extra custom shapes.",
            "Earn relics and press F to activate them.",
            "Dense fog hides the lower board.",
            "Gravity changes direction during play.",
            "Blue water rows rise; fill the gap to clear them.",
            "Two players battle for 5 minutes; highest score wins."
        ];

        int y = 130;
        for (int i = 0; i < modeNames.Length; i++)
        {
            Controls.Add(new Label
            {
                Text = modeNames[i],
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(477, y)
            });

            Controls.Add(new Label
            {
                Text = modeDescriptions[i],
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10f),
                AutoSize = false,
                Size = new Size(465, 28),
                Location = new Point(477, y + 24)
            });

            y += 54;
        }

        var scoring = new Panel
        {
            Location = new Point(42, 485),
            Size = new Size(950, 170),
            BackColor = Color.FromArgb(18, 25, 45)
        };
        Controls.Add(scoring);

        scoring.Controls.Add(new Label
        {
            Text = "SCORING",
            ForeColor = Color.DeepSkyBlue,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(22, 14)
        });

        scoring.Controls.Add(new Label
        {
            Text =
                "BASE: +1 point for each cleared row.\n" +
                "COMBO: clear rows on consecutive piece locks; bonus = combo streak - 1.\n" +
                "B2B: clear 4 rows twice in a row without a 1-3 row clear between them: +2 bonus.\n" +
                "Move / Rotate / Drop / Relic use: no direct score.\n" +
                "Custom shapes: Plus, U, V, P and I5 appear less often than classic pieces.",
            ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(905, 112),
            Location = new Point(22, 42)
        });

        Controls.Add(new Label
        {
            Text = "SUNRISE  •  MSang  •  VMinh  •  PNam  •  MQuân",
            ForeColor = Color.FromArgb(150, 170, 195),
            Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(48, 715)
        });

        var close = new Button
        {
            Text = "GOT IT",
            Size = new Size(180, 46),
            Location = new Point(812, 695),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(27, 39, 67),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        close.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        close.FlatAppearance.BorderSize = 2;
        close.Click += (_, _) => Close();
        Controls.Add(close);
    }
}
