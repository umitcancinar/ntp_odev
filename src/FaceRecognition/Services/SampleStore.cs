using FaceRecognition.Core;
using OpenCvSharp;

namespace FaceRecognition.Services;

public sealed class SampleStore
{
    public string Root { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YMH219-FaceRecognition", "samples");

    public SampleStore() => Directory.CreateDirectory(Root);

    public Dictionary<string, string[]> GetSamples() => Directory.GetDirectories(Root)
        .ToDictionary(path => new DirectoryInfo(path).Name, path => Directory.GetFiles(path, "*.png"), StringComparer.OrdinalIgnoreCase);

    public void Save(string label, Mat normalizedFace)
    {
        string directory = Path.Combine(Root, TrainingRules.NormalizeLabel(label));
        Directory.CreateDirectory(directory);
        // ImEncode + File.WriteAllBytes, Windows'ta Türkçe dizin adlarını da destekler.
        Cv2.ImEncode(".png", normalizedFace, out byte[] bytes);
        File.WriteAllBytes(Path.Combine(directory, $"{Guid.NewGuid():N}.png"), bytes);
    }
}
