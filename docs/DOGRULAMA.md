# Geliştirme doğrulama kaydı

Tarih: 3 Kasım 2026. Ortam: macOS; .NET 10 SDK ile Windows hedefli çapraz derleme.

## Gerçek sonuçlar

- İlk test koşumu: başlangıç API gövdelerinde 10/10 test eksik TCP davranışı nedeniyle `NotImplementedException` ile başarısız oldu (RED).
- Uygulama sonrasında bozuk/aşırı uzun veri testleri `InvalidDataException` yakalanmadığını gösterdi; beklenen bağlantı hata kapsamına eklenerek düzeltildi.
- Ana davranışlar 10/10 geçti. Yoğun trafik sırasında katılma entegrasyon kontrolü de eklendi.
- Son gözden geçirmede okumayı durduran gerçek TCP sunucusu testi eklendi: önce `TimeoutException` ile RED; istemci yazmasına 3 saniyelik süre sınırı eklenince GREEN. Böylece form kapanışı, durmuş bir sunucuya yazmayı süresiz beklemez.
- Son `dotnet run --project tests/ChatApp.Tests/ChatApp.Tests.csproj -c Release`: **12/12 geçti, 0 başarısız**.
- Son `dotnet build ChatApp.sln -c Release`: **0 uyarı, 0 hata**.
- Ek gözden geçirme: el sıkışma onayı gönderilmeden istemci yayın listesine eklenmemesi; görevlerin saklanması/beklenmesi; tüm protokol okuma döngülerinde uzunluk sınırı; UI kapanışında devam eden işlem kapılarının beklenmesi kontrol edildi.

## Doğrulanmamış işler

Windows Forms gerçek Windows üzerinde **çalıştırılmadı**. DPI/yeniden boyutlandırma, klavye erişimi, iki pencere etkileşimi, güvenlik duvarı/LAN ve kapanış davranışının görsel kontrolü `WINDOWS-KONTROL.md` içinde bekliyor. İki gerçek PNG görüntü, öğrenci bilgileri ve GitHub/Classroom teslimi henüz tamamlanmadı.

Testler gerçek localhost soketleri açar; başarısız test sıfırdan farklı çıkış kodu üretir. Sahte ağ nesneleri veya internette sunucu kullanılmaz. Windows GUI testini çekirdek entegrasyon testi yerine geçmiş saymayın.
