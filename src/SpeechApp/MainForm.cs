using System.Globalization;
using SpeechApp.Core;

namespace SpeechApp;

public sealed class MainForm : Form
{
    private readonly RichTextBox _input = new() { Dock = DockStyle.Fill, DetectUrls = false, MaxLength = SpeechText.MaximumLength, AccessibleName = "Okunacak metin" };
    private readonly RichTextBox _output = new() { Dock = DockStyle.Fill, DetectUrls = false, ReadOnly = true, AccessibleName = "Konuşmadan dönüştürülen metin" };
    private readonly ComboBox _voices = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, AccessibleName = "Okuma sesi" };
    private readonly ComboBox _recognizers = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, AccessibleName = "Konuşma tanıyıcısı ve dili" };
    private readonly NumericUpDown _rate = new() { Minimum = -10, Maximum = 10, Value = 0, Width = 70, AccessibleName = "Okuma hızı, eksi on ile artı on arası" };
    private readonly Button _speak = MakeButton("&Başlat", "Metni sesli okumaya başla");
    private readonly Button _pause = MakeButton("&Duraklat", "Sesli okumayı duraklat");
    private readonly Button _resume = MakeButton("De&vam et", "Duraklatılmış okumaya devam et");
    private readonly Button _stop = MakeButton("D&urdur", "Sesli okumayı durdur");
    private readonly Button _clearInput = MakeButton("Metni &temizle", "Okumayı durdur ve giriş metnini temizle");
    private readonly Button _listen = MakeButton("&Dinlemeyi başlat", "Mikrofonu aç ve dikteyi başlat");
    private readonly Button _stopListening = MakeButton("Dinlemeyi durd&ur", "Mikrofon diktesini durdur");
    private readonly Button _clearOutput = MakeButton("Sonuçları temi&zle", "Dikte metnini temizle");
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Fill, AccessibleName = "İşlem durumu", Padding = new Padding(6), Text = "Hazırlanıyor. Mikrofon kapalı." };
    private readonly Label _availability = new() { AutoSize = true, Dock = DockStyle.Fill, AccessibleName = "Konuşma altyapısı bilgisi", Padding = new Padding(6) };
    private readonly Label _resultInfo = new() { AutoSize = true, Text = "Henüz dikte sonucu yok.", AccessibleName = "Son dikte sonucu bilgisi" };
    private readonly Label _counter = new() { AutoSize = true, Text = "0 / 20.000 karakter" };
    private SpeechService? _speech;
    private bool _closing;

    public MainForm()
    {
        Text = "YMH219 · Ses ve Metin Dönüştürücü";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        Size = new Size(1050, 850);
        MinimumSize = new Size(780, 720);
        StartPosition = FormStartPosition.CenterScreen;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 5 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { AutoSize = true, Text = "Metni okutun veya mikrofonla dikte edin. Aynı anda yalnızca bir işlem çalışır.", Padding = new Padding(0, 0, 0, 12) }, 0, 0);
        root.Controls.Add(BuildReadingPanel(), 0, 1);
        root.Controls.Add(BuildDictationPanel(), 0, 2);
        root.Controls.Add(_status, 0, 3);
        root.Controls.Add(_availability, 0, 4);
        Controls.Add(root);

        _speak.Click += (_, _) => _speech?.Speak(_input.Text, _voices.SelectedItem as VoiceOption, (int)_rate.Value);
        _pause.Click += (_, _) => _speech?.Pause();
        _resume.Click += (_, _) => _speech?.Resume();
        _stop.Click += (_, _) => _speech?.StopSpeaking();
        _clearInput.Click += (_, _) => { _speech?.StopSpeaking(); _input.Clear(); _input.Focus(); };
        _listen.Click += (_, _) => _speech?.StartListening(_recognizers.SelectedItem as RecognizerOption);
        _stopListening.Click += (_, _) => _speech?.StopListening();
        _clearOutput.Click += (_, _) => { _output.Clear(); _resultInfo.Text = "Sonuçlar temizlendi."; };
        _input.TextChanged += (_, _) => { _counter.Text = $"{_input.TextLength:N0} / 20.000 karakter"; RefreshControls(); };
        _voices.SelectedIndexChanged += (_, _) => RefreshControls();
        _recognizers.SelectedIndexChanged += (_, _) => RefreshControls();
        Shown += (_, _) => InitializeSpeech();
        FormClosing += (_, _) =>
        {
            _closing = true;
            if (_speech is null) return;
            _speech.Changed -= RefreshControls;
            _speech.Recognized -= AppendResult;
            _speech.Dispose();
        };
        RefreshControls();
    }

    private Control BuildReadingPanel()
    {
        var group = new GroupBox { Text = "1 · Metinden sese", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = SectionLayout();
        var settings = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 4, Margin = Padding.Empty };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.Controls.Add(new Label { Text = "&Ses:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        settings.Controls.Add(_voices, 1, 0);
        settings.Controls.Add(new Label { Text = "&Hız:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        settings.Controls.Add(_rate, 3, 0);
        layout.Controls.Add(settings, 0, 0);
        layout.Controls.Add(_input, 0, 1);
        layout.Controls.Add(Actions(_speak, _pause, _resume, _stop, _clearInput, _counter), 0, 2);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildDictationPanel()
    {
        var group = new GroupBox { Text = "2 · Sesten metne", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = SectionLayout();
        var settings = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        settings.Controls.Add(new Label { Text = "&Tanıyıcı:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        settings.Controls.Add(_recognizers, 1, 0);
        layout.Controls.Add(settings, 0, 0);
        layout.Controls.Add(_output, 0, 1);
        layout.Controls.Add(Actions(_listen, _stopListening, _clearOutput, _resultInfo), 0, 2);
        group.Controls.Add(layout);
        return group;
    }

    private static TableLayoutPanel SectionLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return panel;
    }

    private static FlowLayoutPanel Actions(params Control[] controls)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0, 4, 0, 0) };
        panel.Controls.AddRange(controls);
        return panel;
    }

    private static Button MakeButton(string text, string name) => new()
    {
        Text = text, AccessibleName = name, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Padding = new Padding(6, 3, 6, 3), Margin = new Padding(0, 3, 8, 3), MinimumSize = new Size(80, 34)
    };

    private void InitializeSpeech()
    {
        _speech = new SpeechService(Dispatch);
        _speech.Changed += RefreshControls;
        _speech.Recognized += AppendResult;
        _voices.Items.AddRange(_speech.Voices.Cast<object>().ToArray());
        _recognizers.Items.AddRange(_speech.Recognizers.Cast<object>().ToArray());
        if (_voices.Items.Count > 0) _voices.SelectedIndex = 0;
        var preferred = RecognizerSelection.Choose(_speech.Recognizers, CultureInfo.CurrentUICulture);
        if (preferred is not null) _recognizers.SelectedItem = preferred;
        _availability.Text = _speech.Availability + Environment.NewLine + "Türkçe dikte ancak listede Türkçe bir SAPI tanıyıcısı varsa mümkündür. Konuşmanız seçili dil ile eşleşmelidir.";
        RefreshControls();
    }

    private void Dispatch(Action action)
    {
        if (_closing || IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke((Action)(() => { if (!_closing && !IsDisposed) action(); })); }
        catch (InvalidOperationException) { /* The form's handle was destroyed between checks. */ }
    }

    private void AppendResult(DictationResult result)
    {
        if (_output.TextLength > 100_000)
        {
            _speech?.StopListening();
            _resultInfo.Text = "Metin alanı doldu. Sonuçları kopyalayıp temizleyin; sonra yeniden başlatın.";
            return;
        }
        _output.AppendText((_output.TextLength == 0 ? "" : Environment.NewLine) + result.Text);
        _resultInfo.Text = $"Son sonuç: {result.Time:HH:mm:ss} · Motor güveni: {result.Confidence:P0}";
    }

    private void RefreshControls()
    {
        var activity = _speech?.Activity ?? SpeechActivity.Idle;
        bool idle = activity == SpeechActivity.Idle;
        bool available = _speech is not null && !_closing;
        _speak.Enabled = available && idle && _voices.SelectedItem is VoiceOption && !string.IsNullOrWhiteSpace(_input.Text);
        _pause.Enabled = available && activity == SpeechActivity.Speaking;
        _resume.Enabled = available && activity == SpeechActivity.Paused;
        _stop.Enabled = available && activity is SpeechActivity.Speaking or SpeechActivity.Paused;
        _listen.Enabled = available && idle && _recognizers.SelectedItem is RecognizerOption;
        _stopListening.Enabled = available && activity == SpeechActivity.Listening;
        _voices.Enabled = available && idle && _voices.Items.Count > 0;
        _rate.Enabled = available && idle;
        _recognizers.Enabled = available && idle && _recognizers.Items.Count > 0;
        _input.ReadOnly = activity is SpeechActivity.Speaking or SpeechActivity.Paused or SpeechActivity.StoppingSpeech;
        _clearInput.Enabled = available && activity != SpeechActivity.StoppingSpeech;
        if (_speech is not null) _status.Text = _speech.Status;
    }
}
