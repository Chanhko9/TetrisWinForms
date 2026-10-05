namespace TetrisWinForms.Forms;

public sealed class DuelNameDialog : Form
{
    private readonly TextBox _player1 = new();
    private readonly TextBox _player2 = new();

    public string Player1Name => string.IsNullOrWhiteSpace(_player1.Text) ? "Player 1" : _player1.Text.Trim();
    public string Player2Name => string.IsNullOrWhiteSpace(_player2.Text) ? "Player 2" : _player2.Text.Trim();

    public DuelNameDialog()
    {
        Text = "Duel Arena";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 285);
        BackColor = Color.FromArgb(13, 17, 33);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(new Label
        {
            Text = "DUEL ARENA",
            ForeColor = Color.DeepSkyBlue,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(145, 22)
        });

        Controls.Add(MakeLabel("PLAYER 1", 40, 88));
        _player1.Text = "Player 1";
        _player1.Location = new Point(145, 84);
        _player1.Size = new Size(285, 30);
        Controls.Add(_player1);

        Controls.Add(MakeLabel("PLAYER 2", 40, 135));
        _player2.Text = "Player 2";
        _player2.Location = new Point(145, 131);
        _player2.Size = new Size(285, 30);
        Controls.Add(_player2);

        var cancel = MakeButton("CANCEL", 118, 205, 120);
        cancel.DialogResult = DialogResult.Cancel;
        Controls.Add(cancel);

        var start = MakeButton("START DUEL", 252, 205, 178);
        start.DialogResult = DialogResult.OK;
        start.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        Controls.Add(start);

        AcceptButton = start;
        CancelButton = cancel;
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text = text,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        AutoSize = true,
        Location = new Point(x, y)
    };

    private static Button MakeButton(string text, int x, int y, int width)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(31, 41, 71),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TabStop = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(90, 125, 190);
        return button;
    }
}
