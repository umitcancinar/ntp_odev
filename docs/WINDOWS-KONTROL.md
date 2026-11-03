# Windows manuel doğrulama — henüz yapılmadı

Tarih, Windows/.NET sürümü ve sonuçları gerçek denemeden sonra doldurun. Bu dosya başarılı test tutanağı değildir.

- Deneme tarihi: [DOLDURULACAK]
- Windows ve .NET SDK sürümü: [DOLDURULACAK]
- Deneyen kişi: [DOLDURULACAK]

## Derleme ve başlangıç

- [ ] Depoyu yeni bir klasöre kopyalayıp `dotnet build ChatApp.sln -c Release` çalıştır; hatasız bitişi kaydet.
- [ ] `dotnet run --project tests/ChatApp.Tests -c Release`: 11/11 geçmeli, çıkış kodu 0 olmalı.
- [ ] Uygulamayı iki kez aç; Sunucu/İstemci sekmeleri, Türkçe metinler, başlangıçtaki kapalı/bağlı değil durumları doğru olsun.
- [ ] Formu minimum boyuta indir, büyüt; %100 / %150 / %200 ölçeklemede metin ve düğmeler kesilmesin. Sekme, Tab ve Alt kısayollarıyla odak görünür olsun.

## İşlevler

- [ ] İlk pencerede port 5000 ile sunucuyu aç; ikinci pencerede aynı portla sunucu başlatma denemesi anlaşılır hata vermeli ve uygulama açık kalmalı.
- [ ] İlk pencereden Çağrı, ikinciden İpek olarak 127.0.0.1:5000 adresine bağlan; sunucu listesinde iki kullanıcı ve adres/port görünmeli.
- [ ] İki yönde Türkçe/emoji içeren mesaj gönder; her iki istemcide de kullanıcı adı, mesaj ve saat görünmeli. Sunucu olayları da mesajları içermeli.
- [ ] Enter ile çok satır yaz, Ctrl+Enter ile gönder; boş/yalnızca boşluk mesajı reddedilsin. 2000 karakter sınırını ve sayacı kontrol et.
- [ ] Bir istemciyi kes; diğerinde ayrılma olayı ve güncellenen kullanıcı sayısı görünsün. Tekrar bağlan.
- [ ] Bir istemci penceresini kapat; sunucu listesi güncellensin. Sunucuyu durdur; diğer istemci bağlı değil durumuna dönsün. Yeniden başlat/bağlan döngüsünü üç kez yap.
- [ ] Sunucu yokken bağlan, yanlış adres/porta bağlan, boş kullanıcıyla bağlan: hata anlaşılır olsun, uygulama kapanmasın ve düzeltip yeniden bağlanabilsin.
- [ ] Bağlantı kurulurken ve sohbet sürerken pencereyi kapat; kapanış takılı kalmasın, geride süreç kalmasın.

## İsteğe bağlı yerel ağ

- [ ] Sunucuyu LAN kutusu açıkken başlat; özel ağ güvenlik duvarı izniyle başka bilgisayardan sunucunun IPv4 adresine bağlan.
- [ ] Ağ kesilince arayüz hata gösteriyor mu kontrol et; TCP kopmasının algılanması işletim sistemi zaman aşımını bekleyebilir.

## Teslim

- [ ] `docs/screenshots/README.md` içindeki **iki** senaryonun gerçek PNG görüntülerini çek, README'ye göm.
- [ ] Öğrenci adı/numarasını doldur; açık pencerelerde özel bilgi olmadığını kontrol et.
- [ ] Gerçek Windows sonuçlarını bu dosyaya ve `DOGRULAMA.md` içine ekle.
- [ ] Öğrencinin hesabında bağımsız public GitHub deposuna yükle; Classroom profil bağlantısını ayrıca tamamla.
