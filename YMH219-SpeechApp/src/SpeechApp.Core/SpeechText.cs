namespace SpeechApp.Core;

public static class SpeechText
{
    public const int MaximumLength = 20_000;

    public static string Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Okutmak için önce metin yazın.", nameof(text));
        if (text.Length > MaximumLength)
            throw new ArgumentException($"Metin en fazla {MaximumLength:N0} karakter olabilir.", nameof(text));
        return text.Trim();
    }
}
