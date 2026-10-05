using TetrisWinForms.Controls;
using TetrisWinForms.Managers;

namespace TetrisWinForms.Forms;

public sealed partial class DuelArenaForm
{
    private readonly BoardCanvas _board1 = new();
    private readonly BoardCanvas _board2 = new();
    private readonly TetrominoPreview _next1 = new();
    private readonly TetrominoPreview _hold1 = new();
    private readonly TetrominoPreview _next2 = new();
    private readonly TetrominoPreview _hold2 = new();
    private readonly Label _titleLabel = new();
    private readonly Label _timerLabel = new();
    private readonly Label _player1Label = new();
    private readonly Label _player2Label = new();
    private readonly Label _score1 = new();
    private readonly Label _score2 = new();
    private readonly Button _backButton = new();
    private readonly Panel _leftHud = new();
    private readonly Panel _rightHud = new();

    private void InitializeComponent()
    {
        Text = "TETRIS - DUEL ARENA";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1280, 820);
        BackColor = Color.FromArgb(8, 11, 24);
        BackgroundImage = ImageManager.Get("bg_basic.png");
        BackgroundImageLayout = ImageLayout.Stretch;
        KeyPreview = true;

        _timerLabel.Text = "05:00";
        _timerLabel.ForeColor = Color.Gold;
        _timerLabel.Font = new Font("Segoe UI", 34, FontStyle.Bold);
        _timerLabel.AutoSize = false;
        _timerLabel.TextAlign = ContentAlignment.MiddleCenter;
        _timerLabel.BackColor = Color.Transparent;
        Controls.Add(_timerLabel);

        ConfigureBoard(_board1, Color.DeepSkyBlue);
        ConfigureBoard(_board2, Color.OrangeRed);
        Controls.Add(_board1);
        Controls.Add(_board2);

        ConfigureHud(_leftHud, Color.DeepSkyBlue);
        ConfigureHud(_rightHud, Color.OrangeRed);
        Controls.Add(_leftHud);
        Controls.Add(_rightHud);

        ConfigureNameLabel(_player1Label, Color.DeepSkyBlue);
        ConfigureNameLabel(_player2Label, Color.OrangeRed);
        _leftHud.Controls.Add(_player1Label);
        _rightHud.Controls.Add(_player2Label);

        ConfigureScoreLabel(_score1);
        ConfigureScoreLabel(_score2);
        _leftHud.Controls.Add(_score1);
        _rightHud.Controls.Add(_score2);

        AddCompactPreview(_leftHud, _next1, "NEXT", 174, 8);
        AddCompactPreview(_leftHud, _hold1, "HOLD [C]", 316, 8);
        AddCompactPreview(_rightHud, _next2, "NEXT", 174, 8);
        AddCompactPreview(_rightHud, _hold2, "HOLD [M]", 316, 8);

        _backButton.Text = "BACK TO MODES";
        _backButton.Size = new Size(180, 44);
        _backButton.FlatStyle = FlatStyle.Flat;
        _backButton.BackColor = Color.FromArgb(28, 38, 67);
        _backButton.ForeColor = Color.White;
        _backButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _backButton.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        _backButton.TabStop = false;
        _backButton.Click += (_, _) => Close();
        Controls.Add(_backButton);

