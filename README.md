# YMH219 — Beş Windows Forms Ödevi

Beş C#/.NET 10 Windows Forms uygulamasının kaynak kodları bu toplu çalışma deposundadır. Her proje kendi çözüm dosyası, README, testleri ve kod rehberiyle bağımsız derlenebilir. Önceki beş yerel deponun gerçek commit geçmişleri subtree aktarımıyla korunmuştur.

| Depo | Uygulama | Başlangıç |
|---|---|---|
| [YMH219-FlappyBird](YMH219-FlappyBird/README.md) | Timer, fizik, çarpışma, skor | `src/FlappyBird` |
| [YMH219-KeyEventLogger](YMH219-KeyEventLogger/README.md) | Yalnız kendi penceresindeki tuş olayları, TXT kaydı | `src/KeyEventLogger` |
| [YMH219-ChatApp](YMH219-ChatApp/README.md) | Asenkron TCP sunucu ve istemci | `src/ChatApp` |
| [YMH219-SpeechApp](YMH219-SpeechApp/README.md) | Windows SAPI ile TTS ve STT | `src/SpeechApp` |
| [YMH219-FaceRecognition](YMH219-FaceRecognition/README.md) | OpenCV algılama ve yerel LBPH tanıma | `src/FaceRecognition` |

## Windows'ta hızlı başlangıç

**Hazır Windows x64 EXE paketi ve kaynak yedeği:** [Releases](https://github.com/umitcancinar/ntp_odev/releases/tag/windows-hazirlik-2026-11-03).

ZIP'i **tamamen bir klasöre çıkarın**, ZIP içinden çalıştırmayın. `UYGULAMALARI-AC.cmd` dosyasını açın ve uygulama seçin. Menü, `windows-apps` içinde hazırlanmış x64 EXE varsa onu açar; yoksa .NET 10 SDK ile kaynakları derler. Terminal kullanımı:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action Check
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action Build
powershell -NoProfile -ExecutionPolicy Bypass -File .\Windows.ps1 -Action Tests
```

`-Project ChatApp` gibi bir ad ile yalnız bir proje seçilebilir. Betik yalnız bu işlem için execution policy parametresi kullanır, kalıcı Windows ayarını değiştirmez. Yönetici olarak çalıştırmak gerekmez. İlk NuGet geri yüklemesi internet ister. Hazır self-contained EXE'ler .NET SDK gerektirmez; yüz uygulaması için ayrıca VC++ x64 çalışma zamanı gerekebilir.

**Sonraki çalışma:** [WINDOWS-AKSAM.md](WINDOWS-AKSAM.md). Derleme/test kanıtı ve sınırları: [DOGRULAMA.md](DOGRULAMA.md).

## GitHub ve teslim

Bu GitHub reposu beş uygulamayı tek yerde toplar; klonlandığında tek üst Git deposu vardır. Projeler submodule değildir, kaynak kodları doğrudan içerir. Geliştirme commitleri geçmişte korunmuştur. Release içindeki `ntp5-kaynaklar.zip`, önceki beş bağımsız yerel Git deposunun `.git` geçmişlerini de içerir.

**Föy son teslimde beş ayrı public repo istiyor.** Bu toplu çalışma deposundan uygulamalar daha sonra ayrı teslim depolarına aktarılabilir.

Teslimden önce her README'de öğrenci adı/numarası doldurulacak ve uygulamadan **iki gerçek ekran görüntüsü** eklenecek. Şu an görüntüler yoktur; çekim senaryoları her deponun `docs/screenshots/README.md` dosyasındadır. Windows çalışması doğrulanmadan teslim tamamlandı sayılmaz.

Classroom'a yalnız GitHub profil bağlantısı; son teslim **30 Kasım 2026, 23:59 (Türkiye saati)**.
