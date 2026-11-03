using System.Globalization;
using SpeechApp.Core;

// Each case protects a user-visible rule, without emulating Windows audio APIs.
var cases = new (string Name, Action Test)[]
{
    ("Boş ve yalnız boşluk metni reddedilir", () => {
        foreach (var text in new string?[] { null, "", "  ", "\r\n\t" }) Throws<ArgumentException>(() => SpeechText.Validate(text));
    }),
    ("Türkçe metin korunur ve kenar boşluğu temizlenir", () => Equal("İyi günler, dünya!", SpeechText.Validate("  İyi günler, dünya!\n"))),
    ("Tam sınırdaki metin kabul edilir", () => Equal(20000, SpeechText.Validate(new string('a', 20000)).Length)),
    ("Sınırı aşan metin reddedilir", () => Throws<ArgumentException>(() => SpeechText.Validate(new string('a', 20001)))),
    ("Kurulu olmayan dil için hayali tanıyıcı oluşturulmaz", () => Equal<RecognizerOption?>(null, RecognizerSelection.Choose([], new("tr-TR")))),
    ("Kurulu tam dil eşleşmesi önceliklidir", () => {
        var en = new RecognizerOption("en", "English", new("en-US"));
        var tr = new RecognizerOption("tr", "Türkçe", new("tr-TR"));
        Equal(tr, RecognizerSelection.Choose([en, tr], new("tr-TR")));
    }),
    ("Aynı dilin kurulu lehçesi seçilebilir", () => {
        var en = new RecognizerOption("en", "English", new("en-US"));
        var fr = new RecognizerOption("fr", "Français", new("fr-FR"));
        Equal(fr, RecognizerSelection.Choose([en, fr], new("fr-CA")));
    }),
    ("Tercih edilen dil yoksa gerçek kurulu ilk seçenek döner", () => {
        var en = new RecognizerOption("en", "English", new("en-US"));
        Equal(en, RecognizerSelection.Choose([en], new("tr-TR")));
    }),
    ("Okuma sürerken dinleme ve ikinci okuma başlatılamaz", () => {
        var s = new SpeechSession(); Yes(s.TrySpeak()); No(s.TrySpeak()); No(s.TryListen()); Equal(SpeechActivity.Speaking, s.Activity);
    }),
    ("Dinleme sürerken sesli okuma başlatılamaz", () => {
        var s = new SpeechSession(); Yes(s.TryListen()); No(s.TrySpeak()); No(s.TryListen()); Equal(SpeechActivity.Listening, s.Activity);
    }),
    ("Duraklatma ve devam doğru sırada çalışır", () => {
        var s = new SpeechSession(); No(s.TryPause()); No(s.TryResume()); Yes(s.TrySpeak()); Yes(s.TryPause());
        Equal(SpeechActivity.Paused, s.Activity); No(s.TryPause()); No(s.TryListen()); Yes(s.TryResume()); Equal(SpeechActivity.Speaking, s.Activity);
    }),
    ("İptal tamamlanana kadar yeni iş başlatılamaz", () => {
        var s = new SpeechSession(); Yes(s.TrySpeak()); Yes(s.TryStopSpeech()); Equal(SpeechActivity.StoppingSpeech, s.Activity);
        No(s.TryListen()); No(s.TrySpeak()); No(s.TryResume()); s.CompleteSpeech(); Yes(s.TryListen());
    }),
    ("Duraklatılmış okuma durdurulabilir", () => {
        var s = new SpeechSession(); Yes(s.TrySpeak()); Yes(s.TryPause()); Yes(s.TryStopSpeech()); s.CompleteSpeech(); Equal(SpeechActivity.Idle, s.Activity);
    }),
    ("Dinleme iptali bitene kadar mikrofon yeniden açılamaz", () => {
        var s = new SpeechSession(); Yes(s.TryListen()); Yes(s.TryStopRecognition()); No(s.TrySpeak()); No(s.TryListen());
        s.CompleteRecognition(); Yes(s.TrySpeak());
    }),
    ("Eski okuma bitişi etkin dinlemeyi etkilemez", () => {
        var s = new SpeechSession(); Yes(s.TryListen()); s.CompleteSpeech(); Equal(SpeechActivity.Listening, s.Activity); No(s.TryStopSpeech());
    }),
    ("Eski dinleme bitişi etkin okumayı etkilemez", () => {
        var s = new SpeechSession(); Yes(s.TrySpeak()); s.CompleteRecognition(); Equal(SpeechActivity.Speaking, s.Activity); No(s.TryStopRecognition());
    }),
    ("Tamamlanan işler yeniden başlatılabilir", () => {
        var s = new SpeechSession(); Yes(s.TrySpeak()); s.CompleteSpeech(); Yes(s.TryListen()); s.CompleteRecognition(); Equal(SpeechActivity.Idle, s.Activity);
    })
};
int failed = 0;
foreach (var (name, test) in cases)
{
    try { test(); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {name}: {ex.Message}"); }
}
Console.WriteLine($"{cases.Length - failed}/{cases.Length} başarılı; {failed} başarısız.");
return failed == 0 ? 0 : 1;

static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Beklenen: {expected}; gelen: {actual}"); }
static void Yes(bool actual) => Equal(true, actual);
static void No(bool actual) => Equal(false, actual);
static void Throws<T>(Action action) where T : Exception {
    try { action(); } catch (T) { return; }
    throw new Exception($"{typeof(T).Name} bekleniyordu.");
}
