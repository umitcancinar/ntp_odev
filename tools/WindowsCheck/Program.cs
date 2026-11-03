using System.Speech.Recognition;
using System.Speech.Synthesis;
using FaceRecognition.Services;
using OpenCvSharp;
using OpenCvSharp.Face;

// --devices aygıtları açar; --own-face-demo yüz kırpımlarını yalnız yerelde saklar.
Console.OutputEncoding = System.Text.Encoding.UTF8;
int failures = 0;
byte[]? generatedSpeech = null;
Check("OpenCV ve yüz algılama modeli", () =>
{
    Console.WriteLine($"OpenCV: {Cv2.GetVersionString()}");
    using var vision = new VisionService(Path.Combine(AppContext.BaseDirectory,
        "Models", "haarcascade_frontalface_default.xml"));
    using var blank = new Mat(480, 640, MatType.CV_8UC3, Scalar.All(0));
    if (vision.Analyze(blank, 70).Length != 0) throw new Exception("Boş karede yüz bulundu.");
});
Check("LBPH native eğitim ve tahmin (sentetik test verisi)", () =>
{
    var images = new List<Mat>();
    try
    {
        for (int i = 0; i < 6; i++)
        {
            var image = new Mat(160, 160, MatType.CV_8UC1, Scalar.All(30));
            images.Add(image);
            if (i < 3) Cv2.Circle(image, new OpenCvSharp.Point(75 + i, 75), 35, Scalar.All(220), -1);
            else Cv2.Rectangle(image, new Rect(30 + i, 30, 80, 100), Scalar.All(220), -1);
        }
        using var model = LBPHFaceRecognizer.Create();
        model.Train(images, new[] { 0, 0, 0, 1, 1, 1 });
        model.Predict(images[0], out int label, out double distance);
        if (label != 0 || !double.IsFinite(distance)) throw new Exception("LBPH sonucu geçersiz.");
        model.Train(images.Take(3), new[] { 0, 0, 0 });
        model.Predict(images[1], out label, out distance);
        if (label != 0 || !double.IsFinite(distance)) throw new Exception("Tek etiket LBPH sonucu geçersiz.");
        Console.WriteLine("İki etiket ve tek etiket native çağrıları geçti; gerçek yüz doğruluğu ölçülmedi.");
    }
    finally { foreach (var image in images) image.Dispose(); }
});
Check("SAPI sesleri ve ses üretimi", () =>
{
    using var speech = new SpeechSynthesizer();
    var voices = speech.GetInstalledVoices().Where(v => v.Enabled).ToArray();
    foreach (var voice in voices) Console.WriteLine($"Ses: {voice.VoiceInfo.Name} ({voice.VoiceInfo.Culture})");
    if (voices.Length == 0) throw new Exception("Kurulu SAPI sesi yok.");
    using var wave = new MemoryStream();
    speech.SetOutputToWaveStream(wave);
    speech.Speak("Windows speech test.");
    speech.SetOutputToNull();
    if (wave.Length < 100) throw new Exception("Ses üretilemedi.");
    generatedSpeech = wave.ToArray();
    Console.WriteLine($"Bellekte üretilen WAV: {wave.Length} bayt (kaydedilmedi).");
});
var recognizers = SpeechRecognitionEngine.InstalledRecognizers();
foreach (var recognizer in recognizers)
    Console.WriteLine($"Tanıyıcı: {recognizer.Name} ({recognizer.Culture})");
