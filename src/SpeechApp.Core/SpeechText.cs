namespace SpeechApp.Core;

public static class SpeechText
{
    public const int MaximumLength = 20_000;
    public static string Validate(string? text) => text ?? "";
}
