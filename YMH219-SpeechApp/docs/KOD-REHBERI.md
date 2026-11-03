# Kodu açıklama rehberi

Bu not uygulamayı ezberlemek yerine neden böyle tasarlandığını anlamak içindir. Dosyaları açıp küçük değişiklikler yaparak ve testleri yeniden çalıştırarak hazırlanın.

## Sınıflar ve sorumlulukları

| Sınıf | Sorumluluk |
| --- | --- |
| `Program` | STA iş parçacığında Windows Forms başlatır. |
| `MainForm` | Türkçe kontrolleri kurar, kullanıcı olaylarını servise iletir ve durumu gösterir. |
| `SpeechService` | Windows `SpeechSynthesizer` / `SpeechRecognitionEngine` nesnelerini ve ömürlerini yönetir; sonuç/durum olayları yayınlar. |
| `SpeechSession` | Tek bir ses etkinliği, duraklatma ve asenkron iptal geçişlerini yönetir. Windows'a bağımlı değildir. |
| `SpeechText` | Boş ve 20.000 karakter üzerindeki girdiyi reddeder, kenar boşluklarını temizler. |
| `RecognizerSelection` | Yalnızca gerçekten kurulu listeden dil tercihi yapar. |
| `VoiceOption`, `RecognizerOption`, `DictationResult` | UI'ya taşınan ses/tanıyıcı ve sonuç verileridir. |

Form, mikrofon veya COM nesnesini doğrudan kullanmaz. Servis de form kontrollerini bilmez. Böylece UI olayları ile kaynak yönetiminin görevleri ayrılır. `SpeechSession.Activity` dışarıdan keyfî değiştirilemez; geçiş metotlarından geçirilir (kapsülleme).

## Metin okuma akışı

1. Başlat düğmesi `SpeechService.Speak` çağırır.
2. Metin doğrulanır; gerçek ses ve hız seçilir, varsayılan ses çıkışı hazırlanır.
3. `TrySpeak` yalnızca boşta durumunda geçer; `SpeakAsync` işi başlatır ve UI donmaz.
4. Duraklat/Devam et `Pause`/`Resume` metotlarını ve durum modelini birlikte değiştirir.
5. Durdur bir iptal isteğidir. `StoppingSpeech` durumu bitiş olayına kadar başka iş başlamasını önler.
6. `SpeakCompleted` iptal edilmiş işte de gelebilir. İlgili prompt kimliği doğrulanır, sonra boşta durumuna geçilir.

**Neden hemen boşta yapmıyoruz?** Native motor henüz ses üretirken mikrofon açılabilir. Tamamlanmayı beklemek iki kaynağın çakışmasını engeller. Duraklatılmış okuma da hâlâ etkin bir iş sayılır.

## Dikte akışı

1. Başlangıçta `InstalledRecognizers()` sadece listeleme yapar; mikrofon açılmaz.
2. Kullanıcı gerçek tanıyıcının kimliğini seçer. `SpeechRecognitionEngine(option.Id)` o motoru oluşturur.
3. `DictationGrammar` genel dikte gramerini yükler. Her motorun bunu desteklediği varsayılmaz; hata gösterilir.
4. Kullanıcı başlatınca `SetInputToDefaultAudioDevice()` ve `RecognizeAsync(Multiple)` çağrılır.
5. `SpeechRecognized`, sonucun metnini ve motor güvenini getirir; form metne yeni satır ekler. Bu güven değeri doğruluk garantisi değildir.
6. Durdur, `RecognizeAsyncCancel()` kullanır. Bekleyen sonuçlar, durum artık Listening olmadığı için eklenmez.
7. `RecognizeCompleted` sonrası olaylar ayrılır ve motor `Dispose` edilir. Yeniden başlatma temiz bir motor oluşturur.

## Olaylar, UI iş parçacığı ve kapanış

Windows Forms kontrolleri oluşturan UI iş parçacığından güncellenmelidir. System.Speech geri çağrıları farklı iş parçacıklarından gelebilir. Servis kendisine verilen `Action<Action>` üzerinden formun `BeginInvoke` metoduna iş gönderir. Kuyruğa alınmış kod çalışmadan önce hem servis hem form kapanış/dispose durumunu tekrar kontrol eder. `Invoke` ile UI iş parçacığını beklemek yerine `BeginInvoke` kullanılması, kapanıştaki native kaynak temizliği sırasında karşılıklı bekleme riskini azaltır.

Form kapanırken önce kapanış bayrağı konur ve UI olay abonelikleri kaldırılır. Servis native olaylardan ayrılır, işlemleri iptal eder ve kaynaklarını temizler. Eski tanıyıcıdan gelen kuyruklanmış sonuç, sender kimliği mevcut motorla eşleşmediği için yeni oturuma eklenmez. Kapanış temizliğinde native hatalar best-effort olarak ele alınır; gerçek sürücü davranışı Windows listesinde ayrıca denenmelidir.

## Hata yönetimi

Boş metin bir giriş hatasıdır. Mikrofon izni, ses aygıtı, kurulu motor veya gramer desteği bir çalışma ortamı hatası olabilir. İstisnalar durum mesajına çevrilir; uygulama formu açık kalır. İptalin başarıyla tamamlandığı bilinmiyorsa servis okuma işini boşta ilan etmez; yeniden başlatma gerekir. Tanıyıcı yokluğunu uydurma Türkçe dil seçeneğiyle gizlemeyiz.

## Testlerin sınırı

17 konsol testi gerçek `SpeechText`, `RecognizerSelection` ve `SpeechSession` kodunu çalıştırır. Eksik kurallarla ilk RED çalıştırmada 15 test başarısız olmuş, kurallar uygulandıktan sonra 17/17 GREEN elde edilmiştir. İki sınır durumu başlangıçta da geçiyordu. Bu testler mikrofonu, hoparlörü, Windows Forms yerleşimini veya SAPI motorunun doğruluğunu test etmez. Windows kontrol listesi bu ayrı doğrulama içindir.

```powershell
dotnet run --project tests/SpeechApp.Tests -c Release
```

## Sözlü kontrolde açıklanabilecek sorular

- `SpeakAsync` ve eşzamanlı bir okuma arasındaki UI farkı nedir?
- `IDisposable` neden gerekli? Yalnızca değişkene `null` vermek mikrofonu serbest bırakır mı?
- Duraklatılmış okumada neden mikrofon başlatma kapalı kalır?
- Asenkron iptal sonrası neden tamamlanma olayı beklenir?
- `BeginInvoke` yapılmazsa cross-thread kontrol erişimi neden sorun olur?
- Türkçe arayüz neden Türkçe dikte desteğini kanıtlamaz?
- Bir olay aboneliği kaldırılmazsa eski motor sonuçları nasıl yeni oturuma sızabilir?

## Küçük değişiklik denemeleri

- Metin sınırını değiştirmek isterseniz önce sınır ve sınır+1 testlerini yeni beklenen davranışa göre düzenleyip RED görün; sonra üretim doğrulaması ile UI sayacını/MaxLength değerini birlikte değiştirin.
- Yeni dil tercihi gerektiğinde testte gerçek `RecognizerOption` listesi kullanın; kurulu olmayan bir motor oluşturan kural yazmayın.
- Sonuç formatına zaman damgasını da eklemek isterseniz `AppendResult` metodunun metin biçimini değiştirin; kopyalama davranışını Windows'ta kontrol edin.
- Durdurma sonrası hızlı yeniden başlatma senaryosunu değiştirirken `SpeechSession` testlerini çalıştırın; iptal tamamlanmadan mikrofonu açmayın.
