using TetrisWinForms.Game;

namespace TetrisWinForms.Controls;

public sealed class TetrominoPreview : Control
{
    public TetrominoType? PieceType { get; set; }
    public string EmptyText { get; set; } = "EMPTY";

    private static readonly Color[] Colors =
    {
        Color.Cyan,
        Color.Gold,
        Color.MediumPurple,
        Color.LimeGreen,
        Color.IndianRed,
        Color.RoyalBlue,
        Color.DarkOrange,
        Color.HotPink,
        Color.MediumTurquoise,
        Color.OrangeRed,
        Color.MediumSlateBlue,
        Color.SpringGreen
    };

    public TetrominoPreview()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(16, 20, 36);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (PieceType is null)
        {
            using var font = new Font("Segoe UI", 12, FontStyle.Bold);
            using var emptyBrush = new SolidBrush(Color.FromArgb(120, 150, 170));
            SizeF size = e.Graphics.MeasureString(EmptyText, font);
            e.Graphics.DrawString(EmptyText, font, emptyBrush,
                (ClientSize.Width - size.Width) / 2f,
                (ClientSize.Height - size.Height) / 2f);
            return;
        }

        const float sizeCell = 30f;
        TetrominoType type = PieceType.Value;
        IReadOnlyList<BlockOffset> cells = TetrominoShapes.Get(type, 0);
        int minRow = cells.Min(x => x.Row);
        int maxRow = cells.Max(x => x.Row);
        int minCol = cells.Min(x => x.Col);
        int maxCol = cells.Max(x => x.Col);

        float width = (maxCol - minCol + 1) * sizeCell;
        float height = (maxRow - minRow + 1) * sizeCell;
        float ox = (ClientSize.Width - width) / 2f - minCol * sizeCell;
        float oy = (ClientSize.Height - height) / 2f - minRow * sizeCell;

        Color color = Colors[(int)type];
        using var brush = new SolidBrush(color);
        using var pen = new Pen(Color.FromArgb(160, Color.White), 2);

        foreach (BlockOffset cell in cells)
        {
            RectangleF rect = new(ox + cell.Col * sizeCell, oy + cell.Row * sizeCell, sizeCell, sizeCell);
            e.Graphics.FillRectangle(brush, rect);
            e.Graphics.DrawRectangle(pen, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
        }
    }
}