        LayoutArena();
    }

    private static void ConfigureBoard(BoardCanvas board, Color accent)
    {
        board.BackColor = Color.FromArgb(10, 14, 27);
        board.TabStop = false;
        board.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(210, accent), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, board.Width - 1, board.Height - 1);
        };
    }

    private static void ConfigureHud(Panel panel, Color accent)
    {
        panel.BackColor = Color.FromArgb(220, 10, 14, 30);
        panel.TabStop = false;
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(175, accent), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
    }

    private static void ConfigureNameLabel(Label label, Color accent)
    {
        label.ForeColor = accent;
        label.Font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Location = new Point(14, 10);
        label.Size = new Size(120, 28);
        label.BackColor = Color.Transparent;
    }

    private static void ConfigureScoreLabel(Label label)
    {
        label.ForeColor = Color.Gold;
        label.Font = new Font("Segoe UI", 31, FontStyle.Bold);
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Location = new Point(14, 38);
        label.Size = new Size(120, 58);
        label.Text = "0";
        label.BackColor = Color.Transparent;
    }

    private static void AddCompactPreview(Panel panel, TetrominoPreview preview, string title, int x, int y)
    {
        var titleLabel = new Label
        {
            Text = title,
            ForeColor = Color.Silver,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(104, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(x, y),
            BackColor = Color.Transparent
        };
        panel.Controls.Add(titleLabel);

        preview.Tag = titleLabel;
        preview.Location = new Point(x, y + 20);
        preview.Size = new Size(104, 70);
        preview.TabStop = false;
        panel.Controls.Add(preview);
    }

    private void LayoutArena()
    {
        // Keep the timer in a clean header strip.
        _timerLabel.SetBounds((ClientSize.Width - 420) / 2, 14, 420, 78);

        const int headerBottom = 100;
        const int bottomSpace = 66;
        const int centerGap = 20;
        const int hudGap = 14;
        const int sideMargin = 14;

        int availableHeight = Math.Max(680, ClientSize.Height - headerBottom - bottomSpace);

        // Narrower side HUDs free more horizontal room for larger 10x20 boards.
        int hudWidth = 170;
        int horizontalForBoards = ClientSize.Width - sideMargin * 2 - hudWidth * 2 - hudGap * 2 - centerGap;
        int boardWidthByHorizontal = Math.Max(330, horizontalForBoards / 2);
        int boardHeightByHorizontal = boardWidthByHorizontal * 2;

        int boardHeight = Math.Min(Math.Min(availableHeight, 940), boardHeightByHorizontal);
        int boardWidth = boardHeight / 2;

        int totalWidth = hudWidth + hudGap + boardWidth + centerGap + boardWidth + hudGap + hudWidth;
        int startX = Math.Max(sideMargin, (ClientSize.Width - totalWidth) / 2);

        // Vertically center the complete play area, then bias it slightly downward.
        int freeVertical = Math.Max(0, availableHeight - boardHeight);
        int boardTop = headerBottom + freeVertical / 2 + 12;

        int hudHeight = Math.Min(500, boardHeight);
        int hudTop = boardTop + Math.Max(0, (boardHeight - hudHeight) / 2);

        int leftHudX = startX;
        int board1X = leftHudX + hudWidth + hudGap;
        int board2X = board1X + boardWidth + centerGap;
        int rightHudX = board2X + boardWidth + hudGap;

        _leftHud.SetBounds(leftHudX, hudTop, hudWidth, hudHeight);
        _board1.SetBounds(board1X, boardTop, boardWidth, boardHeight);
        _board2.SetBounds(board2X, boardTop, boardWidth, boardHeight);
        _rightHud.SetBounds(rightHudX, hudTop, hudWidth, hudHeight);

        LayoutSideHud(_leftHud, _player1Label, _score1, _next1, _hold1);
        LayoutSideHud(_rightHud, _player2Label, _score2, _next2, _hold2);

        _backButton.Size = new Size(190, 44);
        _backButton.Location = new Point(
            (ClientSize.Width - _backButton.Width) / 2,
            Math.Min(ClientSize.Height - 50, boardTop + boardHeight + 8));
    }

    private static void LayoutSideHud(
        Panel panel,
        Label playerLabel,
        Label scoreLabel,
        TetrominoPreview nextPreview,
        TetrominoPreview holdPreview)
    {
        int padding = 14;
        int width = panel.Width - padding * 2;

        playerLabel.Location = new Point(padding, 16);
        playerLabel.Size = new Size(width, 34);
        playerLabel.TextAlign = ContentAlignment.MiddleCenter;

        scoreLabel.Location = new Point(padding, 55);
        scoreLabel.Size = new Size(width, 72);
        scoreLabel.TextAlign = ContentAlignment.MiddleCenter;

        int previewWidth = width;
        int previewHeight = 120;

        RepositionPreview(nextPreview, padding, 145, previewWidth, previewHeight);
        RepositionPreview(holdPreview, padding, 300, previewWidth, previewHeight);
    }

    private static void RepositionPreview(TetrominoPreview preview, int x, int y, int width, int height)
    {
        if (preview.Tag is Label titleLabel)
        {
            titleLabel.Location = new Point(x, y);
            titleLabel.Size = new Size(width, 24);
            titleLabel.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        preview.Location = new Point(x, y + 26);
        preview.Size = new Size(width, height);
    }

}
