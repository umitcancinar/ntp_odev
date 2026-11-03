using System.Globalization;

namespace SpeechApp.Core;

public sealed record RecognizerOption(string Id, string Name, CultureInfo Culture)
{
    public override string ToString() => $"{Name} ({Culture.Name})";
}

public static class RecognizerSelection
{
    // The preferred UI language is a preference, never a claim that a recognizer exists.
    public static RecognizerOption? Choose(IReadOnlyList<RecognizerOption> installed, CultureInfo preferred) =>
        installed.FirstOrDefault(x => x.Culture.Name == preferred.Name)
        ?? installed.FirstOrDefault(x => x.Culture.TwoLetterISOLanguageName == preferred.TwoLetterISOLanguageName)
        ?? installed.FirstOrDefault();
}
