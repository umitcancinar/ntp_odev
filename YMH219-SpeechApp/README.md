# YMH219 · Speech-to-Text / Text-to-Speech

Windows Forms üzerinde metni sesli okutan ve mikrofon konuşmasını metne dönüştüren, bağımsız C# masaüstü uygulaması. Nesne Tabanlı Programlama (YMH219) ek uygulama ödevidir.

- **Öğrenci adı-soyadı:** Ümitcan Çinar
- **Öğrenci numarası:** 250541009
- **Teslim:** Kendi hesabınızda `YMH219-SpeechApp` adlı ayrı public GitHub deposu.

## Teknolojiler

- C# / .NET 10, Windows Forms (`net10.0-windows`).
- [System.Speech 10.0.14](https://www.nuget.org/packages/System.Speech/10.0.14): Windows SAPI ses sentezi ve konuşma tanıma.
- `SpeechApp.Core`: platform bağımsız metin, dil tercihi ve oturum kuralları.
- `SpeechApp.Tests`: ek test framework'ü gerektirmeyen .NET 10 konsol test çalıştırıcısı; başarısız testte çıkış kodu 1.
- Yerel Git geçmişi; diğer ödevlere veya üst klasöre proje bağımlılığı yoktur.

## Kurulum ve çalıştırma

1. Windows 10/11 bilgisayara .NET 10 SDK yükleyin. Visual Studio kullanıyorsanız .NET 10 destekli sürüm ve **.NET masaüstü geliştirme** iş yükünü seçin.
2. Ses çıkışını ve varsayılan mikrofonu Windows Ses ayarlarından seçin. Mikrofonun masaüstü uygulamalarına erişimine izin verin.
3. Gerekli konuşma sesini ve desteklenen tanıma dilini Windows dil/konuşma ayarlarından yükleyin. Değişiklikten sonra uygulamayı yeniden başlatın.
4. Depo klasöründe PowerShell açın:

```powershell
dotnet restore YMH219-SpeechApp.sln
dotnet build YMH219-SpeechApp.sln -c Release
dotnet run --project tests/SpeechApp.Tests -c Release
dotnet run --project src/SpeechApp -c Release
```

Visual Studio'da `YMH219-SpeechApp.sln` dosyasını açıp `SpeechApp` projesini başlangıç projesi yapabilirsiniz. İlk geri yükleme NuGet için internet ister. Uygulama kodu herhangi bir ağ servisine ses veya metin göndermez; tanıma kurulu Windows SAPI motoruyla yürütülür.

## Kullanım ve özellikler

**Metinden sese:** İlk alana bir metin yazın, kurulu sesi ve hızı (`-10…10`, normal `0`) seçin. Başlat ile okutun; Duraklat ve Devam et ile yönetin. Durdur iptal eder. Metni temizle, etkin okumayı da iptal eder ve giriş alanını boşaltır. Boş/yalnız boşluk metin okutulmaz; üst sınır 20.000 karakterdir.

**Sesten metne:** Listeden gerçek bir kurulu tanıyıcı seçin. Dinlemeyi başlat ile varsayılan mikrofon açılır. Seçilen tanıyıcının dilinde konuşun; tamamlanan sonuçlar alttaki metne eklenir. Dinlemeyi durdur mikrofon işlemini iptal eder. Sonuçları temizle yalnızca sonuç alanını boşaltır; dinleme sürüyorsa yeni sözler eklenir. Son sonuç saati ve motorun güven değeri gösterilir. Sonuçları Ctrl+A / Ctrl+C ile kopyalayabilirsiniz; otomatik dosya kaydı yoktur.

**Durum ve hata yönetimi:** Mikrofonun açık/kapalı veya kapatılıyor oluşu ve okuma durumu metinle gösterilir. TTS ve STT aynı anda çalıştırılamaz; hoparlör sesinin mikrofona geri beslenmesi bu şekilde önlenir. Durdurma asenkrondur; tamamlanma olayı gelene kadar yeni işlem düğmeleri kapalı kalır. Mikrofon otomatik açılmaz. Form kapanınca olaylar ayrılır, bekleyen işlemler iptal edilir ve konuşma nesneleri temizlenir.

## Dil ve cihaz sınırlamaları

- Arayüz Türkçedir; **bu, Türkçe dikte desteği olduğu anlamına gelmez**. `InstalledRecognizers()` yalnızca bu bilgisayardaki uyumlu SAPI tanıyıcılarını listeler. Türkçe tanıyıcı görünmüyorsa Türkçe dikte yapılamaz; varmış gibi sahte bir seçenek eklenmez.
- Windows'un modern **Win+H sesle yazma**, canlı altyazı veya çevrim içi konuşma özellikleriyle System.Speech aynı altyapı değildir. Bir dilin bu özelliklerde bulunması, SAPI dikte tanıyıcısının da kurulu olduğunu kanıtlamaz. Yalnızca Windows görüntüleme dilini değiştirmek yeterli değildir.
- Uygulama kullanılabilir tanıyıcılar içinden önce arayüz dilini, sonra aynı dilin lehçesini, sonra ilk kurulu seçeneği tercih eder. Seçili seçeneğin dil kodu her zaman görünür. Tanıyıcı yoksa okuma kullanılabilir durumda kalabilir; mikrofon başlatma kapalıdır.
- Windows ses ayarlarındaki her modern/OneCore ses SAPI üzerinden görünmeyebilir. Sadece `GetInstalledVoices()` ile bulunan etkin sesler listelenir.
- Bazı motorlar genel dikte gramerini desteklemeyebilir; uygulama bu hatayı gösterir. Ses paketi/motor kurulumu haricî bir Windows hazırlık adımıdır; uygulama paket indirmez.
- Varsayılan ses/mikrofon kullanılır; uygulama içinden cihaz seçilmez. Cihazı değiştirirken etkin işlemi durdurup yeniden başlatın. Tanıma doğruluğu dil, ses kalitesi ve ortam gürültüsüne bağlıdır. Güven değeri kesin doğruluk yüzdesi değildir.
- Sonuç alanı yaklaşık 100.000 karaktere ulaştığında sonraki sonuçtan önce dinleme durdurulur. Metni kopyalayıp temizleyerek devam edin.
- macOS/Linux'ta Windows Forms ve SAPI çalışmaz. `EnableWindowsTargeting` yalnızca çapraz derlemeye izin verir.

## Doğrulama durumu

3 Kasım 2026 Windows x64 doğrulaması: .NET SDK 10.0.403 ile Release derlemesi **0 uyarı / 0 hata**, davranış testleri **17/17 başarılı**. SAPI ses üretimi ve üretilmiş İngilizce WAV girdisinden tanıma başarılı; varsayılan mikrofon girişi açıldı. Türkçe Tolga okuma sesi ve en-US dikte tanıyıcısı bulundu. Gerçek mikrofondan dikte ve düğmelerin etkileşimli kontrolü bekliyor.

## Ekran görüntüleri — Windows çekimi bekleniyor

Föyün iki ekran görüntüsü koşulu, gerçek Windows testinden sonra tamamlanacaktır. Şu anda depoda gerçek PNG yoktur; sahte görüntü ve kırık görüntü bağlantısı kullanılmamıştır.

| Dosya | Çekilecek gerçek senaryo | Durum |
| --- | --- | --- |
| `docs/screenshots/01-metinden-sese.png` | Metin, seçili ses/hız ve okuma veya duraklatma durumunu gösteren form | Windows çekimi bekliyor |
| `docs/screenshots/02-sesten-metne.png` | Tanıyıcının gerçek dil kodu, mikrofondan oluşmuş metin ve etkin dinleme durumunu gösteren form | Windows çekimi bekliyor |

Çekim ve README'ye gerçek görselleri ekleme adımları: [Ekran görüntüsü yönergesi](docs/screenshots/README.md). Donanım/dil sorunu varsa gerçek hata görüntüsünü ayrıca saklayın; başarılı dikte senaryosu yerine geçirmiş gibi sunmayın.

## Kod düzeni ve açıklama

```text
YMH219-SpeechApp.sln
src/SpeechApp/                 Windows formu ve System.Speech kaynakları
src/SpeechApp.Core/            Metin, tanıyıcı tercihi ve durum geçişleri
tests/SpeechApp.Tests/         Donanımdan bağımsız davranış testleri
docs/KOD-REHBERI.md            Sınıf tasarımı, olay akışı, sözlü kontrol hazırlığı
docs/WINDOWS-KONTROL.md        Gerçek cihaz testleri ve teslim adımları
```

[Öğrenci kod rehberi](docs/KOD-REHBERI.md), metotların gerekçelerini ve küçük değişiklik denemelerini açıklar. Commitler gerçek geliştirme aşamalarını gösterir; geçmiş tarih veya sahte ekran görüntüsü üretilmemiştir.

## Resmî başvuru kaynakları

- [SpeechSynthesizer](https://learn.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer): ses seçimi, hız, pause/resume ve Dispose.
- [SpeechRecognitionEngine](https://learn.microsoft.com/en-us/dotnet/api/system.speech.recognition.speechrecognitionengine): kurulu tanıyıcılar, dikte, asenkron iptal ve kaynak yönetimi.
- [SpeakAsyncCancelAll](https://learn.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer.speakasynccancelall): iptalden sonra da tamamlanma olayının gelmesi.
