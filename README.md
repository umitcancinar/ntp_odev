# YMH219 — OpenCV ile Yüz Algılama ve Yerel Tanıma

C# Windows Forms uygulaması kameradaki veya seçilen dosyadaki yüzleri Haar cascade ile bulur ve dikdörtgenlerle işaretler. Kullanıcının kendi oluşturduğu gönüllü yerel örnekler üzerinde LBPH ile basit etiket tahmini yapar.

**Öğrenci adı soyadı:** DOLDURULACAK  
**Öğrenci numarası:** DOLDURULACAK

## Teknolojiler
- .NET 10 SDK, C#, Windows Forms; Windows 10/11 **x64**.
- `OpenCvSharp4` ve `OpenCvSharp4.runtime.win` **4.13.0.20260627**.
- OpenCV 4.13 Haar `haarcascade_frontalface_default.xml` (depoda mevcut) ve LBPHFaceRecognizer.
- Harici bulut/API anahtarı, internetten kişi verisi veya hazır kimlik veritabanı kullanılmaz.

## Kurulum ve çalıştırma
1. Windows'a [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) kurun. Visual Studio kullanacaksanız .NET 10 destekleyen **Visual Studio 2026** ve “.NET masaüstü geliştirme” iş yükünü seçin. Visual Studio zorunlu değildir.
2. [Visual C++ x64 Redistributable](https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist) kurulumu OpenCV native kitaplığı için gerekebilir. Windows **N** sürümlerinde Media Feature Pack gerekir.
3. Depo klasöründe terminal açın:

```powershell
dotnet restore FaceRecognition.sln
dotnet build FaceRecognition.sln -c Release
dotnet run --project src/FaceRecognition -c Release
```

Visual Studio'da `FaceRecognition.sln` açılır; `FaceRecognition` başlangıç projesi seçilip F5 yapılır. NuGet geri yüklemesi ilk kurulumda internet ister. Algılama XML'i derleme çıktısına otomatik kopyalanır.

## Kullanım ve iki etiket demonstrasyonu
1. **Görüntü Seç** ile kendi fotoğrafınızı açın veya kamera numarasını seçip **Kamerayı Başlat** yapın. Windows Ayarlar → Gizlilik ve güvenlik → Kamera → masaüstü uygulamaları iznini kontrol edin.
2. Tam olarak **bir yüz** görünürken bir takma etiket yazın (ör. `Kişi A`), gönüllü kullanım kutusunu işaretleyin ve **Yüzü Örnek Olarak Kaydet** yapın. Kayıt kamera akışını durdurur; farklı örnek için yeniden başlatın veya başka dosya seçin.
3. Aynı etiket için en az **3 farklı örnek**, tercihen farklı aydınlatma ve küçük açılarla **5–10 örnek** ekleyin. İkinci gönüllü için `Kişi B` etiketiyle tekrarlayın. Föydeki alternatif kullanılırsa kendi yüzünüzün iki farklı örnek sınıfını açık adlarla etiketleyin; bu, iki kişiyi ayırt ettiğini kanıtlamaz.
4. **Örneklerle Eğit** yapın. Eğitim yalnızca en az iki etikette üçer geçerli örnek olduğunda açılır.
5. Eğitimde kullanmadığınız bir görüntü seçin veya kamerayı yeniden başlatın. Her yüzün etiketi ve **LBPH uzaklığı** gösterilir. Düşük uzaklık daha benzerdir; değer güven yüzdesi değildir. Varsayılan eşik **70** üzerindeyse `Bilinmeyen` gösterilir. Bu eşik küçük veri kümenize göre ayarlanmalıdır.
6. Sonradan örnek eklediyseniz yeniden eğitin. Model uygulama kapanınca silinir; örnekler kalır, sonraki açılışta yeniden eğitim yapabilirsiniz.

Örnekler `%LOCALAPPDATA%\YMH219-FaceRecognition\samples\<etiket>\` altında yalnızca yerelde saklanır. **Yerel Örnek Klasörünü Aç** ile inceleyebilir, uygulama kapalıyken örnekleri silebilirsiniz. Bu klasörü, gerçek isimleri ve kişisel yüz görüntülerini GitHub'a yüklemeyin. Kamuya açık alanlarda veya izinsiz kişiler üzerinde kullanmayın.

## Hata ve kaynak yönetimi
- Model eksik/bozuksa, kamera yoksa/başka uygulamada meşgulse, resim okunamıyorsa veya native OpenCV yüklenemiyorsa açıklama gösterilir.
- Birden fazla yüz algılanabilir; yanlış kişiye etiket eklememek için örnek kaydı tek yüzle sınırlandırılır.
- Kare işleme ve eğitim UI dışındaki görevlerde yürütülür. Aynı anda ikinci kare işlenmez; durdurma/kapanış bekleyen kareyi bitirip kamera, `Mat`, `Bitmap` ve model nesnelerini temizler.

## Doğrulama

```powershell
dotnet run --project tests/FaceRecognition.Tests -c Release
```

Bu komut platformdan bağımsız davranış testlerini çalıştırır; klasik `dotnet test` projesi değildir. Etiket güvenliği, minimum eğitim verisi, eşik ve geçersiz skorlar kontrol edilir. Windows hedefli Release derleme ve **15 davranış testi** macOS üzerinde doğrulandı. **Windows arayüzü, gerçek kamera ve gerçek gönüllü örneklerle LBPH başarımı henüz test edilmedi.** Ayrıntılı manuel kontrol: [docs/WINDOWS-KONTROL.md](docs/WINDOWS-KONTROL.md).

## Ekran görüntüleri — Windows aşamasında eklenecek
Föyün istediği iki gerçek uygulama ekran görüntüsü henüz alınmadı. [Çekim yönergesi](docs/screenshots/README.md) tamamlandıktan sonra README'ye şu iki dosyanın görsel bağlantısını ekleyin:

1. `docs/screenshots/01-yuz-algilama.png` — uygulama içinde dikdörtgenle işaretlenen yüz.
2. `docs/screenshots/02-yerel-tanima.png` — en az iki etiketle eğitim sonrası test yüzü, tahmin ve uzaklık.

## Bilinen sınırlamalar ve geliştirme notları
Önden bakan yüzler için eğitim amaçlı küçük veri kümesi demonstrasyonudur; güvenilir kimlik doğrulama sistemi değildir. Işık, poz, gözlük ve az veri sonucu etkiler. Büyük/kalabalık görüntüler ve kamera sürücüleri Windows denemesi gerektirir. Windows ARM64 yerel OpenCV dağıtımı bu projede hedeflenmemiştir; x64 Windows kullanın. Gerçek ekran görüntüleri, öğrenci bilgileri ve yerel iki etiket demonstrasyonu tamamlanmadan teslim hazır sayılmaz.

Kodun sözlü açıklaması: [KOD-REHBERI.md](KOD-REHBERI.md). Model kaynak ve lisansı: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
