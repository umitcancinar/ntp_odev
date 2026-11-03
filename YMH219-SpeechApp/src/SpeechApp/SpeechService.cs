using System.Speech.Recognition;
using System.Speech.Synthesis;
using SpeechApp.Core;

namespace SpeechApp;

public sealed record VoiceOption(string Name, string Culture)
{
    public override string ToString() => $"{Name} ({Culture})";
}

public sealed record DictationResult(string Text, float Confidence, DateTimeOffset Time);

/// <summary>Owns Windows speech resources. Public operations and dispatched callbacks run on the UI thread.</summary>
public sealed class SpeechService : IDisposable
{
    private readonly Action<Action> _dispatch;
    private readonly SpeechSession _session = new();
    private SpeechSynthesizer? _synthesizer;
    private SpeechRecognitionEngine? _recognizer;
    private Prompt? _prompt;
    private volatile bool _disposed;

    public IReadOnlyList<VoiceOption> Voices { get; private set; } = [];
    public IReadOnlyList<RecognizerOption> Recognizers { get; private set; } = [];
    public SpeechActivity Activity => _session.Activity;
    public string Status { get; private set; } = "Hazır. Mikrofon kapalı.";
    public string Availability { get; private set; } = "";
    public event Action? Changed;
    public event Action<DictationResult>? Recognized;

    public SpeechService(Action<Action> dispatch)
    {
        _dispatch = dispatch;
        var notes = new List<string>();
        try
        {
            _synthesizer = new SpeechSynthesizer();
            // The audio device is opened only when the user starts speaking.
            _synthesizer.SpeakCompleted += OnSpeakCompleted;
            Voices = _synthesizer.GetInstalledVoices().Where(v => v.Enabled)
                .Select(v => new VoiceOption(v.VoiceInfo.Name, v.VoiceInfo.Culture.Name)).ToArray();
            if (Voices.Count == 0) notes.Add("Metin okuma için kullanılabilir SAPI sesi bulunamadı.");
        }
        catch (Exception ex)
        {
            ReleaseSynthesizer();
            notes.Add("Sesli okuma başlatılamadı: " + Explain(ex));
        }
        try
        {
            Recognizers = SpeechRecognitionEngine.InstalledRecognizers()
                .Select(r => new RecognizerOption(r.Id, r.Name, r.Culture)).ToArray();
            if (Recognizers.Count == 0)
                notes.Add("Kurulu SAPI konuşma tanıyıcısı yok. Dikte kullanılamıyor; Windows konuşma dili kurulumunu kontrol edin.");
        }
        catch (Exception ex) { notes.Add("Tanıyıcılar listelenemedi: " + Explain(ex)); }
        Availability = notes.Count == 0
            ? "Listeler yalnızca bu bilgisayarda kurulu SAPI seslerini ve tanıyıcılarını gösterir."
            : string.Join(Environment.NewLine, notes);
    }

    public void Speak(string text, VoiceOption? voice, int rate)
    {
        if (_disposed || Activity != SpeechActivity.Idle) return;
        try
        {
            text = SpeechText.Validate(text);
            if (_synthesizer is null || voice is null) throw new InvalidOperationException("Kullanılabilir bir okuma sesi seçin.");
            if (rate is < -10 or > 10) throw new ArgumentOutOfRangeException(nameof(rate));
            _synthesizer.SelectVoice(voice.Name);
            _synthesizer.Rate = rate;
            _synthesizer.SetOutputToDefaultAudioDevice();
            if (!_session.TrySpeak()) return;
            _prompt = _synthesizer.SpeakAsync(text);
            SetStatus("Metin okunuyor. Mikrofon kapalı.");
        }
        catch (Exception ex)
        {
            _prompt = null;
            _session.CompleteSpeech();
            SetStatus("Okuma başlatılamadı: " + Explain(ex));
        }
    }

    public void Pause()
    {
        if (_disposed || Activity != SpeechActivity.Speaking || _synthesizer is null) return;
        try { _synthesizer.Pause(); _session.TryPause(); SetStatus("Okuma duraklatıldı. Devam et veya Durdur seçin."); }
        catch (Exception ex) { SetStatus("Duraklatılamadı: " + Explain(ex)); }
    }

    public void Resume()
    {
        if (_disposed || Activity != SpeechActivity.Paused || _synthesizer is null) return;
        try { _synthesizer.Resume(); _session.TryResume(); SetStatus("Metin okunuyor. Mikrofon kapalı."); }
        catch (Exception ex) { SetStatus("Devam edilemedi: " + Explain(ex)); }
    }

    public void StopSpeaking()
    {
        if (_disposed || !_session.TryStopSpeech()) return;
        SetStatus("Okuma durduruluyor. Mikrofon kapalı.");
        try
        {
            _synthesizer!.SpeakAsyncCancelAll();
            // Flush cancellation even if the synthesizer was paused.
            if (_synthesizer.State == SynthesizerState.Paused) _synthesizer.Resume();
        }
        catch (Exception ex)
        {
            // Do not allow microphone capture after uncertain cancellation.
            SetStatus("Okuma durdurulamadı. Uygulamayı kapatıp yeniden açın: " + Explain(ex));
        }
    }

