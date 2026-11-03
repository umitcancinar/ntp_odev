# Windows doğrulama ve teslim listesi

**Durum:** Bu listedeki gerçek cihaz denemeleri henüz yapılmadı. İşaretleri Windows'ta sonucu görerek doldurun. Derleme ve çekirdek test sonucu cihaz testi yerine geçmez.

## Hazırlık

- [ ] `dotnet --info`: .NET 10 SDK görüldü.
- [ ] Depo tek başına başka klasöre kopyalandığında çözüm açıldı.
- [ ] `dotnet build YMH219-SpeechApp.sln -c Release`: 0 hata.
- [ ] `dotnet run --project tests/SpeechApp.Tests -c Release`: 17/17 başarılı.
- [ ] `dotnet run --project src/SpeechApp -c Release`: form açıldı; mikrofon başlangıçta kapalı.
- [ ] Windows Ses ayarında çalışan varsayılan çıkış ve mikrofon seçildi.
- [ ] Windows mikrofon gizlilik ayarında mikrofon ve masaüstü uygulaması erişimi açık.
- [ ] Gereken SAPI tanıyıcı ve okuma sesi listede gerçek dil koduyla görünüyor. Kurulum yapıldıysa uygulama yeniden başlatıldı.

## Metinden sese

- [ ] Boş ve yalnız boşluk metinde Başlat kapalı; uygulama çökmedi.
- [ ] Seçili sesin diline uygun iki cümle yazılıp Başlat ile duyuldu.
- [ ] Uzun metin okunurken Duraklat sesi kesti; Devam et aynı işleme devam etti.
- [ ] Aktif ve duraklatılmış okumada Durdur denendi; durum yeniden hazır oldu.
- [ ] Aktif ve duraklatılmış okumada Metni temizle denendi; metin silindi, okuma durdu.
- [ ] Durduktan sonra aynı metin yeniden okutulabildi.
- [ ] En az iki farklı hız denenip ses hızının değiştiği duyuldu.
- [ ] Okuma boyunca Dinlemeyi başlat kapalıydı.
- [ ] Varsayılan ses çıkışı yokken anlaşılır hata görüldü; form açık kaldı.

## Sesten metne

- [ ] Uygulama açılır açılmaz mikrofonu kullanmadı.
- [ ] Gerçek kurulu bir tanıyıcı seçildi; **dil kodu not edildi:** `________`.
- [ ] Dinlemeyi başlat ile “Mikrofon AÇIK” durumu görüldü.
- [ ] Seçili dilde konuşuldu; gerçek cümle metin alanına geldi; zaman ve motor güveni gösterildi.
- [ ] Sonuçları temizle eski metni sildi; dinleme sürerken sonraki cümle eklendi.
- [ ] Dinlemeyi durdur sonrası mikrofon kapalı durumuna dönüldü; yeni sonuç eklenmedi.
- [ ] Durdur/yeniden başlat en az üç kez yapıldı; ikinci açılışta hata veya çift sonuç olmadı.
- [ ] Dinlerken metin okutma kapalıydı; durdurma bitince tekrar açıldı.
- [ ] Mikrofon kapalı/bağlı değilken başlatma denenip hata görüldü; uygulama çökmedi.
- [ ] Mikrofon masaüstü izni kapatıldığında anlaşılır hata görüldü; izin geri açılınca yeniden denendi.
- [ ] Tanıyıcı yoksa dikte başlatma kapalı ve kurulum mesajı görünür; okuma bağımsız çalışıyor.
- [ ] Türkçe tanıyıcı yoksa Türkçe dikte desteği varmış gibi sunulmadı; desteklenen dil kullanıldı.

## Arayüz ve kaynaklar

- [ ] Form küçültülüp büyütüldü; metin alanları büyüdü, düğmeler erişilebilir kaldı.
- [ ] %125 / %150 Windows ölçeğinde yazılar ve düğmeler kırpılmadı.
- [ ] Tab ve Shift+Tab ile bütün etkin kontrollere erişildi; odak görünür.
- [ ] Okuma, duraklatma ve dinleme sırasında ayrı ayrı form kapatıldı; arka planda ses/mikrofon kalmadı.
- [ ] Form kapanırken geç gelen olaylar hata penceresi veya cross-thread exception üretmedi.
- [ ] Yeni açılışta mikrofon kapalı; eski metin/kayıt kendiliğinden yüklenmedi.

## Gerçek ekran görüntüsü ve teslim

- [ ] `docs/screenshots/01-metinden-sese.png` gerçek Windows formundan çekildi.
- [ ] `docs/screenshots/02-sesten-metne.png` gerçek dikte sonucu içeriyor.
- [ ] Görseller kişisel veri içermiyor; README'ye iki gerçek görsel eklendi.
- [ ] README öğrenci adı-soyadı ve numarası dolduruldu.
- [ ] Test tarihi / Windows sürümü / kullanılan tanıyıcı ve ses bu belgenin altına yazıldı.
- [ ] `git status` incelendi; bin/obj/.vs eklenmedi. Gerçek görsel ve belge değişiklikleri commit edildi.
- [ ] Kullanıcının kendi hesabında ayrı public GitHub deposuna gönderildi; Classroom'a profil bağlantısı eklendi.

## Test kaydı

- Tarih: [BEKLİYOR]
- Windows sürümü: [BEKLİYOR]
- Ses çıkışı ve mikrofon: [BEKLİYOR]
- Okuma sesi / dil kodu: [BEKLİYOR]
- Dikte tanıyıcısı / dil kodu: [BEKLİYOR]
- Başarısız adım ve gerçek hata mesajı: [VARSA YAZIN]
