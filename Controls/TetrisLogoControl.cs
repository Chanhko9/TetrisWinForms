using System.Drawing.Drawing2D;

namespace TetrisWinForms.Controls;

public sealed class TetrisLogoControl : Control
{
    public Color AccentColor { get; set; } = Color.DeepSkyBlue;

    public TetrisLogoControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        float fontSize = Math.Max(42f, Math.Min(78f, Height * 0.52f));
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        using var shadow = new SolidBrush(Color.FromArgb(110, Color.Black));
        using var glow = new SolidBrush(Color.FromArgb(110, AccentColor));
        using var main = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        RectangleF rect = ClientRectangle;
        RectangleF shadowRect = new(rect.X + 5, rect.Y + 7, rect.Width, rect.Height);
        RectangleF glowRect = new(rect.X, rect.Y + 3, rect.Width, rect.Height);

        e.Graphics.DrawString("TETRIS", font, shadow, shadowRect, format);
        e.Graphics.DrawString("TETRIS", font, glow, glowRect, format);
        e.Graphics.DrawString("TETRIS", font, main, rect, format);

        int block = Math.Max(10, Height / 12);
        DrawBlock(e.Graphics, new Rectangle(18, Height / 2 - block * 2, block, block), Color.Cyan);
        DrawBlock(e.Graphics, new Rectangle(18 + block + 4, Height / 2 - block, block, block), Color.MediumPurple);
        DrawBlock(e.Graphics, new Rectangle(Width - 18 - block, Height / 2 - block * 2, block, block), Color.Gold);
        DrawBlock(e.Graphics, new Rectangle(Width - 18 - block * 2 - 4, Height / 2 - block, block, block), Color.LimeGreen);
    }

    private static void DrawBlock(Graphics g, Rectangle rect, Color color)
    {
        using var brush = new SolidBrush(Color.FromArgb(220, color));
        using var pen = new Pen(Color.FromArgb(230, Color.White), 1.5f);
        g.FillRectangle(brush, rect);
        g.DrawRectangle(pen, rect);
    }
}
