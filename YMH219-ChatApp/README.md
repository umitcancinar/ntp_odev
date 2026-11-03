# YMH219 — İstemci–Sunucu Chat Uygulaması

Windows Forms üzerinde TCP ile kullanıcı adları ve zaman damgalı mesajlar paylaşan ders projesi. Bir uygulamada **Sunucu** ve **İstemci** sekmeleri vardır; birden fazla uygulama örneği aynı odaya bağlanabilir.

**Öğrenci adı-soyadı:** [DOLDURULACAK]  
**Öğrenci numarası:** [DOLDURULACAK]  
**Ders:** Nesne Tabanlı Programlama — YMH219

## Teknolojiler ve yapı

- C# / .NET 10, Windows Forms (`net10.0-windows`).
- `TcpListener`, `TcpClient`, `NetworkStream`, `async/await`, `CancellationToken`, `SemaphoreSlim`.
- UTF-8 JSON satır protokolü; `System.Text.Json`. Harici NuGet paketi yoktur.
- `src/ChatApp.Core`: platformdan bağımsız ağ ve doğrulama sınıfları.
- `src/ChatApp`: Türkçe Windows arayüzü.
- `tests/ChatApp.Tests`: gerçek localhost TCP bağlantılarıyla çalışan bağımlılıksız test koşucusu.

## Windows kurulumu ve çalıştırma

Windows 10/11 üzerinde .NET 10 SDK kurulu olmalıdır. Visual Studio kullanılıyorsa .NET 10 destekleyen sürümü ve **.NET masaüstü geliştirme** iş yükünü yükleyin. `ChatApp.sln` çözümünü açıp `ChatApp` projesini başlangıç projesi seçin. Komut satırı alternatifi, depo kökünde:

```powershell
dotnet --version
dotnet restore ChatApp.sln
dotnet build ChatApp.sln -c Release
dotnet run --project src/ChatApp/ChatApp.csproj -c Release
```

İkinci PowerShell penceresinde aynı klasörden son komutu tekrar çalıştırın. Derlenmiş programı iki kez açmak da mümkündür:

```powershell
.\src\ChatApp\bin\Release\net10.0-windows\ChatApp.exe
```

### Aynı bilgisayarda iki kişiyle deneme

1. İlk pencerede **Sunucu** sekmesinden port `5000` ile **Sunucuyu başlat** düğmesine basın. Yerel ağ kutusu kapalı kalsın.
2. İlk pencerenin **İstemci** sekmesinde adres `127.0.0.1`, port `5000`, kullanıcı `Çağrı` olsun; **Bağlan** düğmesine basın.
3. İkinci pencerede **İstemci** sekmesinden aynı adres/porta `İpek` adıyla bağlanın. İkinci sunucuyu başlatmanız gerekmez.
4. Her iki yönde `Merhaba, çığ öşü 🎉` gibi bir mesaj gönderin. **Gönder** veya **Ctrl+Enter** kullanılabilir. Enter mesaj içinde yeni satır oluşturur.
5. İlk pencerenin **Sunucu** sekmesinde iki kullanıcıyı ve adreslerini görün. İkinci istemcide **Bağlantıyı kes** düğmesine basın; listedeki sayının azaldığını kontrol edin.
6. **Sunucuyu durdur** istemcileri ayırır. Sunucuyu tekrar başlatıp **Bağlan** ile yeniden katılabilirsiniz.

### Yerel ağda deneme ve güvenlik duvarı

Sunucuyu başlatmadan önce **Yerel ağdan bağlantılara izin ver** kutusunu açın. Bu durumda tüm IPv4 arayüzleri dinlenir. Diğer bilgisayarda sunucunun yerel IPv4 adresini (`ipconfig` çıktısı) yazın; `127.0.0.1` yalnızca o bilgisayarı belirtir. Aynı yerel ağ ve aynı port kullanılmalıdır.

Windows Güvenlik Duvarı sorarsa yalnızca güvendiğiniz **özel ağ** için izin verin. Bir kural gerekiyorsa uygulamanın TCP portuna (varsayılan 5000) gelen bağlantıyı özel ağda izinli yapın. Aynı bilgisayardaki varsayılan localhost denemesi için yönlendiricide port açılması gerekmez. İnternete port yönlendirmesi yapmayın. Port başka programca kullanılıyorsa farklı bir port seçin; uygulama hatayı durum satırında gösterir.

