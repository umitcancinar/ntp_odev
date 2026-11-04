# Program çıktısının ekran görüntüsü

`01-konsol-ciktisi.png`, 4 Kasım 2026 saat 08:50:19'da (Türkiye saati) çalıştırılan uygulamanın gerçek standart çıktısını gösterir.

Çalıştırılan komut:

```text
dotnet run --project Hafta5_Evim.csproj -c Release --no-build
```

Programın çıkış kodu 0'dır. Uygulamanın stdout metni yerel HTTP çıktı görüntüleyicisinde açılmış ve gerçek tarayıcı ekran görüntüsü Playwright ile alınmıştır. Görüntü bir Visual Studio veya işletim sistemi Terminal penceresi olarak sunulmaz; görüntüleyicinin altında bu yöntem açıkça yazılıdır. Çıktı elle yeniden yazılmamış, PNG'ye sonradan ekleme yapılmamıştır.

Word raporunda `Kisi.cs`, `Ev.cs` ve `Program.cs` kopyalanabilir metindir. Son sayfada bu PNG gömülüdür. Aynı çıktı `Konsol_Ciktisi.txt` dosyasında da bulunur.
