using System.Text;
namespace KeyEventLogger.Core;

/// <summary>Yalnızca formdan açıkça iletilen olayları saklar; sistem klavyesine erişmez.</summary>
public sealed class LogSession
{
    public const int MaxEntries = 5000;
    private readonly List<KeyLogEntry> entries = [];
    public bool IsRecording { get; private set; }
    public IReadOnlyList<KeyLogEntry> Entries { get; }

    public LogSession() => Entries = entries.AsReadOnly();
    public void Start() => IsRecording = entries.Count < MaxEntries;
    public void Stop() => IsRecording = false;
    public void Clear() => entries.Clear();

    public bool Record(KeyEventKind kind, string key, DateTimeOffset timestamp, bool passwordFocused = false)
    {
        if (!IsRecording || passwordFocused) return false;
        ArgumentException.ThrowIfNullOrEmpty(key);
        entries.Add(new KeyLogEntry(timestamp, kind, key));
        if (entries.Count >= MaxEntries) Stop();
        return true;
    }

    // Hatalar UI katmanında yakalanır; başarısız bir kayıt başarılı gibi gösterilmez.
    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var lines = new[] { "YMH219 — Form içi klavye olayları", "Zaman | Olay | Tuş / karakter" }
            .Concat(entries.Select(e => e.ToString()));
        File.WriteAllLines(path, lines, new UTF8Encoding(false));
    }
}