    public void StartListening(RecognizerOption? option)
    {
        if (_disposed || Activity != SpeechActivity.Idle) return;
        try
        {
            if (option is null || !Recognizers.Any(x => x.Id == option.Id))
                throw new InvalidOperationException("Kurulu bir konuşma tanıyıcısı seçin.");
            _recognizer = new SpeechRecognitionEngine(option.Id);
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SpeechRecognized += OnSpeechRecognized;
            _recognizer.SpeechRecognitionRejected += OnSpeechRejected;
            _recognizer.RecognizeCompleted += OnRecognizeCompleted;
            // Never access the microphone at startup, only after an explicit click.
            _recognizer.SetInputToDefaultAudioDevice();
            if (!_session.TryListen()) { ReleaseRecognizer(); return; }
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            SetStatus($"Mikrofon AÇIK · Dinleniyor ({option.Culture.Name}). Durdur ile kapatın.");
        }
        catch (Exception ex)
        {
            ReleaseRecognizer();
            _session.CompleteRecognition();
            SetStatus("Dikte başlatılamadı. Varsayılan mikrofonu, masaüstü uygulama iznini ve konuşma dilini kontrol edin. " + Explain(ex));
        }
    }

    public void StopListening()
    {
        if (_disposed || !_session.TryStopRecognition()) return;
        SetStatus("Mikrofon kapatılıyor; işlemin bitmesi bekleniyor.");
        try { _recognizer!.RecognizeAsyncCancel(); }
        catch (Exception ex)
        {
            ReleaseRecognizer();
            _session.CompleteRecognition();
            SetStatus("Dinleme sonlandırıldı: " + Explain(ex));
        }
    }

    private void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e) => Post(() =>
    {
        if (!ReferenceEquals(e.Prompt, _prompt)) return;
        _prompt = null;
        _session.CompleteSpeech();
        SetStatus(e.Error is not null ? "Okuma hatası: " + Explain(e.Error)
            : e.Cancelled ? "Okuma durduruldu. Mikrofon kapalı." : "Okuma tamamlandı. Mikrofon kapalı.");
    });

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e) => Post(() =>
    {
        // Ignore already-queued results after stop or after the engine has been replaced.
        if (!ReferenceEquals(sender, _recognizer) || Activity != SpeechActivity.Listening || string.IsNullOrWhiteSpace(e.Result.Text)) return;
        Recognized?.Invoke(new DictationResult(e.Result.Text, e.Result.Confidence, DateTimeOffset.Now));
    });

    private void OnSpeechRejected(object? sender, SpeechRecognitionRejectedEventArgs e) => Post(() =>
    {
        if (ReferenceEquals(sender, _recognizer) && Activity == SpeechActivity.Listening)
            SetStatus("Mikrofon AÇIK · Sözler anlaşılamadı. Seçili tanıyıcının dilinde, mikrofona daha yakın konuşun.");
    });

    private void OnRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e) => Post(() =>
    {
        if (!ReferenceEquals(sender, _recognizer)) return;
        ReleaseRecognizer();
        _session.CompleteRecognition();
        SetStatus(e.Error is not null ? "Dinleme sona erdi. Mikrofon kapalı. " + Explain(e.Error)
            : "Dinleme durduruldu. Mikrofon kapalı.");
    });

    private void Post(Action action)
    {
        if (_disposed) return;
        _dispatch(() => { if (!_disposed) action(); });
    }

    private void SetStatus(string text) { Status = text; Changed?.Invoke(); }

    private static string Explain(Exception ex) => ex switch
    {
        DllNotFoundException or PlatformNotSupportedException or TypeInitializationException =>
            "Windows SAPI konuşma bileşenleri yüklenemedi. Bu uygulama Windows gerektirir. " + ex.Message,
        UnauthorizedAccessException => "Mikrofona erişim izni verilmedi. " + ex.Message,
        _ => ex.Message
    };

    private void ReleaseRecognizer()
    {
        var recognizer = _recognizer;
        _recognizer = null;
        if (recognizer is null) return;
        recognizer.SpeechRecognized -= OnSpeechRecognized;
        recognizer.SpeechRecognitionRejected -= OnSpeechRejected;
        recognizer.RecognizeCompleted -= OnRecognizeCompleted;
        try { recognizer.Dispose(); } catch (Exception) { /* Best-effort native cleanup during failure/closing. */ }
    }

    private void ReleaseSynthesizer()
    {
        var synthesizer = _synthesizer;
        _synthesizer = null;
        if (synthesizer is null) return;
        synthesizer.SpeakCompleted -= OnSpeakCompleted;
        try { synthesizer.Dispose(); } catch (Exception) { /* Best-effort native cleanup during failure/closing. */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // Unsubscribe before cancelling: late callbacks must never touch a closing form.
        if (_recognizer is not null)
        {
            _recognizer.SpeechRecognized -= OnSpeechRecognized;
            _recognizer.SpeechRecognitionRejected -= OnSpeechRejected;
            _recognizer.RecognizeCompleted -= OnRecognizeCompleted;
            try { _recognizer.RecognizeAsyncCancel(); } catch (Exception) { }
        }
        if (_synthesizer is not null)
        {
            _synthesizer.SpeakCompleted -= OnSpeakCompleted;
            try
            {
                _synthesizer.SpeakAsyncCancelAll();
                if (_synthesizer.State == SynthesizerState.Paused) _synthesizer.Resume();
            }
            catch (Exception) { }
        }
        ReleaseRecognizer();
        ReleaseSynthesizer();
        _prompt = null;
    }
}
