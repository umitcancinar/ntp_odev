# Hafta 5 Evim uygulama planı

Kaynak: Kullanıcının 4 Kasım 2026'da verdiği Hafta 5 laboratuvar föyü. Önceki haftaların dizin, öğrenci bilgisi ve sade C# sınıf düzeni örnek alındı. Kullanıcı geliştirme ve ilgili GitHub reposuna göndermeyi yetkilendirdi.

## Tasarım
Föy konsol uygulaması istediğinden .NET 10 konsol projesi kullanılır; Windows Forms gerekmez. Kisi.cs kişiye ait private alanları, doğrulamalı property'leri, iki yapıcı metodu, yaş/bilgi metotlarını ve ortak sayaç/adresi tutar. Ev.cs private kişi listesi üzerinde ekleme, yazdırma, ortalama ve en yaşlı bulma işlerini yapar. Program.cs dört temsili kişiyi oluşturup istenen bütün sonuçları Türkçe biçimde yazar.

Doğum yılı 1900'den büyük ve mevcut yıldan küçük/eşit olmalıdır. Hatalı atamada açıklayıcı istisna kullanılır; önceki geçerli değer korunur ve başarısız kurucu sayacı artırmaz. Boş ev ortalaması sıfırdır; boş evde EnYasli istisna verir. Eşit yaşta ilk eklenen döner. Boş/null/boşluk meslek Belirtilmedi olur. Tek ad alan hazırlık kurucusu mevcut yılı ve Belirtilmedi mesleğini varsayar. Adres gerçek kişisel veri değil temsili sabittir.

## Uygulama ve doğrulama
- [x] Önce gerçek davranış testlerini ve API iskeletini yaz; eksik davranışlarla kırmızı çalıştırmayı gör.
- [x] Kisi, Ev ve dört kişilik Main'i tamamla; tüm testleri ve Release derlemeyi geçir.
- [x] Gerçek konsol çıktısını kaydet; önceki hafta üslubuyla kaynak kod ve açıklamalı Word/PDF raporu oluştur.
- [x] Word'ü paketli renderer ile PDF/PNG'ye çevir; bütün sayfaları görsel incele.
- [x] Kaynak föyü, README, testler ve teslim ZIP'ini ekle. Hafta 5 klasörünü yerel lessons/ntp/hafta5 dizinine de kopyala.
- [x] Yalnız ilgili dosyaları commit edip mevcut main üzerine force kullanmadan gönder; uzak commit ve dosyaları doğrula.

Doğrulama kaydı: 20 davranış testi önce eksik API ile 0/20, uygulama sonrası 20/20 geçti. Release çözümü 0 uyarı ve 0 hata ile derlendi. Bağımsız kod incelemesinde önemli bulgu yok. Word ve PDF beş sayfa; paketli LibreOffice renderer ile tüm PNG sayfaları görsel kontrol edildi. Gönderim kullanıcı tarafından yetkilendirildi; uzak commit doğrulaması paketleme sonrasında yapılacak.
