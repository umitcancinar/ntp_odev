# Windows çalıştırma kontrol listesi

Durum: **Henüz Windows'ta uygulanmadı.** Kontrolü yapan kişi, tarih ve Windows sürümünü sonunda doldurun.

- [ ] .NET 10 SDK / Windows Desktop altyapısı kurulu; `dotnet --info` ile kontrol edildi.
- [ ] `dotnet build FlappyBird.sln -c Release` hatasız; `dotnet run --project tests/FlappyBird.Tests -c Release` 13/13 başarılı.
- [ ] `dotnet run --project src/FlappyBird -c Release` pencereyi açıyor; başlangıçta hazır durumu anlaşılır.
- [ ] Yeni Oyun / R skoru sıfırlıyor ve iki boru çifti sağdan sola geliyor.
- [ ] Boşluk ve Yukarı ok yukarı zıplatıyor; birini basılı tutunca otomatik yeniden zıplama olmuyor. İkisini beraber tutma ve sırayla bırakma kontrol edildi.
- [ ] Zıplama bırakılınca yerçekimi görülüyor; üst boru, alt boru, üst sınır ve zemin ayrı ayrı oyunu bitiriyor.
- [ ] Bir boru çifti geçilince skor bir artıyor; aynı çift ekrandan çıkarken tekrar artmıyor.
- [ ] En az 3 çift geçildi; ekran dışına çıkan engeller yeni boşluk yüksekliğiyle sağdan dönüyor.
- [ ] Oyun bitince kuş, engeller ve skor sabit kalıyor; Yeni Oyun ile tekrar oynanabiliyor.
- [ ] Duraklat / P çalışıyor; Alt+Tab oyunu duraklatıyor; geri dönünce kendiliğinden hareket etmiyor. Devam et ile sürüyor.
- [ ] Tab ve Shift+Tab odak sırası, Alt+Y / Alt+D düğme erişimi ve görünür oyun alanı odağı kontrol edildi.
- [ ] En küçük boyutta ve %100 / %150 / %200 DPI'da metinler kesilmiyor, oyun oranı bozulmuyor. Sistem yüksek karşıtlık modunda sınırlar görülüyor.
- [ ] Pencereyi oyun çalışırken kapatma hata vermiyor; tekrar açma temiz oturum veriyor.
- [ ] `docs/screenshots/README.md` senaryolarıyla iki gerçek PNG çekildi ve README'ye eklendi.

Kontrol eden: __________  Tarih: __________  Windows / DPI: __________
