# Doğrulama raporu — 3 Kasım 2026

Bu rapor macOS arm64 ortamında yapılan hazırlığı kapsar. Windows arayüzlerinin çalıştığı, kamera/mikrofonun kullanıldığı veya ödevlerin teslim edildiği anlamına gelmez.

## Kaynak kod kontrolleri

.NET SDK **10.0.403**, runtime **10.0.14** kullanıldı. Her çözüm için `dotnet build <çözüm>.sln -c Release --nologo`, sonra `dotnet run --project tests/<proje>.Tests -c Release --no-build` çalıştırıldı.

| Proje | Release derleme | Davranış testleri |
|---|---|---|
| FlappyBird | 0 uyarı / 0 hata | 13/13 |
| KeyEventLogger | 0 uyarı / 0 hata | 10/10 |
| ChatApp | 0 uyarı / 0 hata | 12/12 |
| SpeechApp | 0 uyarı / 0 hata | 17/17 |
| FaceRecognition | 0 uyarı / 0 hata | 15/15 |
| **Toplam** | **Beş başarılı derleme** | **67/67** |

Tam çıktı `docs/dogrulama/*-build.log` ve `*-tests.log` dosyalarındadır. Testler kendi konsol çalıştırıcılarıdır; komut `dotnet run` olmalıdır. Core testleri .NET üzerinde çalışır, WinForms yüklemez. Sohbet testleri mock ağ yerine gerçek localhost TCP bağlantıları kullanır; takılan sunucu, eşzamanlı mesaj, bozuk/çok uzun çerçeve, kopma, yeniden bağlanma ve kapanış sınanır.

Bağımsız kod incelemesi beş projede doğrulanmış Critical/Important bulgu raporlamadı. Yüz eğitimi README'sindeki küçük açıklama düzeltildi: düğme eksik veride devre dışı kalmaz, doğrulama uyarısı verir ve eğitimi engeller. İnceleme Windows native görünüm/donanım davranışını değerlendiremedi.

## OpenCV native smoke denemesi

İzole geçici macOS konsolunda OpenCvSharp **4.13.0.20260627** + aynı sürüm resmi `runtime.osx.arm64` ile gerçek native OpenCV **4.13.0** yüklendi. Depodaki cascade açıldı; boş 640×480 BGR karede sıfır yüz bulundu; normalizasyon 160×160 tek kanallı çıktı üretti; eksik model `FileNotFoundException` verdi; iki etikette üçer **sentetik geometrik test dokusu** ile LBPH eğitim çağrısı tamamlandı. Bu, gerçek kişi tanıma doğruluğu ölçümü değildir. Kişi fotoğrafı veya kamera verisi kullanılmadı. İzole harness çalışma dizini `/tmp/ntp5-opencv-smoke` idi; teslim koduna deneme verisi eklenmedi.

## Windows başlatıcı denemesi

Resmi taşınabilir PowerShell **7.6.6** macOS sürümüyle:
- Betik AST ayrıştırması: 0 hata.
- Farklı çalışma dizininden `Windows.ps1 -Action Check`: çıkış 0, SDK ve beş çözüm yolu doğrulandı.
- `-Action Tests -Project FaceRecognition`: çıkış 0, 15/15.
- Menü `K`: SDK ve kaynak yolu kontrolü çıkış 0.
- Menü `3`: Windows süreç başlatma API'si taklit edilen izole testte iki doğru Chat EXE yolu ve çalışma diziniyle çağrıldı. Hazır EXE varsa SDK kontrolü/derleme çağrılmadı; yoksa kaynak derleme yolu seçildi.

Gerçek Windows PowerShell 5.1, cmd.exe ve pencere açılışı Windows'ta ayrıca denenecek. Betik ASCII ve 5.1 uyumlu sözdizimiyle yazıldı. Kaynak çalıştırıcı, hedef .NET 10 çalışma zamanının bulunması için **.NET 10 SDK** kurulumunu ister; sadece daha yeni bir SDK'nın bulunması yeterli sayılmaz. Hazır EXE'ler SDK kontrolünden geçmeden açılır.

## EXE paketleri

Beş uygulama için aşağıdaki yapılandırma başarıyla yayımlandı:

```text
dotnet publish src/<proje>/<proje>.csproj -c Release -r win-x64
  --self-contained true -p:PublishSingleFile=true
  -p:IncludeNativeLibrariesForSelfExtract=true
  -p:EnableCompressionInSingleFile=true -o ../windows-apps/<proje>
```

EXE'ler x64 Windows ve .NET runtime'ı içerir. FaceRecognition XML modeli yanında taşınır; VC++ x64 native bağımlılığı bilgisayarda gerekebilir. **Publish başarısı, EXE'nin Windows'ta denenmiş olduğu iddiası değildir.**

## Hâlâ yapılacaklar

- Windows'ta beş arayüz, klavye/odak/DPI, sesli okuma/dikte, kamera, örnek ekleme ve kapanış denemeleri.
- İki etikete ait izinli yerel verilerle yüz tanıma; bağımsız test görüntüleri.
- Her depoya iki gerçek PNG ve README görsel bağlantıları (toplam en az 10).
- Öğrenci adı/numarasının doldurulması.
- Beş ayrı public GitHub reposu, mevcut commitlerin push edilmesi, erişim kontrolü.
- Classroom'a kullanıcının GitHub profil bağlantısını teslim etmesi.

Akış: [WINDOWS-AKSAM.md](WINDOWS-AKSAM.md).
