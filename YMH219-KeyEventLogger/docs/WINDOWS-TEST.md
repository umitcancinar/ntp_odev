# Windows çalıştırma kontrol listesi

Durum: **Henüz Windows'ta uygulanmadı.** Sonuçları gerçek bilgisayarda gözlemledikten sonra işaretleyin.

- [ ] .NET 10 SDK mevcut; `dotnet build KeyEventLogger.sln -c Release` hatasız.
- [ ] `dotnet run --project tests/KeyEventLogger.Tests -c Release` 10/10 başarılı.
- [ ] `dotnet run --project src/KeyEventLogger -c Release` açılır; ilk durum DURDURULDU; deneme alanına yazmak olay listesine eklenmez.
- [ ] Başlat ile KAYIT AÇIK görünür; `Merhaba dünya, İğüş 123` yazıldığında saat, olay ve tuş/karakter görünür.
- [ ] Bir harf için KeyDown/KeyPress/KeyUp; Shift/Ctrl gibi tuşlar için beklenen KeyDown/KeyUp görülür. Uzun basış tekrarları gözlenir.
- [ ] Durdur sonrası yeni tuş olayları listeye eklenmez; Başlat önceki olayları korur.
- [ ] Alt+Tab ile başka uygulamaya geçiş kaydı durdurur. Başka uygulamada yazılan metin bu listeye gelmez; geri dönmek kendiliğinden başlatmaz.
- [ ] Temizle listeyi ve sayacı sıfırlar; açık/durmuş durumu korur; deneme metni aynı kalır.
- [ ] Dosyaya Kaydet kaydı durdurur; iptal edildiğinde veri korunur ve kayıt durdurulmuş kalır.
- [ ] Kullanıcı Belgeler klasörüne `.txt` kaydeder; Not Defteri'nde Türkçe karakterler, olay türü ve zaman damgası doğru görünür.
- [ ] Yazılamayan bir konuma kaydetme denemesi uygulamayı kapatmaz; hata görünür ve liste korunur. Dosya seçicinin kendi hata mesajı çıkarsa bu da kaydedilir.
- [ ] 5000 olay sınırı otomatik durdurur; Başlat kapalıdır; Temizle sonrasında yeniden kullanılabilir.
- [ ] Tab/Shift+Tab ve Alt+B, Alt+D, Alt+T kısayolları çalışır. Ekran okuyucu deneme alanı ve olay listesinin adlarını okuyabilir.
- [ ] En küçük boyut ile %100 / %150 / %200 DPI'da düğme ve metinler erişilebilir; yüksek karşıtlıkta metinler okunur.
- [ ] Pencereyi açık oturumda kapatmak hata vermez; yeniden açma boş/durmuş oturumla başlar.
- [ ] İki gerçek PNG çekilip README'ye eklendi; kişisel bilgi görünmüyor.

Parola alanı bu UI'da bulunmaz. Gelecekte eklenirse `UseSystemPasswordChar` / `PasswordChar` açıkken olayların loga girmediğini ayrıca kontrol edin; çekirdek engeli otomatik testte sınanır.

Kontrol eden: __________  Tarih: __________  Windows / DPI: __________
