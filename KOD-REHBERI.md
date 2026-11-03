# Sözlü kontrol için kod rehberi

## Sorumluluklar

| Sınıf | Görevi | Önemli alan / metot |
|---|---|---|
| `Bird` | Kuşun konumu ve hızını saklar | `Y`, `Velocity`, `Flap`, `Advance`, `CollidesWith` |
| `PipePair` | Üst-alt engeli tek nesne olarak taşır | `X`, `GapTop`, `TryScore`, `Recycle` |
| `GameSession` | Oyun kurallarını ve nesneleri yönetir | `State`, `Score`, `Start`, `PressFlap`, `Advance` |
| `MainForm` | Düğme, klavye, zamanlayıcı ve durum metnini bağlar | `NewGame`, `OnGameKeyDown`, `TickGame`, `TogglePause` |
| `GameCanvas` | Oturumu okuyup ekrana çizer | `OnPaint`, `DoubleBuffered` |
| `Program` | Windows Forms uygulamasını başlatır | `[STAThread] Main` |

## Oyun döngüsünü anlatma

`Timer` yaklaşık 16 ms'de bir UI iş parçacığında olay üretir. Gerçek geçen süre `Stopwatch` ile ölçülür; `game.Advance(elapsed)` çağrılır ve `Invalidate()` yeniden çizim ister. Zamanlayıcı kesin 60 FPS garantisi vermez. Çizim fizik hesabını değiştirmez; `OnPaint` yalnızca mevcut durumu okur.

`Bird.Advance`: `Velocity += Gravity * seconds`, ardından `Y += Velocity * seconds`. Ekranda Y aşağı doğru büyüdüğünden negatif hız yukarı zıplamadır. `Flap` hızı negatif yapar. `GameSession` 10 ms'yi aşmayan alt adımlar kullanarak engelin içinden atlama riskini azaltır.

İlk `PressFlap` hız verir ve `flapHeld` bayrağını açar. İşletim sisteminin aynı tuş için tekrarlanan KeyDown olayları ikinci zıplamaya dönüşmez. KeyUp bayrağı kapatır. Form, aynı anda tutulan Boşluk ve Yukarı ok tuşlarını da `HashSet<Keys>` ile izler.

Çarpışma üç koşulla biter: üst sınır, zemin veya yatay aralıkları kesişen borunun boşluğu dışında kalma. `GameOver` durumunda `Advance` hemen döner; skor ve engel hareketi durur. `Start` kuşu yeniden oluşturur, engelleri sağa alır, skoru ve tuş kilidini sıfırlar.

`PipePair.TryScore` ancak borunun sağ kenarı kuşun solundan geçince `true` verir. `scored` bayrağı aynı çifti tekrar saydırmaz. `Recycle`, bayrağı temizler ve boruyu diğerinin 340 dünya birimi sağına taşır. Her kare yeni engel nesnesi oluşturulmaz.

## Nesne tabanlı programlama açısından

Konum ve hız özelliklerinin setter'ları `private` olduğu için UI fizik değerlerini rastgele değiştiremez. `GameSession` kuş ve boruları bir araya getirir (bileşim); `GameCanvas` ve `MainForm` Windows Forms sınıflarını genişletir (kalıtım). `IReadOnlyList` dışarıya sadece okuma görünümü verir. Çekirdekte Form, Timer veya çizim türü olmaması testleri macOS'ta da çalıştırmayı sağlar.

## Küçük değişiklik alıştırmaları

- Yerçekimini artırmak için `Bird.Gravity`; zıplama kuvveti için `FlapVelocity` değerini inceleyin. Negatif işaretin anlamını açıklayın.
- Engelleri hızlandırmak için `PipePair.Speed`; boşluğu daraltmak için `GapHeight` kullanılır. Rastgele boşluğun zemin içinde kalmasını koruyun.
- Üçüncü engel eklerken `GameSession.pipes` listesi ile ilk konumlandırma döngüsünü inceleyin.
- Skorun iki kez artmasını hangi testin yakaladığını gösterin.

## Dürüst doğrulama sınırı

Çekirdek için 13 test ve Windows hedefli derleme çalıştırıldı. Native pencerenin gerçek Windows'ta görüntülenmesi, DPI ve klavye kontrolü ile iki gerçek PNG henüz bekliyor. Test başarıları, Windows arayüzünün çalıştırıldığı anlamına gelmez.
