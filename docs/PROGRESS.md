# YMH219 yürütme kaydı

Plan: docs/superpowers/plans/2026-11-03-odevler.md
Kullanıcı: Gerekli işleri yap, Windows kısmını akşam yapacağız; devam.
Pre-flight: Beş proje arasında paylaşılan arayüz yok.
Karar: Sorular yerine föyü ve native Windows Forms varsayımlarını uygula; kullanıcı devam edilmesini açıkça istedi.
Karar: Mevcut Git deposu olmayan boş çalışma alanında her proje için ayrı yerel Git deposu oluştur; ortak worktree yok.
Karar: Gerçek ekran görüntülerini, öğrenci kimliğini ve gönüllü yüz örneklerini Windows aşamasına bırak. Hazır veya sahte teslim kanıtı üretme.
SDK: ~/.dotnet altında .NET SDK 10.0.403 kuruldu; global sistem ayarı değiştirilmedi.

Task 1: complete — FlappyBird 13/13, KeyEventLogger 10/10, iki Release çözümünde 0 uyarı/hata; gerçek yerel commitler ve belgeler.
Task 2: complete — ChatApp 12/12 gerçek TCP testi, Release 0 uyarı/hata; okunmayan sunucuda gönderim zaman aşımı regresyonu RED→GREEN.
Task 3: complete — SpeechApp 17/17 çekirdek testi; Release 0 uyarı/hata; gerçek dil/ses altyapısı Windows'ta kontrol edilecek.
Task 4: complete — FaceRecognition 15/15 çekirdek testi; Release 0 uyarı/hata; native OpenCV smoke izole macOS ortamında geçti.
Task 5: complete — Windows başlatıcı parser/Check/test/mock açılış doğrulandı; beş self-contained Windows x64 publish başarılı; kaynak ZIP ve EXE ZIP CRC bütünlük kontrolleri geçti.
Final review: Bağımsız reviewer beş kaynak deposunda doğrulanmış Critical/Important bulgu bulmadı. 67/67 testi ve beş Release derlemeyi yeniden çalıştırdı.
Final: minor fixed — Face README eğitim düğmesi davranışı açıklaması düzeltildi (belge değişikliği).
Final: minor fixed — Windows menüsü K SDK yanında kaynak yollarını da denetliyor; gerçek PowerShell kontrolü geçti.
Final: Ruling — SDK denetiminde .NET 10 SDK aranması korundu; sadece SDK11 kurulu olması .NET10 hedef çalışma zamanının varlığını garanti etmez. Hazır EXE'ler için SDK gerekmiyor.
Windows UI/hardware ve screenshot gate: Kullanıcı tarafından akşama ertelendi; başarı iddiası yapılmadı.

## Windows aşaması — 3 Kasım 2026
Beş Release derleme: 0 uyarı / 0 hata. Davranış testleri: 70/70. OpenCV, LBPH native çağrıları, SAPI ses üretimi/tanıma, kamera kare alma ve mikrofon girişi geçti. Gerçek kişi eğitimi ve mikrofon diktesi tamamlanmadı. Öğrenci bilgileri dolduruldu. Ayrıntılar: [Windows doğrulaması](WINDOWS-DOGRULAMA.md). Arayüz denetim erişimi ve GitHub kimlik doğrulaması bekliyor; beş ayrı depo kullanıcıya bırakıldı.
