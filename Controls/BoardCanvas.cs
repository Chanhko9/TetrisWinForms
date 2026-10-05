using System.Drawing.Drawing2D;
using TetrisWinForms.Core;
using TetrisWinForms.Game;

namespace TetrisWinForms.Controls;

public sealed class BoardCanvas : Control
{
    private sealed class Particle
    {
        public float Row;
        public float Col;
        public float VRow;
        public float VCol;
        public float Life;
        public float Size;
        public Color Color;
    }

    public BoardModel? Board { get; set; }
    public ActivePiece? ActivePiece { get; set; }
    public ActivePiece? GhostPiece { get; set; }
    public GravityDirection Gravity { get; set; } = GravityDirection.Down;
    public GameMode Mode { get; set; } = GameMode.Basic;
    public bool FogEnabled { get; set; }
    public bool FogRevealed { get; set; }
    public int FogRows { get; set; } = 5;
    public int ModeEventSeconds { get; set; } = int.MaxValue;
    public bool ShowOverlay { get; set; }
    public string OverlayTitle { get; set; } = string.Empty;
    public string OverlaySubtitle { get; set; } = string.Empty;

    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();
    private readonly System.Windows.Forms.Timer _effectTimer = new() { Interval = 16 };
    private Color _flashColor = Color.Transparent;
    private int _flashAlpha;
    private float _shockwaveRow = -1;
    private float _shockwaveCol = -1;
    private float _shockwaveRadius;
    private float _shockwaveLife;
    private float _lineClearLife;
    private int _lineClearCount;
    private GravityDirection _lineClearGravity = GravityDirection.Down;
    private float _impactLife;
    private GravityDirection _impactGravity = GravityDirection.Down;
    private Color _impactColor = Color.White;

    private static readonly Color[] CellColors =
    {
        Color.Transparent,
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
        Color.SpringGreen,
        Color.DeepSkyBlue
    };

    public BoardCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(12, 15, 28);
        SetStyle(ControlStyles.ResizeRedraw, true);