## Özellikler

- Kullanıcı adıyla katılma; 1–32 karakter, boş ve kontrol karakterleri reddedilir.
- Sunucunun atadığı zaman damgasıyla gönderene ve diğer istemcilere mesaj yayını.
- Türkçe ve emoji desteği; 1–2000 karakter mesaj sınırı.
- Sunucuda kullanıcı adı ve uzak adres/port listesi; katılım/ayrılma olayları.
- İstemcide odadaki kullanıcıların görünmesi; bağlantı hatası sonrası yeniden bağlanma.
- Asenkron okuma/yazma, her bağlantıda sıralı yazma, en fazla 16.384 bayt protokol satırı.
- Katılımda 10 saniye, istemci bağlantısında 6 saniye, istemci/sunucu mesaj yazmasında 3 saniye zaman aşımı.
- Sunucu dururken yarım protokol verisi gönderen bağlantıların da kapatılması.
- Segoe UI, DPI ölçekleme, yeniden boyutlanabilen form, klavye erişimi ve metinsel durumlar.

## Test ve doğrulama

```powershell
dotnet run --project tests/ChatApp.Tests/ChatApp.Tests.csproj -c Release
```

Test koşucusu başarısızlıkta sıfırdan farklı çıkış kodu döndürür. 12 senaryo; iki gerçek istemci, Türkçe/emoji, zaman damgası, liste ve ayrılma, sunucu yeniden başlatma, reddedilen bağlantı, girdi doğrulama, bozuk/aşırı uzun protokol, eşzamanlı mesajlar, yarım satırda kapanış ve yoğun akışta katılmayı ve okumayan sunucuda gönderme zaman aşımını kapsar. Portlar işletim sisteminden dinamik alınır.

macOS üzerinde çekirdek testleri çalıştırılmış ve Windows hedefli Release derlemesi yapılmıştır. **Windows arayüzü henüz çalıştırılmadı; bu derleme çalışan Windows ekranı kanıtı değildir.** Gerçek çalışma ve ekran görüntüsü kontrolleri [Windows kontrol listesinde](docs/WINDOWS-KONTROL.md) beklemektedir. [Doğrulama kaydı](docs/DOGRULAMA.md) uygulanan kontrolleri belirtir.

## Ekran görüntüleri — Windows aşamasında eklenecek

Henüz gerçek ekran görüntüsü yoktur. Windows'ta aşağıdaki iki senaryo çekilecek; gerçek PNG dosyaları eklendikten sonra README içine görüntü olarak yerleştirilecektir. Eksik dosyalara görüntü bağlantısı eklenmemiştir.

| Dosya | Gösterilecek durum |
| --- | --- |
| `docs/screenshots/01-sunucu-istemciler.png` | Sunucu açık; iki farklı kullanıcı ve adres/port bilgileri listede, katılım/mesaj olayları görünür. |
| `docs/screenshots/02-iki-istemci-sohbet.png` | İki uygulama penceresi yan yana; farklı kullanıcı adlarıyla iki yönlü Türkçe mesajlar ve zamanları görünür. |

[Ayrıntılı çekim yönergesi](docs/screenshots/README.md).

## Sınırlamalar ve geliştirme notları

Bu eğitim uygulaması güvenilir localhost/yerel ağ içindir. TCP içeriği şifrelenmez; kullanıcı adı kimlik doğrulaması değildir. Aynı kullanıcı adları kullanılabilir; sunucu listesinde adres/port ayırt eder. Özel mesaj, kalıcı geçmiş ve çevrimdışı teslim yoktur. Arayüzde son yaklaşık 1000 olay tutulur; eski satırlar gruplar hâlinde silinir. Çok kullanıcılı üretim yükü hedeflenmemiştir; kullanıcı listesi de protokol satır sınırına tabidir.

Örnekler birbirinden bağımsızdır: ilk penceredeki sunucuyu kapatmak o sunucuya bağlı herkesin bağlantısını keser. Tekrar bağlanma kullanıcı tarafından **Bağlan** ile yapılır; otomatik yeniden gönderim yoktur. Uygulama Windows gerektirir; macOS üzerinde Windows Forms çalıştırılamaz.

[Kod ve sözlü kontrol rehberi](KOD-REHBERI.md). GitHub yayını, öğrenci bilgileri ve iki gerçek ekran görüntüsü teslimden önce tamamlanmalıdır.
