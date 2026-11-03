# İki gerçek Windows ekran görüntüsü

PNG dosyaları henüz yoktur. Windows'ta uygulamayı çalıştırın; yalnızca kişisel bilgi içermeyen örnek giriş kullanın.

1. Başlat'a basın; deneme alanına `Merhaba dünya, İğüş 123` yazın. KeyDown, KeyPress, KeyUp satırları ve **KAYIT AÇIK** durumu görünürken gerçek pencereyi Windows Ekran Alıntısı Aracı ile çekin. `01-kayit-acik.png` olarak bu klasöre kaydedin.
2. Durdur'a basın; Dosyaya Kaydet ile Belgeler klasörüne örnek `.txt` yazın. İletişim kutusu kapandıktan sonra **DURDURULDU**, dolu olay listesi ve başarı metni görünürken çekin. `02-kayit-durdu-kaydedildi.png` olarak kaydedin.
3. İki PNG'yi açıp okunabilirlik ve kişisel bilgi kontrolü yapın. Dosyalar gerçekten oluşunca README'deki bekleme açıklamasını şu Markdown ile değiştirin:

```markdown
![Kayıt açık ve zaman damgalı klavye olayları](docs/screenshots/01-kayit-acik.png)
![Durdurulmuş ve UTF-8 dosyasına kaydedilmiş kayıt](docs/screenshots/02-kayit-durdu-kaydedildi.png)
```

4. README önizlemesini kontrol edin, PNG'ler ve README için yeni gerçek commit oluşturun. Yerel dışa aktarım dosyasını Git'e eklemeyin. Ekran görüntüsü veya tarih üretmeyin/değiştirmeyin.