        _effectTimer.Tick += (_, _) =>
        {
            bool active = false;

            if (_flashAlpha > 0)
            {
                _flashAlpha = Math.Max(0, _flashAlpha - 12);
                active = true;
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                Particle p = _particles[i];
                p.Row += p.VRow;
                p.Col += p.VCol;
                p.VRow += 0.012f;
                p.Life -= 0.045f;
                if (p.Life <= 0) _particles.RemoveAt(i);
                else active = true;
            }

            if (_shockwaveLife > 0)
            {
                _shockwaveLife -= 0.05f;
                _shockwaveRadius += 0.24f;
                active = true;
            }

            if (_lineClearLife > 0)
            {
                _lineClearLife -= 0.07f;
                active = true;
            }

            if (_impactLife > 0)
            {
                _impactLife -= 0.10f;
                active = true;
            }

            Invalidate();
            if (!active) _effectTimer.Stop();
        };
    }

    public void TriggerBurst(int row, int col, Color color, int count = 28, int flashAlpha = 125)
    {
        _flashColor = color;
        _flashAlpha = Math.Clamp(flashAlpha, 0, 210);
        _shockwaveRow = row + 0.5f;
        _shockwaveCol = col + 0.5f;
        _shockwaveRadius = 0.2f;
        _shockwaveLife = 1f;

        for (int i = 0; i < count; i++)
        {
            double a = _random.NextDouble() * Math.PI * 2;
            float speed = 0.08f + (float)_random.NextDouble() * 0.18f;
            _particles.Add(new Particle
            {
                Row = row + 0.5f,
                Col = col + 0.5f,
                VRow = (float)Math.Sin(a) * speed,
                VCol = (float)Math.Cos(a) * speed,
                Life = 0.55f + (float)_random.NextDouble() * 0.7f,
                Size = 0.10f + (float)_random.NextDouble() * 0.22f,
                Color = color
            });
        }

        _effectTimer.Start();
        Invalidate();
    }

    public void TriggerBoardPulse(Color color, int flashAlpha = 105)
    {
        _flashColor = color;
        _flashAlpha = Math.Clamp(flashAlpha, 0, 200);
        _effectTimer.Start();
        Invalidate();
    }

    public void TriggerLineClear(int count, GravityDirection gravity, Color color)
    {
        _lineClearCount = Math.Clamp(count, 1, 4);
        _lineClearGravity = gravity;
        _lineClearLife = 1f;
        _flashColor = color;
        _effectTimer.Start();
        Invalidate();
    }

    public void TriggerImpact(GravityDirection gravity, Color color)
    {
        _impactGravity = gravity;
        _impactColor = color;
        _impactLife = 1f;
        _flashColor = color;
        _flashAlpha = Math.Max(_flashAlpha, 28);
        _effectTimer.Start();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Board is null) return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        float cellW = ClientSize.Width / (float)BoardModel.Columns;
        float cellH = ClientSize.Height / (float)BoardModel.Rows;

        DrawModeBackdrop(e.Graphics);

        using var gridPen = new Pen(Color.FromArgb(38, 255, 255, 255), 1);
        for (int row = 0; row < BoardModel.Rows; row++)
        for (int col = 0; col < BoardModel.Columns; col++)
        {
            RectangleF rect = CellRect(row, col, cellW, cellH);
            int value = Board.Cells[row, col];

            if (value > 0)
                DrawFilledCell(e.Graphics, rect, value, 255);

            e.Graphics.DrawRectangle(gridPen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        if (GhostPiece is not null)
        {
            foreach ((int row, int col) in GhostPiece.Cells())
            {
                if (!Board.IsInside(row, col)) continue;
                RectangleF rect = CellRect(row, col, cellW, cellH);
                using var ghostPen = new Pen(Color.FromArgb(105, Color.White), 2);
                e.Graphics.DrawRectangle(ghostPen, rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
            }
        }

        if (ActivePiece is not null)
        {
            foreach ((int row, int col) in ActivePiece.Cells())
            {
                if (!Board.IsInside(row, col)) continue;
                DrawFilledCell(e.Graphics, CellRect(row, col, cellW, cellH), ActivePiece.ColorValue, 255);
            }
        }

        if (FogEnabled && !FogRevealed)
            DrawFog(e.Graphics, cellH);

        DrawParticles(e.Graphics, cellW, cellH);
        DrawShockwave(e.Graphics, cellW, cellH);
        DrawLineClearSweep(e.Graphics);
        DrawImpact(e.Graphics);

        if (_flashAlpha > 0)
        {
            using var flash = new SolidBrush(Color.FromArgb(_flashAlpha, _flashColor));
            e.Graphics.FillRectangle(flash, ClientRectangle);
        }

        DrawModeWarning(e.Graphics);
        DrawAccentFrame(e.Graphics);
        DrawOverlay(e.Graphics);
    }


    private void DrawImpact(Graphics g)
    {
        if (_impactLife <= 0) return;

        int alpha = Math.Clamp((int)(220 * _impactLife), 0, 220);
        float thickness = 3f + 9f * _impactLife;
        using var pen = new Pen(Color.FromArgb(alpha, _impactColor), thickness);

        switch (_impactGravity)
        {
            case GravityDirection.Down:
                g.DrawLine(pen, 8, Height - 6, Width - 8, Height - 6);
                break;
            case GravityDirection.Up:
                g.DrawLine(pen, 8, 6, Width - 8, 6);
                break;
            case GravityDirection.Right:
                g.DrawLine(pen, Width - 6, 8, Width - 6, Height - 8);
                break;
            case GravityDirection.Left:
                g.DrawLine(pen, 6, 8, 6, Height - 8);
                break;
        }
    }

    private void DrawLineClearSweep(Graphics g)
    {
        if (_lineClearLife <= 0 || _lineClearCount <= 0) return;

        float progress = 1f - Math.Clamp(_lineClearLife, 0f, 1f);
        int alpha = Math.Clamp((int)(210 * _lineClearLife), 0, 210);
        Color accent = ModeVisualTheme.Get(Mode).Accent;
        using var glow = new SolidBrush(Color.FromArgb(alpha, accent));
        using var core = new SolidBrush(Color.FromArgb(Math.Min(245, alpha + 35), Color.White));

        if (_lineClearGravity is GravityDirection.Down or GravityDirection.Up)
        {
            float y = progress * Height;
            float h = 8 + _lineClearCount * 3;
            g.FillRectangle(glow, 0, y - h * 2, Width, h * 4);
            g.FillRectangle(core, 0, y - h / 2, Width, h);
        }
        else
        {
            float x = progress * Width;
            float w = 8 + _lineClearCount * 3;
            g.FillRectangle(glow, x - w * 2, 0, w * 4, Height);
            g.FillRectangle(core, x - w / 2, 0, w, Height);
        }
    }

    private void DrawOverlay(Graphics g)
    {
        if (!ShowOverlay || string.IsNullOrWhiteSpace(OverlayTitle)) return;

        using var dim = new SolidBrush(Color.FromArgb(185, 4, 7, 16));
        g.FillRectangle(dim, ClientRectangle);

        Color accent = ModeVisualTheme.Get(Mode).Accent;
        using var titleFont = new Font("Segoe UI", 28, FontStyle.Bold);
        using var subFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.White);
        using var subBrush = new SolidBrush(accent);

        SizeF titleSize = g.MeasureString(OverlayTitle, titleFont);
        float titleY = Height / 2f - 48;
        g.DrawString(OverlayTitle, titleFont, titleBrush, (Width - titleSize.Width) / 2f, titleY);

        if (!string.IsNullOrWhiteSpace(OverlaySubtitle))
        {
            SizeF subSize = g.MeasureString(OverlaySubtitle, subFont);
            g.DrawString(OverlaySubtitle, subFont, subBrush, (Width - subSize.Width) / 2f, titleY + 54);
        }
    }

    private void DrawModeBackdrop(Graphics g)
    {
        Color tint = ModeVisualTheme.Get(Mode).Accent;
        using var brush = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(20, tint),
            Color.FromArgb(6, tint),
            LinearGradientMode.ForwardDiagonal);
        g.FillRectangle(brush, ClientRectangle);

        if (Mode == GameMode.ZeroGRift)
        {
            using var star = new SolidBrush(Color.FromArgb(95, Color.White));
            for (int i = 0; i < 25; i++)
            {
                int x = (i * 97 + 31) % Math.Max(1, Width);
                int y = (i * 53 + 17) % Math.Max(1, Height);
                g.FillEllipse(star, x, y, 2, 2);
            }
        }
        else if (Mode == GameMode.ArcaneChaos)
        {
            using var pen = new Pen(Color.FromArgb(35, tint), 2);
            float cx = Width / 2f;
            float cy = Height / 2f;
            for (int i = 1; i <= 3; i++)
                g.DrawEllipse(pen, cx - i * 44, cy - i * 44, i * 88, i * 88);
        }
    }

    private static RectangleF CellRect(int row, int col, float cellW, float cellH)
        => new(col * cellW, row * cellH, cellW, cellH);

    private static void DrawFilledCell(Graphics g, RectangleF rect, int colorValue, int alpha)
    {
        Color baseColor = CellColors[Math.Clamp(colorValue, 1, CellColors.Length - 1)];
        using var brush = new LinearGradientBrush(rect,
            Color.FromArgb(alpha, ControlPaint.Light(baseColor, 0.25f)),
            Color.FromArgb(alpha, ControlPaint.Dark(baseColor, 0.18f)),
            LinearGradientMode.ForwardDiagonal);
        g.FillRectangle(brush, rect);

        using var hi = new Pen(Color.FromArgb(Math.Min(alpha, 170), Color.White), 2);
        g.DrawRectangle(hi, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);

        using var shade = new Pen(Color.FromArgb(Math.Min(alpha, 130), Color.Black), 2);
        g.DrawLine(shade, rect.Left + 2, rect.Bottom - 3, rect.Right - 3, rect.Bottom - 3);
        g.DrawLine(shade, rect.Right - 3, rect.Top + 2, rect.Right - 3, rect.Bottom - 3);
    }

    private void DrawFog(Graphics g, float cellH)
    {
        int rows = Math.Clamp(FogRows, 2, BoardModel.Rows - 2);
        float top = ClientSize.Height - rows * cellH;
        RectangleF fogRect = new(0, top, ClientSize.Width, ClientSize.Height - top);

        // The fog intentionally hides the locked blocks almost completely.
        using var fog = new LinearGradientBrush(
            fogRect,
            Color.FromArgb(242, 18, 25, 34),
            Color.FromArgb(255, 5, 8, 13),
            LinearGradientMode.Vertical);
        g.FillRectangle(fog, fogRect);

        int drift = Environment.TickCount / 110;
        using var mist1 = new SolidBrush(Color.FromArgb(72, 205, 220, 230));
        using var mist2 = new SolidBrush(Color.FromArgb(58, 120, 155, 175));
        for (int i = 0; i < 11; i++)
        {
            float x = ((i * 79 + drift * (i % 2 == 0 ? 1 : -1)) % (ClientSize.Width + 220)) - 110;
            float y = top + 14 + (i % 5) * 43;
            g.FillEllipse(i % 2 == 0 ? mist1 : mist2, x - 80, y, 220, 72);
        }

        using var cover = new SolidBrush(Color.FromArgb(105, 5, 9, 15));
        g.FillRectangle(cover, fogRect);

        using var font = new Font("Segoe UI", 12, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(180, Color.White));
        const string text = "PHANTOM FOG";
        SizeF size = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush, (ClientSize.Width - size.Width) / 2f, top + 15);
    }

    private void DrawParticles(Graphics g, float cellW, float cellH)
    {
        foreach (Particle p in _particles)
        {
            int alpha = Math.Clamp((int)(255 * Math.Min(1f, p.Life)), 0, 255);
            using var b = new SolidBrush(Color.FromArgb(alpha, p.Color));
            float size = Math.Max(3f, p.Size * Math.Min(cellW, cellH));
            float x = p.Col * cellW - size / 2f;
            float y = p.Row * cellH - size / 2f;
            g.FillEllipse(b, x, y, size, size);
        }
    }

    private void DrawShockwave(Graphics g, float cellW, float cellH)
    {
        if (_shockwaveLife <= 0 || _shockwaveRow < 0) return;
        int alpha = Math.Clamp((int)(220 * _shockwaveLife), 0, 220);
        using var pen = new Pen(Color.FromArgb(alpha, _flashColor), 3);
        float cx = _shockwaveCol * cellW;
        float cy = _shockwaveRow * cellH;
        float radius = _shockwaveRadius * Math.Min(cellW, cellH);
        g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);
    }

    private void DrawGravityIndicator(Graphics g)
    {
        // Hidden to keep the board clean.
    }

    private void DrawModeWarning(Graphics g)
    {
        if (ModeEventSeconds is < 1 or > 3) return;
        if (Mode is not (GameMode.ZeroGRift or GameMode.CursedTide)) return;

        Color danger = ModeVisualTheme.Get(Mode).Danger;
        using var pen = new Pen(Color.FromArgb(110 + (3 - ModeEventSeconds) * 35, danger), 4);
        g.DrawRectangle(pen, 3, 3, Width - 7, Height - 7);
    }

    private void DrawAccentFrame(Graphics g)
    {
        Color accent = ModeVisualTheme.Get(Mode).Accent;
        using var pen = new Pen(Color.FromArgb(160, accent), 2);
        g.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
    }
}
