# Hafta 5 Evim

**Ümitcan Çınar — 250541009**

Hafta 5 laboratuvar föyündeki sınıf, property, yapıcı metot ve statik üye uygulaması. Lab teslimindeki dört görev ve gelecek haftaya hazırlık için ikinci yapıcı metot tamamlandı. Bu, önceki beş Windows Forms ek ödevinden ayrı bir **.NET 10 konsol uygulamasıdır**; Windows, macOS veya Linux üzerinde çalışır.

## Föydeki görevler

| İstenen | Uygulama |
|---|---|
| Doğrulamalı meslek property'si | `Kisi.Meslek`: boş, null veya yalnız boşluk değeri `Belirtilmedi` olur. |
| En yaşlı üyeyi döndürme | `Ev.EnYasli()`: listedeki gerçek `Kisi` nesnesini döndürür. |
| Ortak sabit adres | `Kisi.EvAdresi`: `const string`; `Main` içinde yazdırılır. |
| En az dört kişi ve bütün sonuçlar | `Program.cs`: dört kişi, liste, ortalama, en yaşlı, sayaç ve adres. |
| Yapıcı metot aşırı yüklemesi | `Kisi(string ad)` üç parametreli kurucuya `this(...)` ile yönlenir. |

## Dosyalar

- `Kisi.cs`: private doğum yılı/meslek, property doğrulaması, iki kurucu, `YasHesapla`, `BilgiYaz`, statik sayaç ve sabit adres.
- `Ev.cs`: private `List<Kisi>`, üye ekleme, listeleme, yaş ortalaması, en yaşlı kişi.
- `Program.cs`: dört kişilik örnek evin kurulması. Kişiler ve adres **temsili örnek veridir**.
- `Konsol_Ciktisi.txt`: 4 Kasım 2026 tarihinde program çalıştırılarak alınan gerçek çıktı.
- [Word raporu](teslim/Hafta5_NTP_Odev.docx) ve [PDF raporu](teslim/Hafta5_NTP_Odev.pdf): tam kaynak kodlar, açıklamalar, gerçek çıktı, testler ve çalıştırma adımları.
- [Özgün föy](docs/Hafta5_Foy.pdf).

## Çalıştırma

.NET 10 SDK kurulu terminalde bu klasöre geçin:

```powershell
dotnet build Hafta5_Evim.sln -c Release
dotnet run --project Hafta5_Evim.csproj -c Release
```

Visual Studio 2026'da `Hafta5_Evim.sln` açılır; `Hafta5_Evim` başlangıç projesi seçilir ve Ctrl+F5 yapılır. Ek NuGet paketi veya cihaz gerekmez. Ana program test projesinden ayrı tutulmuştur.

## Gerçek örnek çıktı

```text
--- EV ÜYELERİ ---
Anne - Öğretmen - 56 yaşında
Baba - Mühendis - 58 yaşında
Ümitcan - Öğrenci - 22 yaşında
Kardeş - Belirtilmedi - 18 yaşında

Ortalama yaş : 38,5
En yaşlı üye : Baba - Mühendis - 58 yaşında
Toplam kişi : 4
Ev adresi : Örnek Mahallesi Öğrenci Sokak No 5 Elazığ
```

Yaşlar sistem yılından hesaplandığından ilerleyen yıllarda değişir. `(56 + 58 + 22 + 18) / 4 = 38,5`. Türkçe kültür ayarı ondalık ayırıcıyı virgül olarak gösterir. Boş bırakılan meslek, kurucudan property setter'ına geçerken `Belirtilmedi` olur.

## Doğrulama ve sınır durumları

```powershell
dotnet run --project tests/Hafta5_Evim.Tests -c Release
```

Konsol test çalıştırıcısında **20/20** kontrol geçti; Release çözümü **0 uyarı, 0 hata** ile derlendi. `dotnet test` yerine yukarıdaki komut kullanılır. Testler meslek doğrulamasını, doğum yılı sınırlarını, sayacı, kurucu overload'unu, ortalamayı, en yaşlı seçimini ve gerçek listeleme çıktısını sınar.

Doğum yılı kuralı föyle aynıdır: `1900 < yıl <= mevcut yıl`. Föydeki örnek geçersiz değer için uyarı yazarken bu uygulama `ArgumentOutOfRangeException` fırlatır; böylece hatalı bir kişi oluşturulamaz, geçerli yıl bozulmaz ve başarısız kurucu sayacı artırmaz. Ana program geçerli değerlerle çalışır. Boş evin ortalaması `0`, en yaşlı sorgusu açıklayıcı istisnadır; aynı yaşta ilk eklenen üye döner. Null üye eklenemez.

`Kisi.KisiSayisi`, süreç boyunca başarıyla oluşturulan kişi nesnelerini sayar; evdeki üye sayısı değildir. Ana programda dört kişi oluşturulup dördü de aynı eve eklenir. Tek ad alan hazırlık kurucusu doğum yılı için mevcut yılı ve meslek için `Belirtilmedi` değerini kullanır; `Main` içindeki dört kişide üç parametreli kurucu kullanıldığı için çıktıdaki sayaç 4 kalır.

## Kodu açıklarken

`get` private alanı okur, `set` gelen `value` değerini doğrular. Kurucu dönüş tipi taşımaz ve nesne oluşturulurken bir kez çalışır. Normal metotlar `kisi.YasHesapla()` biçiminde nesneden; ortak sayaç ve sabit adres `Kisi.KisiSayisi` / `Kisi.EvAdresi` biçiminde sınıftan kullanılır. Ortalama hesabında `double` dönüşümü tamsayı bölmesini önler. `EnYasli` listeyi tarar ve bir nesne döndürür; o nesnenin bilgisi `BilgiYaz` ile yazdırılır.
