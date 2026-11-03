# İki gerçek Windows ekran görüntüsü

Bu klasörde henüz PNG yoktur. Ekran görüntüleri Windows'ta çalışan gerçek uygulamadan alınacaktır.

1. `dotnet run --project src/FlappyBird -c Release` ile açın. Yeni Oyun'a basıp en az bir engel geçin. Kuş, iki engel çifti ve skor görünürken Windows Ekran Alıntısı Aracı ile uygulama penceresini çekin. `01-oyun-skor.png` olarak bu klasöre kaydedin.
2. Kuşu bilerek bir boruya veya zemine çarptırın. Oyun bitti, skor ve Yeni Oyun görünürken çekin. `02-oyun-bitti.png` olarak kaydedin.
3. PNG'leri açarak metnin okunabildiğini ve gerçek uygulamayı gösterdiğini doğrulayın. İki dosya gerçekten eklendikten sonra README'deki bekleme açıklamasının yerine şu Markdown'ı ekleyin:

```markdown
![Oyun sırasında skor ve engeller](docs/screenshots/01-oyun-skor.png)
![Çarpışma sonrası oyun bitti](docs/screenshots/02-oyun-bitti.png)
```

4. README önizlemesini kontrol edin ve gerçek dosyalarla yeni bir commit oluşturun. Çekim tarihi veya geliştirme geçmişini değiştirmeyin.
