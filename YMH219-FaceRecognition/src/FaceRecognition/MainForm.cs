using System.Diagnostics;
using FaceRecognition.Services;
using Cv = OpenCvSharp;

namespace FaceRecognition;

public sealed class MainForm : Form
{
    private readonly PictureBox _preview = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(24, 28, 32), AccessibleName = "Kamera veya seçilen görüntü, işaretlenmiş yüzler" };
    private readonly Label _status = new() { AutoSize = true, Text = "Hazırlanıyor…", Padding = new Padding(8), AccessibleName = "İşlem durumu" };
    private readonly Label _results = new() { AutoSize = true, MaximumSize = new Size(900, 0), Text = "Kamerayı başlatın veya bir görüntü seçin." };
    private readonly NumericUpDown _cameraIndex = new() { Minimum = 0, Maximum = 9, Width = 55, AccessibleName = "Kamera numarası" };
    private readonly NumericUpDown _threshold = new() { Minimum = 10, Maximum = 200, Value = 70, Width = 75, AccessibleName = "Bilinmeyen yüz uzaklık eşiği" };
    private readonly TextBox _label = new() { Dock = DockStyle.Top, MaxLength = 40, AccessibleName = "Örneğin etiketi", PlaceholderText = "Örn. Kişi A" };
    private readonly CheckBox _consent = new() { AutoSize = true, MaximumSize = new Size(290, 0), Text = "Bu örnekler bana ait veya gönüllünün izni var." };
    private readonly CheckBox _ownFaceOnly = new() { AutoSize = true, MaximumSize = new Size(270, 0), Text = "Yalnızca kendi yüzümle çalışıyorum (tek etiket)." };
    private readonly ListBox _samples = new() { Dock = DockStyle.Fill, IntegralHeight = false, AccessibleName = "Yerel etiketler ve örnek sayıları" };
    private readonly Button _start = MakeButton("Kamerayı &Başlat");
    private readonly Button _stop = MakeButton("Kamerayı &Durdur");
    private readonly Button _open = MakeButton("&Görüntü Seç…");
    private readonly Button _save = MakeButton("Yüzü Örnek Olarak &Kaydet");
    private readonly Button _train = MakeButton("Örneklerle &Eğit");
    private readonly Button _folder = MakeButton("Yerel Örnek Klasörünü Aç");
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 120 };
    private VisionService? _vision;
    private SampleStore? _store;
    private Cv.VideoCapture? _capture;
    private Cv.Mat? _current;
    private FaceResult[] _faces = [];
    private Task<FramePacket>? _pendingFrame;
    private Task? _operation;
    private bool _busy, _processing, _closing, _canClose;

    public MainForm()
    {
        Text = "YMH219 · Yüz Algılama ve Yerel Tanıma";
        Font = new Font("Segoe UI", 10);
        ClientSize = new Size(1100, 760);
        MinimumSize = new Size(920, 680);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 4 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 8) };
        tools.Controls.AddRange([new Label { Text = "Kamera:", AutoSize = true, Margin = new Padding(0, 9, 4, 0) }, _cameraIndex, _start, _stop, _open]);
        root.Controls.Add(tools, 0, 0); root.SetColumnSpan(tools, 2);
        root.Controls.Add(_preview, 0, 1);
        root.Controls.Add(BuildTrainingPanel(), 1, 1);
        root.Controls.Add(_results, 0, 2); root.SetColumnSpan(_results, 2);
        root.Controls.Add(_status, 0, 3); root.SetColumnSpan(_status, 2);
        Controls.Add(root);

        _start.Click += async (_, _) => await RunOperationAsync(StartCameraAsync);
        _stop.Click += async (_, _) => await RunOperationAsync(StopCameraAsync);
        _open.Click += async (_, _) => await RunOperationAsync(OpenImageAsync);
        _save.Click += async (_, _) => await RunOperationAsync(SaveSampleAsync);
        _train.Click += async (_, _) => await RunOperationAsync(TrainAsync);
        _folder.Click += (_, _) =>
        {
            try { if (_store is not null) Process.Start(new ProcessStartInfo(_store.Root) { UseShellExecute = true }); }
            catch (Exception ex) { ReportError(ex); }
        };
        _consent.CheckedChanged += (_, _) => UpdateControls();
        _timer.Tick += async (_, _) => await RefreshCameraAsync();
        Shown += (_, _) => InitializeVision();
        FormClosing += OnClosing;
        UpdateControls();
    }

    private GroupBox BuildTrainingPanel()
    {
        var group = new GroupBox { Text = "Gönüllü yerel örnekler", Dock = DockStyle.Fill, Padding = new Padding(12) };
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 12, ColumnCount = 1 };
        for (int i = 0; i < 12; i++) panel.RowStyles.Add(new RowStyle(i == 5 ? SizeType.Percent : SizeType.AutoSize, i == 5 ? 100 : 0));
        panel.Controls.Add(new Label { Text = "Tek yüz görünürken etiket girip kaydedin. Her etiket için en az 3 farklı örnek ekleyin. İki gönüllü etiketi veya kendi yüzünüzle tek etiket kullanın.", AutoSize = true, MaximumSize = new Size(270, 0), Margin = new Padding(0, 0, 0, 12) }, 0, 0);
        panel.Controls.Add(new Label { Text = "Örnek etiketi:", AutoSize = true }, 0, 1);
        panel.Controls.Add(_label, 0, 2);
        panel.Controls.Add(_consent, 0, 3);
        panel.Controls.Add(_save, 0, 4);
        panel.Controls.Add(_samples, 0, 5);
        panel.Controls.Add(_ownFaceOnly, 0, 6);
        panel.Controls.Add(_train, 0, 7);
        panel.Controls.Add(new Label { Text = "Bilinmeyen yüz eşiği (LBPH):", AutoSize = true }, 0, 8);
        panel.Controls.Add(_threshold, 0, 9);
        panel.Controls.Add(new Label { Text = "Uzaklık düşükse benzerlik yüksektir. Skor bir güven yüzdesi değildir. Tek etiket, kişiler arası ayırımı ölçmez.", AutoSize = true, MaximumSize = new Size(270, 0), Margin = new Padding(0, 4, 0, 12) }, 0, 10);
        panel.Controls.Add(_folder, 0, 11);
        group.Controls.Add(panel);
        return group;
    }

    private void InitializeVision()
    {
        try
        {
            _store = new SampleStore();
            _vision = new VisionService(Path.Combine(AppContext.BaseDirectory, "Models", "haarcascade_frontalface_default.xml"));
            RefreshSamples();
            _status.Text = "Hazır · yalnızca yüz algılama. Tanıma için yerel örneklerle eğitim yapın.";
        }
        catch (Exception ex) { ReportError(ex); }
        UpdateControls();
    }

    private async Task RunOperationAsync(Func<Task> action)
    {
        if (_busy || _closing) return;
        _busy = true; UpdateControls();
        try { _operation = action(); await _operation; }
        catch (Exception ex) { if (!_closing) ReportError(ex); }
        finally { _operation = null; _busy = false; if (!_closing) UpdateControls(); }
    }

    private async Task StartCameraAsync()
    {
        if (_vision is null || _capture is not null) return;
        int index = (int)_cameraIndex.Value;
        _status.Text = "Kamera açılıyor…";
        _capture = await Task.Run(() =>
        {
            var camera = new Cv.VideoCapture(index, Cv.VideoCaptureAPIs.DSHOW);
            try
            {
                if (!camera.IsOpened()) throw new IOException("Kamera açılamadı. Kamera numarasını, Windows kamera iznini ve başka uygulamaların kamerayı kullanmadığını kontrol edin.");
                camera.Set(Cv.VideoCaptureProperties.FrameWidth, 640);
                camera.Set(Cv.VideoCaptureProperties.FrameHeight, 480);
                return camera;
            }
            catch { camera.Dispose(); throw; }
        });
        if (_closing) return;
        _timer.Start();
        _status.Text = "Kamera aktif · görüntüler otomatik kaydedilmez.";
    }

    private async Task StopCameraAsync()
    {
        _timer.Stop();
        var camera = _capture; _capture = null;
        if (_pendingFrame is not null)
            try { await _pendingFrame; } catch (Exception) { /* Kare hatasını okuma döngüsü bildirir. */ }
        camera?.Dispose();
        if (!_closing) _status.Text = "Kamera durduruldu · son görüntü ekranda.";
    }

    private async Task RefreshCameraAsync()
    {
        if (_processing || _busy || _closing || _capture is null || _vision is null) return;
        _processing = true; UpdateControls();
        var camera = _capture;
        double threshold = (double)_threshold.Value;
        try
        {
            _pendingFrame = Task.Run(() =>
            {
                var frame = new Cv.Mat();
                try
                {
                    if (!camera.Read(frame) || frame.Empty()) throw new IOException("Kameradan görüntü alınamadı. Bağlantıyı ve izinleri kontrol edip yeniden başlatın.");
                    return ProcessFrame(frame, threshold);
                }
                catch { frame.Dispose(); throw; }
            });
            FramePacket packet = await _pendingFrame;
            if (_closing) packet.Dispose(); else Display(packet);
        }
        catch (Exception ex)
        {
            await StopCameraAsync();
            if (!_closing) ReportError(ex);
        }
        finally { _pendingFrame = null; _processing = false; if (!_closing) UpdateControls(); }
    }

    private async Task OpenImageAsync()
    {
        using var dialog = new OpenFileDialog { Title = "Gönüllü yerel görüntü seç", Filter = "Görüntüler|*.png;*.jpg;*.jpeg;*.bmp|Tüm dosyalar|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await StopCameraAsync();
        double threshold = (double)_threshold.Value;
        _status.Text = "Görüntü işleniyor…";
        FramePacket packet = await Task.Run(() =>
        {
            var frame = Cv.Cv2.ImDecode(File.ReadAllBytes(dialog.FileName), Cv.ImreadModes.Color);
            try
            {
                if (frame.Empty()) throw new InvalidDataException("Bu dosya bir görüntü olarak okunamadı.");
                // Çok büyük dosyaları yüz algılama öncesi küçültür; en/boy oranı korunur.
                if (Math.Max(frame.Width, frame.Height) > 1600)
                    Cv.Cv2.Resize(frame, frame, new Cv.Size(), 1600.0 / Math.Max(frame.Width, frame.Height), 1600.0 / Math.Max(frame.Width, frame.Height));
                return ProcessFrame(frame, threshold);
            }
            catch { frame.Dispose(); throw; }
        });
        if (_closing) packet.Dispose(); else { Display(packet); _status.Text = "Görüntü yüklendi · özgün dosya değiştirilmedi."; }
    }

    private FramePacket ProcessFrame(Cv.Mat frame, double threshold)
    {
        FaceResult[] faces = _vision!.Analyze(frame, threshold);
        Cv.Cv2.ImEncode(".png", frame, out byte[] bytes);
        using var stream = new MemoryStream(bytes);
        using var decoded = new Bitmap(stream);
        var bitmap = new Bitmap(decoded);
        try
        {
            using var graphics = Graphics.FromImage(bitmap);
            using var pen = new Pen(Color.LimeGreen, 3);
            using var font = new Font("Segoe UI", Math.Max(10, bitmap.Width / 65f), FontStyle.Bold);
            foreach (var face in faces)
            {
                var r = face.Bounds;
                graphics.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                string caption = face.Distance is double d ? $"{face.Label} · {d:F1}" : face.Label;
                SizeF size = graphics.MeasureString(caption, font);
                float y = Math.Max(0, r.Y - size.Height);
                graphics.FillRectangle(Brushes.Black, r.X, y, Math.Min(size.Width + 4, bitmap.Width - r.X), size.Height);
                graphics.DrawString(caption, font, Brushes.White, r.X + 2, y);
            }
            return new FramePacket(frame, bitmap, faces);
        }
        catch { bitmap.Dispose(); throw; }
    }

    private void Display(FramePacket packet)
    {
        _current?.Dispose(); _preview.Image?.Dispose();
        _current = packet.Raw; _preview.Image = packet.Preview; _faces = packet.Faces;
        _results.Text = _faces.Length == 0 ? "Yüz algılanmadı. Kameraya dönün ve aydınlatmayı kontrol edin." :
            string.Join(" | ", _faces.Select(f => f.Distance is double d ? $"{f.Label}: LBPH uzaklığı {d:F1}" : f.Label));
    }

    private async Task SaveSampleAsync()
    {
        await StopCameraAsync();
        if (_current is null || _faces.Length != 1) throw new InvalidOperationException("Örnek kaydetmek için görüntüde tam olarak bir yüz olmalıdır.");
        if (!_consent.Checked) throw new InvalidOperationException("Örnek kaydetmeden önce gönüllü kullanım onayını işaretleyin.");
        using Cv.Mat face = VisionService.Normalize(_current, _faces[0].Bounds);
        _store!.Save(_label.Text, face);
        RefreshSamples();
        _status.Text = "Örnek yerel klasöre kaydedildi · örnekler değişti, tanıma için yeniden eğitin. Kamera durduruldu.";
    }

    private async Task TrainAsync()
    {
        await StopCameraAsync();
        var samples = _store!.GetSamples();
        bool ownFaceOnly = _ownFaceOnly.Checked;
        _status.Text = "LBPH modeli yerel örneklerle eğitiliyor…";
        await Task.Run(() => _vision!.Train(samples, ownFaceOnly));
        _status.Text = $"Eğitim tamamlandı · {samples.Count} etiket, {samples.Values.Sum(v => v.Length)} örnek. Kamerayı başlatın veya yeni bir test görüntüsü seçin.";
    }

    private void RefreshSamples()
    {
        _samples.Items.Clear();
        foreach (var pair in _store!.GetSamples().OrderBy(p => p.Key)) _samples.Items.Add($"{pair.Key} — {pair.Value.Length} örnek");
        if (_samples.Items.Count == 0) _samples.Items.Add("Henüz yerel örnek yok.");
    }

    private void UpdateControls()
    {
        bool ready = _vision is not null && !_busy && !_closing;
        _start.Enabled = ready && _capture is null;
        _stop.Enabled = ready && _capture is not null;
        _open.Enabled = ready;
        _cameraIndex.Enabled = ready && _capture is null;
        _save.Enabled = ready && _current is not null && _faces.Length == 1 && _consent.Checked;
        _train.Enabled = ready;
        _folder.Enabled = _store is not null && !_closing;
        _label.Enabled = ready; _consent.Enabled = ready; _threshold.Enabled = ready;
        _ownFaceOnly.Enabled = ready;
    }

    private void ReportError(Exception error)
    {
        string detail = error is DllNotFoundException or TypeInitializationException or BadImageFormatException
            ? "OpenCV başlatılamadı. Windows x64 sürümünü ve Visual C++ 2015–2022 x64 çalışma zamanını kontrol edin.\n" + error.GetBaseException().Message
            : error.Message;
        _status.Text = "İşlem tamamlanamadı · " + detail.Split('\n')[0];
        MessageBox.Show(this, detail, "İşlem tamamlanamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private async void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (_canClose) return;
        e.Cancel = true;
        if (_closing) return;
        _closing = true; Enabled = false; _timer.Stop();
        try
        {
            if (_operation is not null) try { await _operation; } catch (Exception) { }
            await StopCameraAsync();
        }
        finally
        {
            _timer.Dispose(); _current?.Dispose(); _preview.Image?.Dispose(); _vision?.Dispose();
            _canClose = true; Close();
        }
    }

    private static Button MakeButton(string text) => new() { Text = text, AutoSize = true, MinimumSize = new Size(0, 34), Padding = new Padding(6, 2, 6, 2), UseVisualStyleBackColor = true };
    private sealed record FramePacket(Cv.Mat Raw, Bitmap Preview, FaceResult[] Faces) : IDisposable
    {
        public void Dispose() { Raw.Dispose(); Preview.Dispose(); }
    }
}
