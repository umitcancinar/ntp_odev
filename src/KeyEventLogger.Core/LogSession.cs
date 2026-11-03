namespace KeyEventLogger.Core;
public enum KeyEventKind { KeyDown, KeyUp, KeyPress }
public sealed record KeyLogEntry(DateTimeOffset Timestamp, KeyEventKind Kind, string Key)
{
    public override string ToString() => throw new NotImplementedException();
}
public sealed class LogSession
{
    public const int MaxEntries = 5000;
    public bool IsRecording { get; private set; }
    public IReadOnlyList<KeyLogEntry> Entries { get; } = new List<KeyLogEntry>();
    public void Start() => throw new NotImplementedException();
    public void Stop() => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Record(KeyEventKind kind, string key, DateTimeOffset timestamp, bool passwordFocused = false) => throw new NotImplementedException();
    public void Save(string path) => throw new NotImplementedException();
}
