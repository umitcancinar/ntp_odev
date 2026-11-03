using System.Globalization;
namespace KeyEventLogger.Core;

public enum KeyEventKind { KeyDown, KeyUp, KeyPress }

public sealed record KeyLogEntry(DateTimeOffset Timestamp, KeyEventKind Kind, string Key)
{
    public override string ToString() =>
        $"{Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)} | {Kind,-8} | {Escape(Key)}";

    private static string Escape(string value) => value.Replace("\\", "\\\\")
        .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
}
