# YMH219-NotepadApp · Not Defteri Uygulaması (Serileştirme & Dosya İşlemleri)

Windows Forms üzerinde JSON serileştirme (serialization) ve dosya işlemleri (File I/O) odaklı, notları kalıcı olarak saklayan ve TXT olarak dışa aktarabilen C# masaüstü uygulaması. Nesne Tabanlı Programlama (YMH219) ek uygulama ödevidir.

- **Öğrenci Adı-Soyadı:** Ümitcan Çinar
- **Öğrenci Numarası:** 250541009
- **Ders:** Nesne Tabanlı Programlama (YMH219)
- **Depo Adı:** `YMH219-NotepadApp`
- **Son Teslim Tarihi:** 29 Kasım 2026

---

## 1. Projenin Amacı ve Özellikleri

Uygulamanın temel amacı, nesne tabanlı programlama prensipleriyle kullanıcı notlarını yönetmek, bunları bilgisayarda JSON biçiminde kalıcı olarak saklamak (`serialization`) ve ihtiyaç duyulduğunda geri yüklemektir (`deserialization`).

### Temel Yetenekler:
- **Eksiksiz CRUD Yönetimi:** Not ekleme, listeleme, düzenleme ve silme işlemleri.
- **JSON Serileştirme:** Notlar `System.Text.Json` kullanılarak yapılandırılmış JSON dosyasına otomatik kaydedilir ve uygulama açılışında geri yüklenir.
- **TXT Dışa Aktarma:** Tekil bir notu veya tüm notları tarih, kategori ve istatistik üst bilgileriyle biçimlendirilmiş düz metin (`.txt`) olarak kaydetme.
- **Anlık Arama & Filtreleme:** Başlık, içerik veya kategoriye göre (Genel, Ders, Kişisel, İş, Önemli) anlık süzme.
- **Otomatik Kaydetme & Canlı Sayaç:** Not düzenlenirken arka planda çalışan zamanlayıcı ile otomatik kayıt ve canlı kelime/karakter sayacı.
- **Hata Yönetimi:** Dosya bulunamaması veya bozuk JSON içeriği gibi durumlarda uygulamanın çökmesini engelleyen güvenli mimari.

---

## 2. Kullanılan Teknolojiler

- **Dil & Çalışma Zamanı:** C# / .NET 10 (`net10.0-windows` ve `net10.0`).
- **Arayüz Çatısı:** Windows Forms (WinForms).
- **Serileştirme:** `System.Text.Json` (UTF-8, Türkçe karakter destekli `JavaScriptEncoder`).
- **Dosya Giriş/Çıkış:** `System.IO` (`File`, `StreamWriter`, `Path`).
- **Mimari:** Katmanlı tasarım (`NotepadApp.Core`, `NotepadApp` ve `NotepadApp.Tests`). Harici üçüncü parti bağımlılık yoktur.

---

## 3. Kurulum ve Çalıştırma

### Gereksinimler:
- Windows 10/11 x64
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) veya Visual Studio 2026 (.NET masaüstü geliştirme iş yükü)

### Terminal ile Çalıştırma:
```powershell
# Depoyu klonlayın ve klasöre girin
git clone https://github.com/umitcancinar/YMH219-NotepadApp.git
cd YMH219-NotepadApp

# Bağımlılıkları geri yükleyin ve derleyin
dotnet restore NotepadApp.sln
dotnet build NotepadApp.sln -c Release

# Uygulamayı başlatın
dotnet run --project src/NotepadApp -c Release
```

### Visual Studio ile Çalıştırma:
1. `NotepadApp.sln` dosyasını Visual Studio ile açın.
2. Çözüm Gezgini'nden `NotepadApp` projesine sağ tıklayıp **Başlangıç Projesi Olarak Ayarla** seçin.
3. **F5** veya **Ctrl + F5** tuşlarına basarak projeyi çalıştırın.

---

## 4. Testlerin Çalıştırılması

Proje iş mantığı, dosya I/O ve serileştirme dayanıklılığı 15 kapsamlı birim testiyle doğrulanmıştır:

```powershell
dotnet run --project tests/NotepadApp.Tests -c Release
```

Çıktı özeti: `15/15 test geçti. (0 başarısız)`

---

## 5. Uygulama Ekran Görüntüleri

*Not: Windows ortamında uygulamayı çalıştırdıktan sonra alınan ekran görüntüleri aşağıdaki alana eklenecektir:*

![Not listesi ve düzenleme](docs/screenshots/01-not-listesi-ve-duzenleme.png)
![Arama ve TXT dışa aktarma](docs/screenshots/02-arama-ve-disa-aktarma.png)

*(Ekran görüntüsü çekim rehberi için [docs/screenshots/README.md](docs/screenshots/README.md) dosyasına bakınız.)*

---

## 6. Bilinen Sınırlamalar ve Geliştirme Notları

- Veritabanı olarak yerel JSON dosyası `%APPDATA%\YMH219-NotepadApp\notlar.json` yolu tercih edilmiştir; böylece kullanıcı izin sorunu yaşamadan notlarına kalıcı erişebilir.
- Çok büyük metin dosyalarında (100.000+ satır) arama gecikmelerini önlemek adına LINQ sorgusu optimize edilmiştir.
- Sözlü değerlendirme hazırlığı ve sınıf sorumlulukları detayları için [KOD-REHBERI.md](KOD-REHBERI.md) belgesini inceleyebilirsiniz.
