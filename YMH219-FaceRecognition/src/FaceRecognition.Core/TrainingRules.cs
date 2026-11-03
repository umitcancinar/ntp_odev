namespace FaceRecognition.Core;

public static class TrainingRules
{
    public static string NormalizeLabel(string label)
    {
        ArgumentNullException.ThrowIfNull(label);
        string value = label.Trim();
        if (value.Length is < 1 or > 40 || value.Any(c => !char.IsLetterOrDigit(c) && c is not ' ' and not '-' and not '_'))
            throw new ArgumentException("Etiket 1–40 harf, rakam, boşluk, tire veya alt çizgi içermelidir.", nameof(label));
        string upper = value.ToUpperInvariant();
        if (upper is "CON" or "PRN" or "AUX" or "NUL" ||
            (upper.Length == 4 && (upper.StartsWith("COM", StringComparison.Ordinal) || upper.StartsWith("LPT", StringComparison.Ordinal)) && upper[3] is >= '0' and <= '9'))
            throw new ArgumentException("Bu ad Windows tarafından ayrılmıştır; başka bir etiket kullanın.", nameof(label));
        return value;
    }

    public static string? ValidateTraining(IReadOnlyDictionary<string, int> counts, bool ownFaceOnly = false)
    {
        if (ownFaceOnly && counts.Count != 1) return "Kendi yüzüm çalışmasında yalnızca bir etiket kullanın.";
        if (!ownFaceOnly && counts.Count < 2) return "Eğitim için en az iki farklı etiket ekleyin veya kendi yüzüm seçeneğini kullanın.";
        if (counts.Any(pair => pair.Value < 3)) return "Her etiket için en az üç yüz örneği ekleyin (beş veya daha fazla önerilir).";
        return null;
    }

    // LBPH bir olasılık üretmez. Küçük uzaklık daha benzer örnek demektir.
    public static bool IsKnown(int label, double distance, double threshold) =>
        label >= 0 && double.IsFinite(distance) && double.IsFinite(threshold) &&
        distance >= 0 && threshold > 0 && distance <= threshold;
}
