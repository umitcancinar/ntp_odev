# Windows'ta kalan kontroller

3 Kasım 2026 tarihinde SDK kurulumu, beş Release derleme, 70 davranış testi ve OpenCV/SAPI bileşen kontrolleri tamamlandı. Beş self-contained x64 EXE, `windows-apps` klasöründe hazır. Ayrıntılar: [Windows doğrulaması](docs/WINDOWS-DOGRULAMA.md).

## Uygulamaları açma

`UYGULAMALARI-AC.cmd` dosyasını açıp uygulama seçin. Hazır EXE varsa SDK aranmadan açılır. Menüde `K` SDK kontrolü, `B` derleme, `T` davranış testleri, `N` OpenCV/SAPI kontrolüdür. Yönetici yetkisi gerekmez.

## Etkileşimli denemeler

1. **Flappy Bird:** Boşluk ve yukarı okla tekrarlı zıplama, basılı tutma, engelden geçince skor, çarpışma, yeniden başlatma, P ile duraklatma ve odak kaybını deneyin.
2. **Key Event Logger:** Başlat ile yalnız bu formda deneme metni yazın; Durdur, Temizle ve UTF-8 TXT kaydını deneyin. Başka pencereye geçince kaydın durduğunu doğrulayın.
3. **Chat:** Menü `3` iki pencere açar. İlk pencerede 5000 portunda sunucu başlatın. İki istemciyi farklı adlarla `127.0.0.1:5000` adresine bağlayın. İki yönlü Türkçe mesaj, saatler, istemci listesi, ayrılma ve yeniden bağlanmayı deneyin.
4. **Speech:** Türkçe metni Microsoft Tolga sesiyle okutun; duraklat/devam/durdur işlemlerini deneyin. Bu bilgisayarda dikte tanıyıcısı **en-US**; mikrofon denemesini İngilizce yapın. Türkçe okuma sesi, Türkçe dikte tanıyıcısı anlamına gelmez.
5. **FaceRecognition:** Kendi yüzünüz için aynı etikete farklı ışık/açıyla en az üç, tercihen beş örnek ekleyin; **Yalnızca kendi yüzümle çalışıyorum** seçeneğiyle eğitin. Sonra eğitimde kullanılmayan bir karede deneyin. İki gönüllüyle çalışırken bu kutuyu kapatıp her iki etikete en az üçer örnek ekleyin. Yüz fotoğrafları yalnız yerelde kalmalı.

Her projenin `docs` klasöründe ayrıntılı Windows kontrol listesi bulunur. Pencereyi küçültme, DPI, klavye odağı ve işlem sırasında kapatmayı da kontrol edin.

## Ekran görüntüleri ve teslim

Her uygulamadan iki gerçek PNG alın; dosya adları `docs/screenshots/README.md` içinde yazılı. Gerçek dosyaları ekledikten sonra README'ye görsel olarak yerleştirin. Yüz fotoğraflarını public repoya koymayın; kişisel görüntü içermeyen arayüz örnekleri kullanın ve başarıyla yapılmamış bir denemeyi yapılmış gibi göstermeyin.

Öğrenci bilgileri beş README'de dolduruldu: **Ümitcan Çinar — 250541009**. Beş ayrı public teslim deposuna aktarımı kullanıcı yapacak. Commit geçmişini ve gerçek tarihleri koruyun. Classroom'a GitHub profil bağlantısı teslim edilir; son tarih **30 Kasım 2026, 23:59 (Türkiye saati)**.