if (recognizers.Count == 0) { failures++; Console.WriteLine("FAIL SAPI tanıyıcısı yok; dikte için konuşma dili kurulmalı."); }
if (recognizers.Any(r => r.Culture.Name == "en-US")) Check("SAPI İngilizce tanıma: üretilmiş WAV girdisi", () =>
{
    var english = recognizers.FirstOrDefault(r => r.Culture.Name == "en-US")
        ?? throw new Exception("Bu deneme için en-US tanıyıcısı gerekli.");
    if (generatedSpeech is null) throw new Exception("Önce ses üretimi başarılı olmalı.");
    using var engine = new SpeechRecognitionEngine(english);
    engine.LoadGrammar(new DictationGrammar());
    using var input = new MemoryStream(generatedSpeech);
    engine.SetInputToWaveStream(input);
    var result = engine.Recognize(TimeSpan.FromSeconds(10));
    if (string.IsNullOrWhiteSpace(result?.Text)) throw new Exception("WAV girdisinden sonuç alınamadı.");
    Console.WriteLine($"Üretilmiş sesten sonuç: {result.Text} (mikrofon kullanılmadı).");
});
else Console.WriteLine("ATLANDI İngilizce WAV testi: en-US tanıyıcısı kurulu değil. Kurulu diğer diller uygulamada kullanılabilir.");
if (args.Contains("--devices"))
{
    Check("Kamera 0: kare alma", () =>
    {
        using var camera = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
        using var frame = new Mat();
        if (!camera.IsOpened() || !camera.Read(frame) || frame.Empty())
            throw new Exception("Kamera açılamadı veya kare alınamadı.");
        Console.WriteLine($"Kare: {frame.Width} x {frame.Height} (kaydedilmedi).");
    });
    Check("SAPI dikte grameri ve varsayılan mikrofon", () =>
    {
        if (recognizers.Count == 0) throw new Exception("Kurulu tanıyıcı yok.");
        using var engine = new SpeechRecognitionEngine(recognizers[0]);
        engine.LoadGrammar(new DictationGrammar());
        engine.SetInputToDefaultAudioDevice();
        Console.WriteLine("Giriş aygıtı açıldı; konuşma tanıma başlatılmadı.");
    });
}
if (args.Contains("--own-face-demo"))
{
    Check("Kendi yüzüm: yerel örnekler, LBPH eğitimi ve yeni kare", () =>
    {
        using var vision = new VisionService(Path.Combine(AppContext.BaseDirectory,
            "Models", "haarcascade_frontalface_default.xml"));
        using var camera = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
        using var frame = new Mat();
        var store = new SampleStore();
        const string label = "Kendi yüzüm";
        int saved = 0;
        for (int attempt = 0; attempt < 30 && saved < 5; attempt++)
        {
            Thread.Sleep(700);
            if (!camera.Read(frame) || frame.Empty()) continue;
            var faces = vision.Analyze(frame, 70);
            if (faces.Length != 1) continue;
            using var normalized = VisionService.Normalize(frame, faces[0].Bounds);
            store.Save(label, normalized);
            saved++;
        }
        if (saved < 5) throw new Exception($"Tek yüz içeren yeterli kare alınamadı ({saved}/5). Kaydedilen örnekler yalnız yerelde.");
        vision.Train(new Dictionary<string, string[]> { [label] = store.GetSamples()[label] }, ownFaceOnly: true);
        Thread.Sleep(1000);
        if (!camera.Read(frame) || frame.Empty()) throw new Exception("Test karesi alınamadı.");
        var results = vision.Analyze(frame, 70);
        if (results.Length != 1) throw new Exception("Yeni test karesinde tam bir yüz bulunmalı.");
        Console.WriteLine($"Yeni kare: {results[0].Label}; LBPH uzaklığı: {results[0].Distance:F2}.");
        Console.WriteLine("5 yeni örnek yalnız LOCALAPPDATA altında. Tek kişi denemesi, kişiler arası doğruluk ölçümü değildir.");
        if (results[0].Label != label) throw new Exception("Test yüzü eşik içinde tanınmadı.");
    });
}
Console.WriteLine($"Başarısız kontrol: {failures}. Bu kontrol gerçek dikte ve kişi tanıma denemesinin yerine geçmez.");
return failures == 0 ? 0 : 1;

void Check(string name, Action action)
{
    try { action(); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failures++; Console.WriteLine($"FAIL {name}: {ex.GetBaseException().Message}"); }
}
