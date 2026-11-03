namespace FaceRecognition.Core;

public static class TrainingRules
{
    public static string NormalizeLabel(string label) => throw new NotImplementedException();
    public static string? ValidateTraining(IReadOnlyDictionary<string, int> counts) => throw new NotImplementedException();
    public static bool IsKnown(int label, double distance, double threshold) => throw new NotImplementedException();
}
