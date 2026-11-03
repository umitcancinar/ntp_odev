# Sözlü kontrol için kod rehberi

| Sınıf / enum | Sorumluluğu |
|---|---|
| `Program` | STA iş parçacığında Windows Forms'u başlatır. |
| `MainForm` | Butonları, formun klavye olaylarını, odağı ve dosya iletişim kutusunu yönetir. |
| `LogSession` | Kayıt açık/kapalı durumunu, kayıt sınırını, listeyi ve UTF-8 dışa aktarımı yönetir. |
| `KeyLogEntry` | Zaman damgası, olay türü ve tuş/karakter bilgisini değişmez bir record olarak tutar. |
| `KeyEventKind` | KeyDown, KeyUp ve KeyPress değerlerini tür güvenli biçimde ayırır. |

## Olay akışını anlatma

Formda `KeyPreview = true` olduğu için yerel klavye olayları odaktaki alt kontrolden önce formda değerlendirilebilir. `KeyDown` fiziksel tuş ve Ctrl/Shift gibi modifier bilgilerini; `KeyPress` yazılabilir karakteri; `KeyUp` bırakmayı gösterir. Örneğin A basışında KeyDown, KeyPress ve KeyUp görülebilir; Shift veya yön tuşunun KeyPress üretmesi beklenmez. Uzun basışta işletim sistemi KeyDown/KeyPress tekrarı üretir.

Form bu üç olayı `RecordEvent` metoduna yönlendirir. Pencerenin odaklı/aktif olduğu kontrol edilir; parola kontrolü varsa olay reddedilir. `LogSession.Record` kayıt açıksa zaman damgasıyla giriş oluşturur. `RichTextBox` yalnızca kabul edilen girişi gösterir. `DateTimeOffset.Now` zaman dilimi farkını da korur. Satır sonu ve sekme karakterleri `\r`, `\n`, `\t` olarak görünür; tek olay tek satırda kalır.

## Oturum durumu

`Start` sınıra ulaşılmamışsa kaydı açar, `Stop` kapatır. `Clear` yalnızca listeyi siler; kayıt durumunu değiştirmez. Liste 5000 olaya ulaşınca `Record` kendisi durdurur. `IReadOnlyList` görünümü dışarıdan listeye eklemeyi engeller. Tuş dinleme yalnızca formda vardır; çekirdek klavye donanımına veya işletim sistemi hook'una erişmez.

Pencere başka uygulama/iletişim kutusu yüzünden deaktive olunca kayıt kapatılır. Dönünce kullanıcı Başlat'a basar. Böylece sessiz devam etme olmaz. `PasswordFocused`, aktif kontrol zincirinde gizli TextBox / MaskedTextBox tespit eder; aynı kural çekirdeğe bool olarak iletilir ve test edilir.

## Dosya işlemleri

`SaveLog` önce kaydı durdurur. `SaveFileDialog` dosyayı kullanıcıya seçtirir; iptal bir hata değildir. `LogSession.Save` kayıtları UTF-8 ile yazar. UI `IOException`, erişim ve geçersiz yol hatalarını yakalar; hata mesajı verir ve listeyi temizlemez. Kaydetme başarılı olmadan başarı bildirimi gösterilmez.

Kayıtlar yalnızca bellekte tutulur; Save dışında otomatik dosya, internet, başlangıç görevi veya arka plan servisi yoktur. Bu uygulama sistem keyloggerı değildir; föyün istediği form içi olay gösterimidir.

## Küçük değişiklik alıştırmaları

- Olay sayısını fiziksel tuş sayısı diye adlandırmak neden yanlış? Üç olayın aynı basıştan gelebileceğini gösterin.
- Zaman biçimini değiştirmek için `KeyLogEntry.ToString` incelenir. Kültür farkından etkilenmemek için sabit format kullanılır.
- Maksimum olayı değiştirmek için `LogSession.MaxEntries` kullanılır; sınır testinin hangi hatayı yakaladığını anlatın.
- Kaydetme sırasında dizin yolu verilirse konsol testinin gerçek hatayı nasıl yakaladığını gösterin.
- Temizle sonrası kayıt açık kalıyorsa hangi sınıfın kuralı olduğunu anlatın.

## Test / Windows ayrımı

10 çekirdek testi macOS'ta çalıştırıldı; Windows hedefli çözüm derlendi. Bu, native KeyPreview ve odak davranışının Windows'ta denenmiş olduğu anlamına gelmez. `docs/WINDOWS-TEST.md` ve iki gerçek ekran görüntüsü hâlâ tamamlanacaktır.
