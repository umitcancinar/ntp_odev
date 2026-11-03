using FaceRecognition.Core;

// Bu testler yanlış etiketin diske yol olarak yazılmasını, eksik eğitimi
// ve bilinmeyen yüzün biliniyor gibi sunulmasını yakalar.
var tests = new (string Name, Action Run)[]
{
    ("Türkçe etiket korunur, dış boşluklar temizlenir", () => Equal("İpek Çınar", TrainingRules.NormalizeLabel("  İpek Çınar  "))),
    ("Boş etiket reddedilir", () => Reject(() => TrainingRules.NormalizeLabel("   "))),
    ("Dizin geçişi reddedilir", () => Reject(() => TrainingRules.NormalizeLabel("../başka"))),
    ("Windows aygıt adı reddedilir", () => Reject(() => TrainingRules.NormalizeLabel("CON"))),
    ("Çok uzun etiket reddedilir", () => Reject(() => TrainingRules.NormalizeLabel(new string('a', 41)))),
    ("Sıfır etiket ile eğitim engellenir", () => True(TrainingRules.ValidateTraining(new Dictionary<string, int>()) is not null)),
    ("Tek etiket ile eğitim engellenir", () => True(TrainingRules.ValidateTraining(new Dictionary<string, int> { ["Kişi A"] = 10 }) is not null)),
    ("Az örnekli ikinci etiket eğitimi engeller", () => True(TrainingRules.ValidateTraining(new Dictionary<string, int> { ["Kişi A"] = 3, ["Kişi B"] = 2 }) is not null)),
    ("İki etiket ve üçer örnek ile eğitim açılır", () => True(TrainingRules.ValidateTraining(new Dictionary<string, int> { ["Kişi A"] = 3, ["Kişi B"] = 3 }) is null)),
    ("Eşik altındaki geçerli etiket kabul edilir", () => True(TrainingRules.IsKnown(0, 42.5, 70))),
    ("Eşik sınırı kabul edilir", () => True(TrainingRules.IsKnown(1, 70, 70))),
    ("Eşiğin üstü bilinmeyendir", () => True(!TrainingRules.IsKnown(0, 70.01, 70))),
    ("OpenCV bilinmeyen etiketi reddedilir", () => True(!TrainingRules.IsKnown(-1, 10, 70))),
    ("Geçersiz sayısal skor kabul edilmez", () => True(!TrainingRules.IsKnown(0, double.NaN, 70) && !TrainingRules.IsKnown(0, double.PositiveInfinity, 70))),
    ("Negatif skor ve eşik reddedilir", () => True(!TrainingRules.IsKnown(0, -1, 70) && !TrainingRules.IsKnown(0, 10, -1)))
};
int failed = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failed++; Console.WriteLine($"FAIL {test.Name}: {ex.Message}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} passed");
return failed == 0 ? 0 : 1;

static void True(bool value) { if (!value) throw new Exception("Beklenen davranış sağlanmadı."); }
static void Equal(string expected, string actual) { if (expected != actual) throw new Exception($"Beklenen '{expected}', gelen '{actual}'."); }
static void Reject(Action action)
{
    try { action(); } catch (ArgumentException) { return; }
    throw new Exception("Geçersiz etiket ArgumentException ile reddedilmeliydi.");
}
