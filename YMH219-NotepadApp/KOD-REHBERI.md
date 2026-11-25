# Sözlü Kontrol ve Kod Açıklama Rehberi

Bu rehber, **Nesne Tabanlı Programlama (YMH219)** dersi kapsamında Not Defteri uygulamasının sözlü kontrolünde sorulabilecek sorulara ve mimari detaylara hızlıca hazırlanabilmeniz için hazırlanmıştır.

---

## 1. Sınıf ve Katman Sorumlulukları

| Sınıf / Bileşen | Katman | Temel Görevi | Önemli Alan / Metotlar |
|---|---|---|---|
| `Not` | `Core/Models` | Tek bir notun verilerini tutan temel model | `Id`, `Baslik`, `Icerik`, `Kategori`, `OlusturmaTarihi`, `DuzenlemeTarihi`, `KelimeSayisi`, `Guncelle()` |
| `Kategori` | `Core/Models` | Standart kategori sabitleri ve filtre listeleri | `Genel`, `Ders`, `Kisisel`, `Is`, `Onemli`, `VarsayilanKategoriler` |
| `NotDepolamaServisi` | `Core/Services` | JSON serileştirme ve TXT dışa aktarma statik servisi | `KaydetJson()`, `YukleJson()`, `DisaAktarTxt()`, `TumunuDisaAktarTxt()` |
| `NotDefteriYonetici` | `Core/Services` | Koleksiyon yönetimi, CRUD ve arama/filtreleme mantığı | `NotEkle()`, `NotGuncelle()`, `NotSil()`, `Filtrele()`, `OtomatikKaydet()` |
| `MainForm` | `NotepadApp (UI)` | Windows Forms kullanıcı arayüzü ve olay yönetimi | `BilesenleriOlustur()`, `OlaylariBagla()`, `ListeyiYenile()`, `SeciliNotuTxtAktar()` |

---

## 2. Nesne Tabanlı Programlama ve Mimari Detaylar

1. **Katmanlı Mimari (Separation of Concerns):**  
   İş mantığı (`NotepadApp.Core`) ile arayüz (`NotepadApp`) birbirinden tamamen ayrılmıştır. `Core` katmanı Windows Forms bağımlılığı taşımaz, bu sayede birim testleri işletim sisteminden bağımsız olarak çalışabilir.
2. **Kapsülleme (Encapsulation):**  
   Not koleksiyonu `NotDefteriYonetici` içerisinde `private List<Not>` olarak saklanır ve dışarıya salt okunur `IReadOnlyList<Not>` olarak açılır. Not düzenleme işlemleri rastgele değil, `Guncelle()` metodu üzerinden doğrulamalı olarak yürütülür.
3. **Serileştirme (JSON Serialization):**  
   `System.Text.Json` kullanılarak nesne grafiği JSON metnine dönüştürülür. `JavaScriptEncoder.Create(UnicodeRanges.All)` kullanılarak Türkçe karakterlerin (`ğ, ü, ş, ı, ö, ç`) düzgün UTF-8 saklanması garanti edilir.
4. **Hata ve İstisna Yönetimi (Defensive Programming):**  
   `YukleJson()` metodu dosya bulunamadığında veya dosya bozulduğunda (`JsonException`) uygulamanın çökmesini engeller; güvenli bir şekilde boş liste döndürerek kullanıcı deneyimini korur.

---

## 3. Olası Sözlü / Hoca Kontrol Soruları ve Yanıtları

* **Soru: Notlar kapatılıp açılınca nasıl korunuyor?**  
  *Cevap:* Notlar her değişiklik yapıldığında veya form kapanırken `NotDepolamaServisi.KaydetJson()` metoduyla `%APPDATA%\YMH219-NotepadApp\notlar.json` dosyasına serileştirilir. Uygulama açılışında `OtomatikYukle()` çağrılarak liste JSON'dan geri yüklenir.
* **Soru: Arama mekanizması nasıl çalışıyor?**  
  *Cevap:* `NotDefteriYonetici.Filtrele()` metodu LINQ kullanarak başlıkta veya içerikte `StringComparison.OrdinalIgnoreCase` ile arama yapar. Aynı zamanda seçilen kategoriye göre eşzamanlı süzme uygular ve sonuçları en son düzenlenen not en üstte olacak şekilde sıralar.
* **Soru: Bir notun kelime ve karakter sayısını nasıl hesaplıyorsun?**  
  *Cevap:* `Not` sınıfındaki `KelimeSayisi` özelliği, içeriği boşluk ve yeni satır karakterlerine göre (`StringSplitOptions.RemoveEmptyEntries`) bölerek dizi uzunluğunu döndürür. `KarakterSayisi` ise doğrudan `Icerik.Length` değerini verir.

---

## 4. Küçük Kod Değişikliği Pratikleri

- **Yeni bir kategori eklemek için:** `Kategori.cs` içine yeni bir sabit (örneğin `public const string Proje = "Proje";`) ekleyip `VarsayilanKategoriler` dizisine dahil edin.
- **Otomatik kaydetme süresini değiştirmek için:** `MainForm.cs` içindeki `_otoKayitTimer.Interval` değerini (varsayılan 1200 ms) ayarlayın.
- **Varsayılan dışa aktarma formatını değiştirmek için:** `NotDepolamaServisi.DisaAktarTxt()` metodundaki üst bilgi şablonunu düzenleyin.
