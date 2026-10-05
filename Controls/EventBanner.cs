using System.Drawing.Drawing2D;

namespace TetrisWinForms.Controls;

public sealed class EventBanner : Control
{
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30 };
    private DateTime _startedAt;
    private int _durationMs = 900;
    private Color _accent = Color.DeepSkyBlue;
    private string _title = string.Empty;
    private string _subtitle = string.Empty;

    public EventBanner()
    {
        DoubleBuffered = true;
        Visible = false;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;

        _timer.Tick += (_, _) =>
        {
            if ((DateTime.Now - _startedAt).TotalMilliseconds >= _durationMs)
            {
                _timer.Stop();
                Visible = false;
            }
            else
            {
                Invalidate();
            }
        };
    }

    public void ShowEvent(string title, string subtitle, Color accent, int durationMs = 900)
    {
        _title = title;
        _subtitle = subtitle;
        _accent = accent;
        _durationMs = Math.Max(250, durationMs);
        _startedAt = DateTime.Now;
        Visible = true;
        BringToFront();
        _timer.Stop();
        _timer.Start();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!Visible) return;

        double elapsed = (DateTime.Now - _startedAt).TotalMilliseconds;
        double p = Math.Clamp(elapsed / _durationMs, 0d, 1d);
        int alpha = p < 0.72 ? 235 : (int)(235 * (1d - (p - 0.72) / 0.28));
        alpha = Math.Clamp(alpha, 0, 235);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle rect = new(2, 2, Width - 5, Height - 5);
        using var path = RoundedRect(rect, 18);
        using var bg = new SolidBrush(Color.FromArgb(Math.Min(alpha, 215), 7, 10, 24));
        using var border = new Pen(Color.FromArgb(alpha, _accent), 2.5f);
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(border, path);

        using var titleFont = new Font("Segoe UI", 18, FontStyle.Bold);
        using var subFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.FromArgb(alpha, Color.White));
        using var subBrush = new SolidBrush(Color.FromArgb(Math.Min(alpha, 200), _accent));

        SizeF titleSize = e.Graphics.MeasureString(_title, titleFont);
        e.Graphics.DrawString(_title, titleFont, titleBrush, (Width - titleSize.Width) / 2f, 12);
        SizeF subSize = e.Graphics.MeasureString(_subtitle, subFont);
        e.Graphics.DrawString(_subtitle, subFont, subBrush, (Width - subSize.Width) / 2f, 50);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
