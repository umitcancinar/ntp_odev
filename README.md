# YMH219 — Nesne Tabanlı Programlama Ödevleri

## Haftalık laboratuvar ödevleri

**[Hafta 5 Evim](Hafta5_Evim/README.md)** — sınıflar, doğrulamalı property, yapıcı metotlar, statik sayaç ve sabit adres. Dört kişilik konsol uygulaması, 20 davranış testi, özgün föy ve teslim raporları hazırdır.

- [Word raporu](Hafta5_Evim/teslim/Hafta5_NTP_Odev.docx)
- [PDF raporu](Hafta5_Evim/teslim/Hafta5_NTP_Odev.pdf)
- [Teslim ZIP'i](Hafta5_Evim/teslim/250541009_NTP_Hafta5.zip)

## Ek uygulama ödevleri

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

`-Project ChatApp` gibi bir ad ile yalnız bir proje seçilebilir. `-Action NativeCheck` (menüde `N`) OpenCV ve SAPI bileşenlerini donanımı açmadan denetler. Betik yalnız bu işlem için execution policy parametresi kullanır, kalıcı Windows ayarını değiştirmez. Yönetici olarak çalıştırmak gerekmez. İlk NuGet geri yüklemesi internet ister. Hazır self-contained EXE'ler .NET SDK gerektirmez; yüz uygulaması için ayrıca VC++ x64 çalışma zamanı gerekebilir.

Windows üzerinde beş Release derleme (0 uyarı / 0 hata), 70/70 davranış testi, OpenCV, SAPI ve cihaz erişim kontrolleri geçti. Ayrıntılar: [Windows doğrulaması](docs/WINDOWS-DOGRULAMA.md). Etkileşimli arayüz kontrolleri ve gerçek ekran görüntüleri bekliyor.

## GitHub ve teslim

Bu GitHub reposu beş uygulamayı tek yerde toplar; klonlandığında tek üst Git deposu vardır. Projeler submodule değildir, kaynak kodları doğrudan içerir. Geliştirme commitleri geçmişte korunmuştur. Release içindeki `ntp5-kaynaklar.zip`, önceki beş bağımsız yerel Git deposunun `.git` geçmişlerini de içerir.

**Föy son teslimde beş ayrı public repo istiyor.** Bu toplu çalışma deposundan uygulamalar daha sonra ayrı teslim depolarına aktarılabilir.

Öğrenci: **Ümitcan Çinar — 250541009**. Her uygulamadan **iki gerçek ekran görüntüsü** eklenecek. Şu an görüntüler yoktur; çekim senaryoları her deponun `docs/screenshots/README.md` dosyasındadır. Pencere etkileşimleri, gerçek mikrofon diktesi ve yerel yüz eğitimi tamamlanmalı.

Classroom'a yalnız GitHub profil bağlantısı; son teslim **30 Kasım 2026, 23:59 (Türkiye saati)**.
