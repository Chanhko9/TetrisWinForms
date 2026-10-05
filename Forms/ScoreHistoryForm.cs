using TetrisWinForms;
using System.Diagnostics;
using TetrisWinForms.Core;
using TetrisWinForms.Managers;
using TetrisWinForms.Models;

namespace TetrisWinForms.Forms;

public sealed class ScoreHistoryForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly ComboBox _modeFilter = new();

    public ScoreHistoryForm()
    {
        Text = "Score History";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(920, 640);
        BackColor = Color.FromArgb(10, 13, 27);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        BuildUi();
        LoadRows();
    }

    private void BuildUi()
    {
        Controls.Add(new Label
        {
            Text = "SCORE HISTORY",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(45, 30)
        });

        _modeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _modeFilter.Location = new Point(640, 52);
        _modeFilter.Width = 220;
        _modeFilter.Items.Add("ALL MODES");
        foreach (GameModeProfile profile in GameModeProfile.All.Values)
            _modeFilter.Items.Add(profile.DisplayName);
        _modeFilter.SelectedIndex = 0;
        _modeFilter.SelectedIndexChanged += (_, _) => LoadRows();
        Controls.Add(_modeFilter);

        _grid.Location = new Point(45, 105);
        _grid.Size = new Size(820, 430);
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.FromArgb(17, 21, 38);
        _grid.BorderStyle = BorderStyle.None;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.DefaultCellStyle.BackColor = Color.FromArgb(23, 28, 49);
        _grid.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(44, 73, 119);
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 39, 68);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _grid.EnableHeadersVisualStyles = false;

        _grid.Columns.Add("Rank", "#");
        _grid.Columns.Add("Player", "PLAYER");
        _grid.Columns.Add("Score", "SCORE");
        _grid.Columns.Add("Mode", "MODE");
        _grid.Columns.Add("Time", "PLAYED AT");
        _grid.Columns[0].FillWeight = 30;
        _grid.Columns[1].FillWeight = 120;
        _grid.Columns[2].FillWeight = 80;
        _grid.Columns[3].FillWeight = 120;
        _grid.Columns[4].FillWeight = 125;
        Controls.Add(_grid);

        var openTxt = new Button
        {
            Text = "OPEN TXT FOLDER",
            Location = new Point(45, 555),
            Size = new Size(190, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(33, 44, 76),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TabStop = false
        };
        openTxt.FlatAppearance.BorderColor = Color.FromArgb(75, 125, 200);
        openTxt.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppPaths.DataDirectory,
                    UseShellExecute = true
                });
            }
            catch { }
        };
        Controls.Add(openTxt);

        var close = new Button
        {
            Text = "CLOSE",
            Location = new Point(710, 555),
            Size = new Size(155, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(33, 44, 76),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            TabStop = false
        };
        close.FlatAppearance.BorderColor = Color.FromArgb(75, 125, 200);
        close.Click += (_, _) => Close();
        Controls.Add(close);
    }

    private void LoadRows()
    {
        List<ScoreRecord> rows = ScoreManager.LoadAll();

        if (_modeFilter.SelectedIndex > 0)
        {
            GameMode selectedMode = GameModeProfile.All.Values.ElementAt(_modeFilter.SelectedIndex - 1).Mode;
            rows = rows.Where(x => x.Mode == selectedMode).ToList();
        }

        rows = rows
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.PlayedAt)
            .Take(100)
            .ToList();

        _grid.Rows.Clear();
        for (int i = 0; i < rows.Count; i++)
        {
            ScoreRecord row = rows[i];
            _grid.Rows.Add(
                i + 1,
                row.PlayerName,
                row.Score.ToString("N0"),
                GameModeProfile.Get(row.Mode).DisplayName,
                row.PlayedAt.ToString("dd/MM/yyyy HH:mm"));
        }
    }
}
