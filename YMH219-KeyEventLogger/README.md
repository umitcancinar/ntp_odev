# YMH219-KeyEventLogger

Kendi Windows Forms penceresindeki klavye olaylarını zaman damgasıyla gösteren eğitsel uygulama. Nesne Tabanlı Programlama (YMH 219) ödevi.

**Öğrenci:** Ümitcan Çinar
**Öğrenci numarası:** 250541009

## Teknolojiler

C#, .NET 10, Windows Forms; `KeyPreview`, `KeyDown`, `KeyUp`, `KeyPress`; `File.WriteAllLines` ve UTF-8. Harici NuGet paketi yoktur. Platformdan bağımsız `KeyEventLogger.Core` açık kayıt oturumunu ve dışa aktarımı yönetir. Başka ödev deposuna bağımlılık yoktur.

## Kurulum ve çalıştırma

Windows 10/11 ve .NET 10 SDK gerekir. Visual Studio için .NET 10 destekleyen sürüm ve **.NET masaüstü geliştirme** iş yükünü kullanın. Depo kökünde PowerShell açın:

```powershell
dotnet build KeyEventLogger.sln -c Release
dotnet run --project src/KeyEventLogger -c Release
```

Visual Studio'da `KeyEventLogger.sln` açılıp `KeyEventLogger` başlangıç projesi seçilebilir. İlk derleme Windows Desktop hedefleme paketini indirebilir.

Windows dağıtım klasörü için:

```powershell
dotnet publish src/KeyEventLogger -c Release -r win-x64 --self-contained false -o publish
```

Bu dağıtım .NET 10 Desktop Runtime gerektirir. macOS'ta alınan `bin/` dosyalarını doğrudan taşımak yerine Windows'ta derleyin veya Windows hedefli publish kullanın.

## Özellikler ve kullanım

1. **Başlat** ile kaydı açın. Deneme alanına `Merhaba dünya, İğüş 123` gibi kişisel bilgi içermeyen bir metin yazın.
2. KeyDown tuşu/modifier bilgisini; KeyPress üretilen karakteri; KeyUp bırakılan tuşu gösterir. Her olayda tarih, saat, milisaniye ve UTC farkı vardır. Olay sayısı fiziksel tuş sayısından farklıdır.
3. **Durdur** yeni olayları engeller; **Temizle** olay listesini siler. Temizle mevcut açık/durmuş durumunu korur; deneme alanını değiştirmez.
4. **Dosyaya Kaydet** kaydı durdurur ve kullanıcının seçtiği `.txt` dosyasına UTF-8 yazar. İptalde kayıt durdurulmuş kalır. Başlat ile tekrar açılabilir. Dosya hatasında bellekteki olaylar korunur.
5. Pencere odağı kaybolunca kayıt durur. Geri gelince açıkça Başlat gerekir. 5000 olayda otomatik durur; kaydedip temizledikten sonra yeniden başlayabilirsiniz.

## Form sınırı ve gizlilik

Uygulama yalnızca kendi formunun normal olaylarını kullanır. Global klavye hook'u, gizli/arka plan toplama, otomatik başlangıç veya ağ iletişimi yoktur. Parola alanı eklenirse parola gizleme özelliği açık TextBox / MaskedTextBox olayları da reddedilir. Kendi formundaki düğmelere ve salt okunur olay listesine odaklanıldığında oluşan yerel tuş olayları da kapsam içindedir.

Kayıtlar kendiliğinden diske yazılmaz; yalnızca kullanıcı SaveFileDialog üzerinden seçerse kaydedilir. Uygulama kapanınca bellekteki kayıtlar kaybolur. Deneme metnine parola veya kişisel bilgi yazmayın. Dışa aktarımları depo dışında tutun; varsayılan `klavye-olaylari-*.txt` adı `.gitignore` içinde ayrıca dışlanır.

## Test ve mevcut doğrulama

```powershell
dotnet run --project tests/KeyEventLogger.Tests -c Release
```

Çerçevesiz konsol çalıştırıcısı **10 davranış testi** içerir: durdurulmuş kayıt, üç olay türü, başlat/durdur/temizle, parola alanı engeli, Türkçe UTF-8 dışa aktarımı, gerçek dosya hatası, kontrol karakterlerinin tek satır gösterimi, kayıt sınırı ve boş olay bilgisi.

3 Kasım 2026 Windows x64 doğrulaması: .NET SDK 10.0.403 ile Release derlemesi **0 uyarı / 0 hata**, davranış testleri **10/10 başarılı**. Pencere açılışı yapıldı; klavye, odak, DPI ve ekran görüntüsü senaryolarının etkileşimli kontrolü bekliyor.

## Ekran görüntüleri

**İki gerçek Windows PNG'si henüz eklenmedi.** Beklenen dosyalar:

1. `docs/screenshots/01-kayit-acik.png` — Başlat sonrası deneme metni, zaman damgalı üç olay türü ve KAYIT AÇIK durumu.
2. `docs/screenshots/02-kayit-durdu-kaydedildi.png` — durdurulmuş durum, dolu olay listesi ve UTF-8 kaydetme başarı bilgisi.

[Ekran görüntüsü yönergesi](docs/screenshots/README.md) çekimi ve README'ye eklemeyi açıklar. PNG'ler eklenene kadar föyün ekran görüntüsü şartı tamamlanmış sayılmaz. Sahte ekran görüntüsü kullanılmadı.

## Kod düzeni ve sınırlamalar

`src/KeyEventLogger`: form ve giriş noktası. `src/KeyEventLogger.Core`: `LogSession`, `KeyLogEntry`, olay türü. `tests/KeyEventLogger.Tests`: gerçek davranış testleri. [Sözlü kod rehberi](KOD-REHBERI.md), [geliştirme kaydı](docs/GELISTIRME.md).

Bazı gezinme/kısayol tuşları odaktaki Windows kontrolü tarafından işlenebilir; KeyPress her fiziksel tuş için üretilmez. Uzun basış tekrarlanan KeyDown/KeyPress olayları üretir; bunlar bu ödevde olay olarak gösterilir. IME ve birleşik karakterlerin olay dizisi klavye düzenine göre değişebilir. Windows DPI, sekme sırası ve dosya izinleri kontrolü bekliyor. Beş ayrı teslim deposuna aktarım daha sonra yapılacak.
