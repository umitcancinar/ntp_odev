# YMH219-FlappyBird

Windows Forms üzerinde iki engel çiftinden kaçılarak puan toplanan, sınıflara ayrılmış masaüstü oyunu. Nesne Tabanlı Programlama (YMH 219) ödevi.

**Öğrenci:** [AD SOYAD]  
**Öğrenci numarası:** [NUMARA]

## Teknolojiler

C#, .NET 10, Windows Forms, `System.Windows.Forms.Timer`, GDI+ çizimi. Harici NuGet paketi yoktur. `FlappyBird.Core` platformdan bağımsız fizik ve oyun kurallarını içerir. Her proje bu depo içindedir; diğer ödevlere bağımlı değildir.

## Kurulum ve çalıştırma

Windows 10/11 ve .NET 10 SDK gerekir. Visual Studio kullanılacaksa .NET 10 destekleyen sürümde **.NET masaüstü geliştirme** iş yükünü kurun. Depo kökünde PowerShell açın:

```powershell
dotnet build FlappyBird.sln -c Release
dotnet run --project src/FlappyBird -c Release
```

Visual Studio'da `FlappyBird.sln` dosyasını açıp `FlappyBird` projesini başlangıç projesi seçerek F5 kullanabilirsiniz. İlk derleme Windows Desktop hedefleme paketini indirebilir.

Windows için dağıtım dosyası oluşturmak isterseniz:

```powershell
dotnet publish src/FlappyBird -c Release -r win-x64 --self-contained false -o publish
```

Bu dağıtım Windows'ta .NET 10 Desktop Runtime gerektirir. macOS'ta alınan mevcut `bin/` çıktısını taşımak yerine Windows'ta derleyin veya yukarıdaki Windows hedefli publish komutunu kullanın.

## Özellikler ve kullanım

- **Yeni Oyun / R:** Skoru, kuşu, engelleri ve tuş durumunu sıfırlar.
- **Boşluk / Yukarı ok:** Her yeni basışta bir zıplama. Basılı tutmak tekrar zıplatmaz.
- **Duraklat / P:** Oyunu duraklatır veya devam ettirir. Başka pencereye geçildiğinde oyun otomatik duraklar.
- Kuş yerçekimiyle düşer. Üst sınır, zemin veya boruyla çarpışınca oyun biter.
- İki üst-alt boru çifti sola ilerler; ekrandan çıkan çift aynı nesne olarak sağ tarafa taşınır.
- Her engel çifti tam geçildiğinde yalnızca bir puan eklenir.
- Yeniden boyutlandırma oyun dünyasının oranını korur; sistem teması, Segoe UI ve görünür klavye odağı kullanılır.

## Test ve mevcut doğrulama

```powershell
dotnet run --project tests/FlappyBird.Tests -c Release
```

Çerçevesiz konsol çalıştırıcısı **13 davranış testi** içerir; başarısızlıkta sıfırdan farklı çıkış kodu üretir. Fizik, çarpışma, skorun tek sayılması, engel geri dönüşümü, tuş tekrarı, oyun sonu, sıfırlama ve geçersiz zaman adımı sınanır.

3 Kasım 2026 macOS doğrulaması: Release çözüm derlemesi **0 uyarı / 0 hata**, testler **13/13 başarılı**. Windows Forms penceresi macOS'ta çalıştırılamaz; gerçek Windows çalıştırması henüz doğrulanmadı. [Windows kontrol listesi](docs/WINDOWS-TEST.md) akşam tamamlanacaktır.

## Ekran görüntüleri

**İki gerçek ekran görüntüsü Windows aşamasında eklenecek; bu teslim hazırlığında PNG dosyaları yoktur.** Sahte görüntü kullanılmadı. Beklenen dosyalar:

1. `docs/screenshots/01-oyun-skor.png` — oyun sürerken kuş, iki engel çifti ve en az 1 puan.
2. `docs/screenshots/02-oyun-bitti.png` — çarpışma sonrası oyun bitti bilgisi, skor ve Yeni Oyun düğmesi.

Çekim ve README'ye ekleme adımları [ekran görüntüsü yönergesinde](docs/screenshots/README.md). İki gerçek PNG eklenene kadar föyün ekran görüntüsü şartı tamamlanmış sayılmaz.

## Kod düzeni ve sınırlamalar

`src/FlappyBird`: Form, çizim kontrolü ve giriş noktası. `src/FlappyBird.Core`: Bird, PipePair, GameSession. `tests/FlappyBird.Tests`: Konsol davranış testleri. [Sözlü sınav için kod rehberi](KOD-REHBERI.md), [geliştirme kaydı](docs/GELISTIRME.md).

Çarpışma hesabı kuşun dikdörtgen sınırlarını kullanır; çizilen kuş elipstir. En yüksek skor dosyaya yazılmaz. Fizik sabit dünya koordinatlarında işler; uzun bir arayüz takılmasında zaman adımı 250 ms ile sınırlanır. Windows'ta DPI, odak, tuş olayları ve çizimin akıcılığı kontrol edilmelidir. GitHub uzak deposu henüz oluşturulmadı; öğrenci bilgileri doldurulacak.
