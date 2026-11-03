using System.Drawing.Drawing2D;
using System.ComponentModel;
using FlappyBird.Core;
namespace FlappyBird;

/// <summary>Yalnızca çizer; fizik ve çarpışma hesabı GameSession'a aittir.</summary>
internal sealed class GameCanvas : Control
{
    private readonly GameSession game;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Paused { get; set; }
    public GameCanvas(GameSession game)
    {
        this.game = game;
        DoubleBuffered = true;
        ResizeRedraw = true;
        TabStop = true;
        BackColor = SystemColors.Window;
        SetStyle(ControlStyles.Selectable, true);
        MouseDown += (_, _) => Focus();
    }

    protected override bool IsInputKey(Keys keyData) =>
        (keyData & Keys.KeyCode) is Keys.Up or Keys.Space || base.IsInputKey(keyData);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(SystemColors.Window);
        float scale = Math.Min(ClientSize.Width / GameSession.WorldWidth, ClientSize.Height / GameSession.WorldHeight);
        if (scale <= 0) return;
        var saved = g.Save();
        g.TranslateTransform((ClientSize.Width - GameSession.WorldWidth * scale) / 2, (ClientSize.Height - GameSession.WorldHeight * scale) / 2);
        g.ScaleTransform(scale, scale);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool highContrast = SystemInformation.HighContrast;
        using var pipeBrush = new SolidBrush(highContrast ? SystemColors.Highlight : Color.FromArgb(39, 121, 82));
        using var birdBrush = new SolidBrush(highContrast ? SystemColors.HighlightText : Color.FromArgb(227, 171, 32));
        using var outline = new Pen(SystemColors.WindowText, 2);
        foreach (var pipe in game.Pipes)
        {
            g.FillRectangle(pipeBrush, pipe.X, 0, PipePair.Width, pipe.GapTop);
            float bottom = pipe.GapTop + PipePair.GapHeight;
            g.FillRectangle(pipeBrush, pipe.X, bottom, PipePair.Width, GameSession.GroundY - bottom);
        }
        g.FillRectangle(SystemBrushes.ControlDark, 0, GameSession.GroundY, GameSession.WorldWidth, 40);
        g.FillEllipse(birdBrush, game.Bird.X, game.Bird.Y, Bird.Size, Bird.Size);
        g.DrawEllipse(outline, game.Bird.X, game.Bird.Y, Bird.Size, Bird.Size);
        g.DrawLine(outline, game.Bird.X + 16, game.Bird.Y + 8, game.Bird.X + 20, game.Bird.Y + 8);
        if (game.State != GameState.Running || Paused)
        {
            var panel = new RectangleF(180, 155, 480, 130);
            g.FillRectangle(SystemBrushes.Window, panel);
            g.DrawRectangle(outline, panel.X, panel.Y, panel.Width, panel.Height);
            using var heading = new Font("Segoe UI", 22, FontStyle.Bold);
            using var detail = new Font("Segoe UI", 12);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            string title = Paused ? "Duraklatıldı" : game.State == GameState.GameOver ? "Oyun bitti" : "Flappy Bird";
            string hint = Paused ? "Devam et veya P" : game.State == GameState.GameOver ? $"Skor: {game.Score} • Yeni Oyun veya R" : "Yeni Oyun veya Boşluk ile başlayın";
            g.DrawString(title, heading, SystemBrushes.WindowText, new RectangleF(190, 175, 460, 45), format);
            g.DrawString(hint, detail, SystemBrushes.WindowText, new RectangleF(190, 225, 460, 35), format);
        }
        g.Restore(saved);
        ControlPaint.DrawBorder(g, ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
        if (Focused) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -3, -3));
    }
}
