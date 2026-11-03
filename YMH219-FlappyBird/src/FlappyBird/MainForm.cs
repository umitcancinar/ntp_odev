using System.Diagnostics;
using FlappyBird.Core;
namespace FlappyBird;

public sealed class MainForm : Form
{
    private readonly GameSession game = new();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Stopwatch clock = new();
    private readonly Label score = new() { AutoSize = true, Text = "Skor: 0", Margin = new Padding(16, 9, 0, 0) };
    private readonly Label status = new() { AutoSize = true, Text = "Hazır — Yeni Oyun veya Boşluk ile başlayın.", Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 6) };
    private readonly Button pause = new() { Text = "&Duraklat", AutoSize = true, Enabled = false };
    private readonly GameCanvas canvas;
    private readonly HashSet<Keys> heldKeys = [];
    private bool paused;

    public MainForm()
    {
        Text = "YMH219 • Flappy Bird";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(900, 640);
        MinimumSize = new Size(660, 520);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        canvas = new GameCanvas(game) { Dock = DockStyle.Fill, TabIndex = 1, AccessibleName = "Oyun alanı", AccessibleDescription = "Zıplamak için Boşluk veya Yukarı ok. Yeni oyun için R." };
        var reset = new Button { Text = "&Yeni Oyun", AutoSize = true, TabIndex = 0 };
        reset.Click += (_, _) => NewGame();
        pause.Click += (_, _) => TogglePause();
        var toolbar = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
        toolbar.Controls.AddRange([reset, pause, score]);
        var instructions = new Label { AutoSize = true, Dock = DockStyle.Fill, Text = "Boşluk / ↑: zıpla   •   R: yeniden başlat   •   P: duraklat / devam et\nHer boru çifti 1 puan. Tuşu basılı tutmak tekrar zıplatmaz.", Padding = new Padding(0, 6, 0, 8) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(instructions, 0, 1);
        layout.Controls.Add(canvas, 0, 2);
        layout.Controls.Add(status, 0, 3);
        Controls.Add(layout);
        timer.Tick += (_, _) => TickGame();
        KeyDown += OnGameKeyDown;
        KeyPress += (_, e) => { if (e.KeyChar is ' ' or 'r' or 'R' or 'p' or 'P') e.Handled = true; };
        KeyUp += (_, e) => { heldKeys.Remove(e.KeyCode); if (!heldKeys.Contains(Keys.Space) && !heldKeys.Contains(Keys.Up)) game.ReleaseFlap(); };
        Deactivate += (_, _) => { heldKeys.Clear(); game.ReleaseFlap(); if (game.State == GameState.Running && !paused) TogglePause(); };
        Shown += (_, _) => canvas.Focus();
    }

    private void NewGame()
    {
        game.Start();
        heldKeys.Clear();
        paused = false;
        canvas.Paused = false;
        pause.Enabled = true;
        pause.Text = "&Duraklat";
        score.Text = "Skor: 0";
        status.Text = "Oynanıyor — Boşluk / ↑ ile zıplayın.";
        clock.Restart();
        timer.Start();
        canvas.Focus();
        canvas.Invalidate();
    }

    private void TogglePause()
    {
        if (game.State != GameState.Running) return;
        paused = !paused;
        canvas.Paused = paused;
        if (paused) timer.Stop(); else { clock.Restart(); timer.Start(); }
        pause.Text = paused ? "&Devam et" : "&Duraklat";
        status.Text = paused ? "Duraklatıldı — Devam et düğmesi veya P ile sürdürün." : "Oynanıyor — Boşluk / ↑ ile zıplayın.";
        canvas.Invalidate();
        canvas.Focus();
    }

    private void OnGameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is not (Keys.Space or Keys.Up or Keys.R or Keys.P)) return;
        // KeyUp must reach the form so the next press can flap again.
        e.Handled = true;
        bool firstPress = heldKeys.Add(e.KeyCode);
        if (!firstPress) return;
        if (e.KeyCode == Keys.R) { NewGame(); heldKeys.Add(Keys.R); return; }
        if (e.KeyCode == Keys.P) { TogglePause(); return; }
        if (game.State == GameState.Ready) { NewGame(); heldKeys.Add(e.KeyCode); }
        if (!paused) game.PressFlap();
    }

    private void TickGame()
    {
        float elapsed = (float)clock.Elapsed.TotalSeconds;
        clock.Restart();
        game.Advance(elapsed);
        score.Text = $"Skor: {game.Score}";
        if (game.State == GameState.GameOver)
        {
            timer.Stop();
            pause.Enabled = false;
            status.Text = $"Oyun bitti — Skor: {game.Score}. Yeni Oyun veya R ile tekrar deneyin.";
        }
        canvas.Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { timer.Dispose(); clock.Stop(); }
        base.Dispose(disposing);
    }
}
