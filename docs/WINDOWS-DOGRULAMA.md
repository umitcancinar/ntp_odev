# Windows doğrulaması — 3 Kasım 2026

Windows x64, .NET SDK 10.0.403 ve Windows Desktop Runtime 10.0.14 kullanıldı. Visual C++ x64 14.51 çalışma zamanı mevcut.

| Proje | Release derleme | Davranış testleri |
| --- | --- | --- |
| FlappyBird | 0 uyarı, 0 hata | 13/13 |
| KeyEventLogger | 0 uyarı, 0 hata | 10/10 |
| ChatApp | 0 uyarı, 0 hata | 12/12 |
| SpeechApp | 0 uyarı, 0 hata | 17/17 |
| FaceRecognition | 0 uyarı, 0 hata | 18/18 |
| Toplam | Beş başarılı derleme | 70/70 |

Komutlar:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action Build
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action Tests
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action NativeCheck
```

Çıktılar: [build.log](dogrulama/windows/build.log), [tests.log](dogrulama/windows/tests.log), [native.log](dogrulama/windows/native.log).

## Yerel bileşenler

- OpenCV 4.13.0 yüklendi; depodaki Haar cascade açıldı, boş karede yüz bulunmadı.
- Sentetik geometrik görüntülerle iki etiketli ve tek etiketli LBPH eğitim/tahmin çağrıları geçti. Bu kontrol kişi tanıma doğruluğu ölçmez.
- Kameradan 640×480 kare alındı; kare diske yazılmadı.
- SAPI ses üretimi başarılı. Türkçe Microsoft Tolga sesi ve İngilizce sesler mevcut.
- en-US SAPI tanıyıcısı mevcut; Türkçe tanıyıcı bulunmadı.
- Bellekte üretilen “Windows speech test” ses girdisi en-US tanıyıcısıyla metne çevrildi. Bu, mikrofondan dikte denemesi değildir.
- Varsayılan mikrofon SAPI tarafından açıldı; dikte grameri yüklendi.

Donanım kontrolü açıkça istenirse:

```powershell
dotnet run --project tools/WindowsCheck -c Release -- --devices
```

Kendi yüzüyle yerel eğitim denemesi için kameranın önünde yalnız kullanıcı bulunmalı:

```powershell
dotnet run --project tools/WindowsCheck -c Release -- --own-face-demo
```

Bu son komut beş yüz kırpımını yalnız `%LOCALAPPDATA%\YMH219-FaceRecognition\samples\Kendi yüzüm` altında saklar, model eğitir ve yeni kareyi dener. Örnekler repoya eklenmez. Yapılan denemede yeterli yüz algılanmadı (0/5); gerçek yüzle eğitim başarılı sayılmadı.

## Değişiklikler

- FlappyBird girişinde `KeyUp` olayının korunması için `SuppressKeyPress` yerine `Handled` kullanıldı; karakter olayı ayrıca işlenir. [Windows Forms klavye olayları](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/input-keyboard/events).
- KeyEventLogger dosya hatası testi, işletim sistemine göre istisna türü değişen bir dizine yazmak yerine bulunmayan alt dizine yazar. Başarısız kayıtta bellekteki olayların korunduğunu da denetler.
- Föydeki kendi yüzüyle çalışma alternatifi için FaceRecognition'a açık bir tek etiket seçeneği eklendi. Normal iki etiket koşulu korunur; her iki kullanımda da en az üç örnek gerekir.
- Windows başlatıcısı kullanıcıya kurulan SDK'yı `DOTNET_ROOT` üzerinden de bulur. Menüde `N` yerel bileşen kontrolünü çalıştırır.
- Beş README'ye öğrenci bilgileri eklendi: Ümitcan Çinar, 250541009.

## Bekleyen kontroller

FlappyBird ve KeyEventLogger süreçleri pencere açtı. Uygulama denetim aracı erişimi reddettiği için arayüz etkileşimleri ve on gerçek ekran görüntüsü tamamlanmadı. Mikrofonla gerçek dikte, gerçek yüz örnekleriyle tanıma ve DPI/odak kontrolleri ayrıca yapılmalı. Kişisel yüz görüntüleri public repoya yüklenmeyecek.

Beş ayrı teslim deposunun oluşturulması kullanıcıya bırakıldı. Aynı toplu repoya gönderim için GitHub kimlik doğrulaması gerekiyor. Önceki macOS hazırlığının raporu [DOGRULAMA.md](../DOGRULAMA.md) içinde korunmuştur.
