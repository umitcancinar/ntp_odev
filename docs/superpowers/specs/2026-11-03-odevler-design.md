# YMH219 — beş bağımsız Windows Forms projesi

Yetki: Kullanıcı gerekli işleri burada yapmamızı, Windows çalıştırma kontrollerini akşama bırakmamızı istedi. Tasarım soruları yerine föye uygun varsayımlarla ilerliyoruz.

## Amaç ve sınırlar
`docs/ogrenci-foyu.txt` bağlayıcı gereksinim kaynağıdır. Beş ayrı klasör, birbirine bağımlı olmayan beş yerel Git deposu olarak hazırlanacak. GitHub yayınlama daha sonra kullanıcı ile yapılacak. Geçmiş gerçek geliştirme adımlarını gösterecek; tarihler ve ekran görüntüleri uydurulmayacak.

## Yaklaşım
Föy Windows Forms istediğinden C#/.NET 10 Windows Forms kullanılacak. Web veya macOS arayüzüne dönüştürmek föyü karşılamaz. Windows hedefli derleme macOS'ta `EnableWindowsTargeting` ile yapılacak. İş kuralları uygun olduğunda `net10.0` kitaplığına ayrılacak ve gerçek davranış testleri macOS'ta çalışacak. Native arayüz, kamera ve konuşma altyapısı Windows'ta doğrulanacak.

UI: Türkçe etiketler, Segoe UI, sistem renkleri, yerleşik odak göstergeleri, büyütülebilen formlar ve TableLayoutPanel/FlowLayoutPanel. İşlem düğmeleri ve metinsel durum göstergeleri açık olacak. Gereksiz görsel efekt yok. `impeccable` ve `ui-ux-pro-max` ilkeleri yerel masaüstü kapsamına uyarlanacak.

## Bağımsız projeler (föy sırası)
1. **YMH219-FlappyBird:** Bird, PipePair ve GameSession sınıfları; iki veya daha fazla yeniden kullanılan engel, yerçekimi, tek basışta zıplama, çarpışma, skor ve yeniden başlatma. Timer yalnızca UI döngüsünü besler.
2. **YMH219-KeyEventLogger:** Yalnızca kendi formundaki KeyDown/KeyUp/KeyPress; başlat/durdur/temizle, zaman damgası, metinsel durum ve kullanıcı isteğiyle UTF-8 `.txt` kaydı. Global hook, arka plan toplama veya ağ iletişimi yok.
3. **YMH219-ChatApp:** Aynı uygulamada sunucu ve istemci görünümleri; TCP, kullanıcı adı, zaman damgalı mesajlar, istemci listesi, asenkron okuma/yazma, ayrılma ve hatalarda yeniden bağlanabilme. JSON satır protokolü, boyut sınırları ve kaynak temizliği.
4. **YMH219-SpeechApp:** System.Speech ile metni okutma/duraklatma/devam/durdurma/temizleme; kurulu ses ve tanıyıcı seçimi; mikrofon diktesi, anlaşılır hata ve durum mesajları. Desteklenmeyen Türkçe konuşma tanıma varmış gibi gösterilmeyecek.
5. **YMH219-FaceRecognition:** OpenCvSharp, dağıtıma dahil Haar cascade; kamera ve dosya, çoklu yüz kutuları; gönüllü yerel örnekler için LBPH eğitimi, en az iki etiket kontrolü, etiket ve eşik açıklamalı uzaklık skoru. Örnekler ve kişisel veriler kullanıcı profilinde yerel tutulur, Git'e girmez. Hazır gerçek kişi verisi indirilmeyecek.

## Teslim
Her depoda `.sln`, kaynak kod, `.gitignore`, Türkçe README, açıklama notları, Windows kontrol listesi ve ekran görüntüsü yönergesi bulunacak. README öğrenci adı/numarasını doldurulacak alan olarak gösterecek. Gerçek iki PNG ekran görüntüsü ve kullanıcı örnekleri Windows aşamasında eklenecek. Üst klasör ortak Windows hazırlık/başlatma araçları ve doğrulama raporu içerecek.

## Doğrulama
Beş uygulama Release olarak derlenecek. Saf iş kuralları ve TCP localhost senaryoları platformdan bağımsız test edilecek. Windows çalıştırması yapılmadığı her raporda açıkça belirtilecek. Sonraki adımlar: Windows UI kontrolü, mikrofon/kamera/iki etiket demonstrasyonu, iki ekran görüntüsü/depo, öğrenci bilgileri ve beş public GitHub deposu.
