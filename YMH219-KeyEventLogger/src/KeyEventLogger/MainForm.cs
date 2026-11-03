using System.Security;
using KeyEventLogger.Core;
namespace KeyEventLogger;

public sealed class MainForm : Form
{
    private readonly LogSession session = new();
    private readonly Button start = new() { Text = "&Başlat", AutoSize = true };
    private readonly Button stop = new() { Text = "&Durdur", AutoSize = true, Enabled = false };
    private readonly Button save = new() { Text = "&Dosyaya Kaydet…", AutoSize = true, Enabled = false };
    private readonly Label state = new() { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
    private readonly Label notice = new() { AutoSize = true, Dock = DockStyle.Fill, Text = "Hazır — Başlat ile form içi olay kaydını açın.", Padding = new Padding(0, 6, 0, 0) };
    private readonly TextBox practice = new() { Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, AccessibleName = "Tuş deneme alanı", MaxLength = 5000 };
    private readonly RichTextBox output = new() { ReadOnly = true, Dock = DockStyle.Fill, WordWrap = false, DetectUrls = false, AccessibleName = "Zaman damgalı klavye olayları", BackColor = SystemColors.Window };

    public MainForm()
    {
        Text = "YMH219 • Form İçi Klavye Olayları";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(920, 680);
        MinimumSize = new Size(700, 560);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        var clear = new Button { Text = "&Temizle", AutoSize = true };
        start.Click += (_, _) => { session.Start(); notice.Text = session.IsRecording ? "Kayıt başladı. Deneme alanına yazabilirsiniz." : "5000 olay sınırına ulaşıldı. Yeniden başlamak için Temizle'yi seçin."; UpdateState(); practice.Focus(); };
        stop.Click += (_, _) => { session.Stop(); notice.Text = "Kayıt durduruldu. Mevcut olaylar korunuyor."; UpdateState(); };
        clear.Click += (_, _) => { session.Clear(); output.Clear(); notice.Text = "Olay listesi temizlendi."; UpdateState(); };
        save.Click += (_, _) => SaveLog();
        var toolbar = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = Padding.Empty };
        toolbar.Controls.AddRange([start, stop, clear, save]);
        var intro = new Label { AutoSize = true, Dock = DockStyle.Fill, Text = "Yalnızca bu pencere odaktayken KeyDown, KeyUp ve KeyPress olayları gösterilir.\nPencereden ayrılınca kayıt durur; geri dönünce Başlat ile yeniden açın. Kişisel bilgi yazmayın.", Padding = new Padding(0, 0, 0, 12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 8 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 75));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(intro, 0, 0);
        layout.Controls.Add(toolbar, 0, 1);
        layout.Controls.Add(state, 0, 2);
        layout.Controls.Add(new Label { Text = "&Deneme alanı", AutoSize = true }, 0, 3);
        layout.Controls.Add(practice, 0, 4);
        layout.Controls.Add(new Label { Text = "Olay listesi • zaman / olay türü / tuş", AutoSize = true, Padding = new Padding(0, 10, 0, 4) }, 0, 5);
        layout.Controls.Add(output, 0, 6);
        layout.Controls.Add(notice, 0, 7);
        Controls.Add(layout);
        KeyDown += (_, e) => RecordEvent(KeyEventKind.KeyDown, e.KeyData.ToString());
        KeyUp += (_, e) => RecordEvent(KeyEventKind.KeyUp, e.KeyData.ToString());
        KeyPress += (_, e) => RecordEvent(KeyEventKind.KeyPress, e.KeyChar.ToString());
        Deactivate += (_, _) => { if (!session.IsRecording) return; session.Stop(); notice.Text = "Pencere odağı kayboldu; kayıt durduruldu. Devam etmek için Başlat'ı seçin."; UpdateState(); };
        UpdateState();
    }

    private void RecordEvent(KeyEventKind kind, string key)
    {
        if (!ContainsFocus || Form.ActiveForm != this) return;
        if (!session.Record(kind, key, DateTimeOffset.Now, PasswordFocused())) return;
        output.AppendText(session.Entries[^1] + Environment.NewLine);
        output.SelectionStart = output.TextLength;
        output.ScrollToCaret();
        if (!session.IsRecording) notice.Text = "5000 olay sınırına ulaşıldı; kayıt durdu. Kaydedin veya Temizle ile yeni oturum açın.";
        UpdateState();
    }

    private bool PasswordFocused()
    {
        Control? focused = ActiveControl;
        while (focused is ContainerControl container && container.ActiveControl is { } child) focused = child;
        return focused is TextBox text && (text.UseSystemPasswordChar || text.PasswordChar != '\0')
            || focused is MaskedTextBox masked && (masked.UseSystemPasswordChar || masked.PasswordChar != '\0');
    }

    private void UpdateState()
    {
        state.Text = $"Durum: {(session.IsRecording ? "KAYIT AÇIK" : "DURDURULDU")}   •   Olay sayısı: {session.Entries.Count} / {LogSession.MaxEntries}";
        start.Enabled = !session.IsRecording && session.Entries.Count < LogSession.MaxEntries;
        stop.Enabled = session.IsRecording;
        save.Enabled = session.Entries.Count > 0;
    }

    private void SaveLog()
    {
        session.Stop();
        UpdateState();
        notice.Text = "Dosya seçimi için kayıt durduruldu.";
        using var dialog = new SaveFileDialog { Title = "Olayları UTF-8 metin dosyasına kaydet", Filter = "Metin dosyası (*.txt)|*.txt", DefaultExt = "txt", AddExtension = true, FileName = $"klavye-olaylari-{DateTime.Now:yyyyMMdd-HHmmss}.txt", OverwritePrompt = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) { notice.Text = "Kaydetme iptal edildi. Kayıt durdurulmuş durumda."; return; }
        try
        {
            session.Save(dialog.FileName);
            notice.Text = $"{session.Entries.Count} olay UTF-8 olarak kaydedildi: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or NotSupportedException)
        {
            notice.Text = "Dosya kaydedilemedi. Olaylar bellekte korunuyor; başka bir konum deneyin.";
            MessageBox.Show(this, $"Dosya kaydedilemedi. Yazma iznini ve disk alanını kontrol edin.\n\n{ex.Message}", "Kayıt hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
