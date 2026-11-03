using System.Globalization;

namespace SpeechApp.Core;

public sealed record RecognizerOption(string Id, string Name, CultureInfo Culture)
{
    public override string ToString() => $"{Name} ({Culture.Name})";
}

public static class RecognizerSelection
{
    public static RecognizerOption? Choose(IReadOnlyList<RecognizerOption> installed, CultureInfo preferred) => null;
}
