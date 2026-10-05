namespace TetrisWinForms.Forms;

public sealed class PlayerNameDialog : Form
{
    private readonly TextBox _nameBox = new();
    public string PlayerName => string.IsNullOrWhiteSpace(_nameBox.Text) ? "Player" : _nameBox.Text.Trim();

    public PlayerNameDialog(string defaultName = "Player")
    {
        Text = "Player Name";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(390, 190);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(18, 22, 38);

        var label = new Label
        {
            Text = "Enter player name before starting:",
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(28, 25)
        };
        Controls.Add(label);

        _nameBox.Text = defaultName;
        _nameBox.Location = new Point(28, 58);
        _nameBox.Width = 330;
        _nameBox.MaxLength = 24;
        _nameBox.SelectAll();
        Controls.Add(_nameBox);

        var cancel = new Button
        {
            Text = "CANCEL",
            DialogResult = DialogResult.Cancel,
            Location = new Point(98, 112),
            Size = new Size(110, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(42, 46, 62),
            ForeColor = Color.White,
            TabStop = false
        };
        Controls.Add(cancel);

        var ok = new Button
        {
            Text = "START",
            DialogResult = DialogResult.OK,
            Location = new Point(220, 112),
            Size = new Size(138, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(42, 74, 120),
            ForeColor = Color.White,
            TabStop = false
        };
        Controls.Add(ok);

        AcceptButton = ok;
        CancelButton = cancel;
    }
}
