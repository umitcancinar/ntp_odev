# Kodu açıklama rehberi

- `Program.cs`: STA giriş noktası ve WinForms mesaj döngüsü.
- `MainForm.cs`: Kamera/dosya komutları, durumlar, görseller ve olaylar. `Timer` her 120 ms'de kare ister; `_processing` önceki kare bitmeden yenisini başlatmaz. Ağır işler `Task.Run` ile yapılır, `await` sonrası UI iş parçacığına dönülür.
- `VisionService`: Haar cascade yüzün **nerede olduğunu** bulur. LBPH, normalize edilen yüzün **hangi yerel etikete benzediğini** tahmin eder. İkisi farklı işlemdir. `Train` etiketleri sayısal kimliğe eşler. OpenCV için yüzler 160×160 gri tona dönüştürülüp histogram eşitlemesi uygulanır.
- `SampleStore`: Yüz kırpımlarını yalnızca kullanıcı profilindeki yerel klasöre yazar. Türkçe dosya yolları için `ImEncode` ve .NET dosya API'leri kullanılır.
- `TrainingRules`: UI ve native OpenCV'den bağımsız doğrulama. Etiketin `../` gibi dizin geçişine veya Windows aygıt adına dönüşmesini engeller. Normalde en az iki etikette üçer örnek ister. ownFaceOnly seçeneğinde tek etikette en az üç örnek kabul edilir; bu, föydeki kendi yüzüyle çalışma alternatifidir. `NaN`, sonsuz, negatif veya eşik üzerindeki skor biliniyor sayılmaz.
- `FaceResult` verisi algılanan dikdörtgen, etiket ve nullable uzaklık taşır. Model yoksa uzaklık `null` olur; UI uydurma tahmin göstermez.
- `IDisposable`: `Mat`, `VideoCapture`, cascade ve LBPH native bellek kullanır. Sadece GC'ye güvenmek kamerayı/belleği gereksiz tutabilir. Kapanış bekleyen işlemleri bitirdikten sonra kaynakları bırakır.

## Derste gösterebileceğin küçük değişiklikler
1. `TrainingRules.ValidateTraining` içindeki minimum örnek sayısını ve ilgili testi birlikte değiştir.
2. UI eşiğini değiştirip aynı test görüntüsünde `Bilinmeyen` kararını karşılaştır.
3. Kamera `Timer` aralığını 120'den 200'e çıkarıp CPU/akış farkını gözlemle.
4. `MainForm.ProcessFrame` içindeki dikdörtgen kalınlığını değiştir.

Sorulursa: LBPH uzaklığı doğruluk yüzdesi değildir. Eğitim görüntüsünü tekrar test etmek bağımsız başarı ölçümü değildir; test için ayrı görüntü kullanılır.
