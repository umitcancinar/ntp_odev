# Kod ve sözlü kontrol rehberi

## Sınıflar hangi sorumluluğu taşıyor?

- `ChatServer`: `TcpListener` sahibi; katılımları kabul eder, her istemci için `ServeAsync` çalıştırır, mesajı herkese yollar ve bağlantıları kapatır.
- `ChatClient`: tek istemci bağlantısının sahibi; bağlanır, sunucudan katılım onayı alır, gönderir ve okuma döngüsünü yönetir.
- `JsonLineConnection`: ağ akışını JSON paketlerine dönüştürür; satır uzunluğunu sınırlarken parçalı TCP okumalarını birleştirir. Her bağlantının kendi yazma kilidi vardır.
- `ChatPacket` / `ClientInfo`: ağda taşınan veri modelleri (`record`).
- `ChatRules`: kullanıcı adı ve mesaj doğrulaması. Hem istemci hem sunucu uygular; ham ağ istemcisi yerel arayüzü atlayabilir.
- `MainForm`: Türkçe kontroller, kullanıcı olayları ve ağ olaylarını ekrana aktarır. Ağ iş mantığı bu formun içinde değildir.

## Bir mesajın yolculuğu

1. Kullanıcı `Gönder` düğmesine basar veya Ctrl+Enter yapar.
2. `MainForm.SendMessageAsync` → `ChatClient.SendAsync`; metin doğrulanır.
3. `JsonLineConnection.WriteAsync`, JSON + satır sonunu UTF-8 olarak TCP akışına yazar. `SemaphoreSlim` aynı bağlantıya eşzamanlı yazıları sıralar.
4. Sunucuda `ServeAsync` tam satırı okur, paketin türünü ve mesajını kontrol eder. Kullanıcı adını gelen `message` paketinden almaz; katılımda doğrulanan adı kullanır. Zamanı sunucu belirler.
5. `BroadcastAsync`, tüm katılmış istemcilere aynı paketi gönderir. Gönderene de dönmesi, mesajın sunucuya ulaştığını gösterir.
6. İstemcinin `ReceiveAsync` olayı tetikler. Form `BeginInvoke` ile UI iş parçacığına geçer ve zamanı yerel saatle `HH:mm:ss` olarak gösterir.

## Neden satır protokolü?

TCP bir bayt akışıdır; tek `WriteAsync`, karşı tarafta tek okumaya karşılık gelmez. Bir JSON paketini bir satır olarak gönderiyoruz. `JsonLineConnection` 4096 baytlık tamponu bir sonraki satıra da taşır; gelen satırı en çok 16.384 bayta kadar biriktirir. JSON metnindeki yeni satırlar kaçış karakteri olarak kodlandığı için gerçek protokol satırını bölmez. Hatalı UTF-8, bozuk JSON, bilinmeyen paket veya sınır aşımı ilgili bağlantıyı kapatır.

Örnek katılım paketi (özellik adları büyük harfle başlar):

```json
{"Type":"join","User":"Çağrı"}
```

İstemciden giden mesaj:

```json
{"Type":"message","Text":"Merhaba"}
```

Sunucu `welcome`, `system`, `message` ve `clients` paketlerini üretir. Zaman damgası UTC `DateTimeOffset` olarak taşınır; ekranda yerel saate çevrilir. Kullanıcı listesinde kullanıcı adı ve IP:port bulunur. `leave` paketi veya TCP kapanışı ayrılma sayılır; uygulamanın kes düğmesi TCP'yi kapatır.

## async/await ve kaynak temizliği

`await`, ağ beklenirken form iş parçacığını engellemez. `Task.Run` ile her ağa bir iş parçacığı ayırmak gerekmez. Sunucunun kabul görevi ve oturum görevleri saklanır; `StopAsync` bunları bekler. Tamamlanan başarılı oturumlar listeden temizlenir. Beklenen ağ/protokol hataları yakalanır; görünmeyen, istisnası atılmış görevler bırakılmaz.

`CancellationTokenSource` durdurma isteğini taşır. Sunucu durunca token iptal edilir, dinleyici durur, soketler kapatılır ve döngüler beklenir. Eksik JSON satırı gönderen istemci de kapanır. İstemcinin `DisconnectAsync` metodu okuma görevini bekler ve kaynakları bırakır. `IAsyncDisposable` testlerde `await using` kullanımını sağlar.

Bağlan/kes ve sunucu başlat/durdur yaşam döngüsü işlemleri çağıran tarafından sırayla yapılmalıdır. Formdaki ayrı `clientGate` ve `serverGate` kilitleri bunu sağlar; mesaj yazıları ayrıca bağlantının kendi kilidinde sıralanır. Form kapanışı devam eden kullanıcı işlemini bekler, sonra iki servisi temizler. Ağ olaylarının UI güncellemesi `BeginInvoke` ile yapılır.

## Nesne tabanlı programlama açısından

Kaynak sahipliği ve ağ akışının ayrıntıları özel alanlarla kapsüllenmiştir. Form, sunucunun iç istemci koleksiyonunu değiştiremez; `Clients` her seferinde salt okunur bir görüntü verir. Olaylar servisi formdan ayırır; çekirdek WinForms'a başvurmaz. Kalıtım zorlamak yerine her sınıfa tek bir sorumluluk verilmiştir. `ChatPacket` veri taşır, `ChatServer` davranışı yönetir.

## Sözlü kontrolde gösterilebilecek küçük değişiklikler

- Varsayılan portu değiştirme: `MainForm.Port` içindeki `Value`; iki pencere aynı port kullanmalı.
- Kullanıcı adı uzunluğu: `ChatRules.User` ve formdaki `username.MaxLength` birlikte değişmeli.
- Mesaj sınırı: `ChatRules.Message`, `message.MaxLength`, sayaç ve belgeler birlikte değişmeli; ağ satırı sınırıyla uyum korunmalı.
- Zaman gösterimine tarihi ekleme: `MainForm.Format` biçimini `dd.MM.yyyy HH:mm:ss` yapma.
- Yeni paket türü: protokol, sunucu doğrulaması ve istemci kabul listesi birlikte düşünülmeli; rastgele türü doğrudan kabul etmek doğru değildir.

Değişiklikten sonra `dotnet run --project tests/ChatApp.Tests -c Release` ve `dotnet build ChatApp.sln -c Release` çalıştırılır. Windows arayüzü ayrıca gerçek Windows üzerinde denenir.
