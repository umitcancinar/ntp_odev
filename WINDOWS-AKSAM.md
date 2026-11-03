# Akşam Windows'ta tamamlanacaklar

## 1. Dosyaları taşıma
`ntp5-kaynaklar.zip` dosyasını Windows'a taşıyıp çıkartın. Varsa `ntp5-windows-x64.zip` içeriğini de aynı klasöre çıkartın; `windows-apps` klasörü `Windows.ps1` ile yan yana olmalı. Hazır EXE paketi olmadan da kaynaklar .NET 10 SDK ile çalışır.

**Windows 10/11 x64** kullanın. Kaynak kod geliştirme için [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) gerekir. İsterseniz terminalde `winget install --id Microsoft.DotNet.SDK.10 --exact` kullanın, ardından terminali yeniden açın. IDE zorunlu değildir; Visual Studio için .NET 10 destekleyen Visual Studio 2026 + .NET masaüstü geliştirme gerekir. OpenCV yükleme hatasında [VC++ x64 Redistributable](https://learn.microsoft.com/cpp/windows/latest-supported-vc-redist) kurun.

## 2. Derleme ve testler
`UYGULAMALARI-AC.cmd` açın. `K` SDK kontrolü, `B` bütün kaynakların Release derlemesi, `T` davranış testleri içindir. Hata olursa terminal metnini saklayın. Bu adım mikrofon/kamera/arayüz testinin yerine geçmez.

## 3. Beş uygulamayı deneme
1. **Flappy Bird:** Başlat, klavyeyle zıpla, boruyu geç ve skor artışını kontrol et. Boru/zemin çarpışmasında Game Over; yeniden başlatınca skor/durum sıfırlanmalı. Başka pencereye geçiş/pause ve Windows ölçeklemesini dene.
2. **Key Event Logger:** Başlat ve yalnız uygulamanın giriş alanına yaz. Başka uygulamaya geçince kayıt durmalı. Durdur, temizle, Türkçe metinle TXT kaydet ve dosyayı kontrol et.
3. **Chat:** Menü `3` iki pencere açar. İlk pencerede Sunucu sekmesinden 5000 portunda başlat. İki pencerede İstemci sekmesinden farklı adlarla `127.0.0.1:5000` adresine bağlan. Çift yönlü Türkçe mesaj, saatler, sunucu istemci listesi, bağlantıyı kesme ve yeniden bağlanmayı kontrol et. Yerel deneme için ağdan erişim kutusunu açmak gerekmez.
4. **Speech:** Metin yaz, gerçekten kurulu sesi seç, okut/duraklat/devam/durdur. Kurulu tanıyıcının dilinde mikrofona konuş; metne dönüşümü kontrol et. Windows gizlilik ayarlarından mikrofon ve masaüstü uygulama erişimine izin ver. Türkçe SAPI tanıyıcısı yoksa varmış gibi gösterilmez; kurulu desteklenen dili kullan. Windows `Win+H` diktesinin çalışması System.Speech motorunun kurulu olduğunu tek başına kanıtlamaz.
5. **FaceRecognition:** Önce dosyadan yüz algıla, ardından kamerayı dene. İzinli iki etikette en az üçer, tercihen beşer farklı örnek ekle, eğit ve eğitimde olmayan görüntüde tahmin/uzaklık göster. Örnekler yalnız `%LOCALAPPDATA%` altında kalır. README'deki ayrıntılı veri/etiket yönergesini izle.

Her depoda ek manuel kontrol listesi vardır. Kamera/mikrofon yoksa bu işlevler kontrol edilmiş sayılmaz; dosyadan yüz algılama ve sesli okuma gibi erişilebilir bölümler ayrıca denenebilir.

## 4. İki ekran görüntüsü / depo
Her repo `docs/screenshots/README.md` içindeki iki senaryoyu gerçek uygulamada göster. `Win+Shift+S` ile PNG al, belirtilen dosya adlarıyla kaydet ve README'deki görsel kodlarını etkinleştir. Yüzler/isimler paylaşılacaksa izin ve kişisel veri kontrolü yap. Toplam en az **10 gerçek görüntü** gerekir.

## 5. Kimlik, kod açıklaması ve GitHub
- Her README'deki öğrenci adı/numarası alanlarını doldur.
- Kendi deponun `KOD-REHBERI.md` veya `docs/KOD-REHBERI.md` dosyasını okuyup akışı açıkla. Bir küçük değişiklik yapıp denemek sözlü kontrol için yararlı olur.
- Beş ayrı **public**, boş GitHub repo aç: klasör adlarıyla aynı isimleri kullan; GitHub'da otomatik README/.gitignore ekleme.
- Release içindeki kaynak ZIP'i kullanıyorsan her klasörde ayrı `.git` geçmişi vardır; ilgili remote'u ekleyip commitleri push et. Bu toplu repoyu klonladıysan geçmiş üst repodadır; ayrı depolara aktarımı birlikte yapacağız. Kaynak ZIP içindeki `.git` klasörlerini silme.
- Windows düzeltmeleri ve gerçek görüntüleri anlamlı yeni commitlerle ekle. Geçmişi geriye tarihlemeyin.
- Gizli sekmede beş reponun erişilebilir ve README görsellerinin görünür olduğunu kontrol et.
- Classroom'a GitHub **profil bağlantısını** teslim et; tarih: **30 Kasım 2026 23:59**.

Toplu çalışma deposu: https://github.com/umitcancinar/ntp_odev . Beş ayrı teslim deposu henüz oluşturulmadı; Windows kontrolünden sonra birlikte aktaracağız.
