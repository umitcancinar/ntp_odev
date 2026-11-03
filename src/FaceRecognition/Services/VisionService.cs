using FaceRecognition.Core;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognition.Services;

public sealed record FaceResult(Rect Bounds, string Label, double? Distance);

/// <summary>UI tek işlem yürütür; bu nesnenin eşzamanlı çağrılması gerekmez.</summary>
public sealed class VisionService : IDisposable
{
    private readonly CascadeClassifier _detector;
    private LBPHFaceRecognizer? _recognizer;
    private string[] _labels = [];
    public bool IsTrained => _recognizer is not null;

    public VisionService(string cascadePath)
    {
        if (!File.Exists(cascadePath))
            throw new FileNotFoundException("Yüz algılama modeli bulunamadı. Models klasörünü uygulamayla birlikte kopyalayın.", cascadePath);
        _detector = new CascadeClassifier(cascadePath);
        if (_detector.Empty())
        {
            _detector.Dispose();
            throw new InvalidDataException("Yüz algılama modeli okunamadı; model dosyasını yeniden kopyalayın.");
        }
    }

    public FaceResult[] Analyze(Mat frame, double threshold)
    {
        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);
        Rect[] faces = _detector.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(50, 50));
        return faces.Select(bounds =>
        {
            if (_recognizer is null) return new FaceResult(bounds, "Yüz algılandı · eğitim gerekli", null);
            using Mat normalized = Normalize(frame, bounds);
            _recognizer.Predict(normalized, out int id, out double distance);
            string name = TrainingRules.IsKnown(id, distance, threshold) && id < _labels.Length ? _labels[id] : "Bilinmeyen";
            return new FaceResult(bounds, name, distance);
        }).ToArray();
    }

    public static Mat Normalize(Mat frame, Rect bounds)
    {
        using var cropped = new Mat(frame, bounds);
        using var gray = new Mat();
        Cv2.CvtColor(cropped, gray, ColorConversionCodes.BGR2GRAY);
        var normalized = new Mat();
        try
        {
            Cv2.Resize(gray, normalized, new OpenCvSharp.Size(160, 160));
            Cv2.EqualizeHist(normalized, normalized);
            return normalized;
        }
        catch { normalized.Dispose(); throw; }
    }

    public void Train(Dictionary<string, string[]> samples)
    {
        string? error = TrainingRules.ValidateTraining(samples.ToDictionary(p => p.Key, p => p.Value.Length));
        if (error is not null) throw new InvalidOperationException(error);
        string[] labels = samples.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        var images = new List<Mat>();
        var ids = new List<int>();
        LBPHFaceRecognizer? next = null;
        try
        {
            for (int id = 0; id < labels.Length; id++)
                foreach (string file in samples[labels[id]])
                {
                    var image = Cv2.ImDecode(File.ReadAllBytes(file), ImreadModes.Grayscale);
                    if (image.Empty() || image.Width != 160 || image.Height != 160)
                    {
                        image.Dispose();
                        throw new InvalidDataException($"Örnek dosyası geçersiz: {Path.GetFileName(file)}. Bozuk örneği yerel klasörden kaldırın.");
                    }
                    images.Add(image);
                    ids.Add(id);
                }
            next = LBPHFaceRecognizer.Create();
            next.Train(images, ids);
            _recognizer?.Dispose();
            _recognizer = next;
            next = null;
            _labels = labels;
        }
        finally
        {
            next?.Dispose();
            foreach (var image in images) image.Dispose();
        }
    }

    public void Dispose() { _recognizer?.Dispose(); _detector.Dispose(); }
}
