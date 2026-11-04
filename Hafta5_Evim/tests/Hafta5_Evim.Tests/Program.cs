using Hafta5_Evim;
using System.Globalization;

CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
var tests = new (string Name, Action Run)[]
{
    ("Boş meslek Belirtilmedi olur", () => Equal("Belirtilmedi", new Kisi("A", 2004, "").Meslek)),
    ("Boşluk meslek Belirtilmedi olur", () => Equal("Belirtilmedi", new Kisi("A", 2004, "  \t").Meslek)),
    ("Null meslek Belirtilmedi olur", () => Equal("Belirtilmedi", new Kisi("A", 2004, null!).Meslek)),
    ("Meslek setter yeni değeri de doğrular", () => { var k = new Kisi("A", 2004, "Öğrenci"); k.Meslek = " "; Equal("Belirtilmedi", k.Meslek); }),
    ("Geçerli meslekte dış boşluklar temizlenir", () => Equal("Öğretmen", new Kisi("A", 2004, " Öğretmen ").Meslek)),
    ("Yaş mevcut yıldan hesaplanır", () => Equal(22, new Kisi("A", DateTime.Now.Year - 22, "Öğrenci").YasHesapla())),
    ("1900 yılı reddedilir", () => Throws<ArgumentOutOfRangeException>(() => new Kisi("A", 1900, "Öğrenci"))),
    ("Gelecek yıl reddedilir", () => Throws<ArgumentOutOfRangeException>(() => new Kisi("A", DateTime.Now.Year + 1, "Öğrenci"))),
    ("Geçersiz yıl mevcut değeri bozmaz", () => { var k = new Kisi("A", 2004, "Öğrenci"); Throws<ArgumentOutOfRangeException>(() => k.DogumYili = 1900); Equal(2004, k.DogumYili); }),
    ("İki sınır yıl kabul edilir", () => { Equal(1901, new Kisi("A", 1901, "Öğrenci").DogumYili); Equal(0, new Kisi("B", DateTime.Now.Year, "").YasHesapla()); }),
    ("Sayaç her geçerli nesnede bir artar", () => { int once = Kisi.KisiSayisi; _ = new Kisi("A", 2004, ""); _ = new Kisi("B"); Equal(once + 2, Kisi.KisiSayisi); }),
    ("Başarısız kurucu sayacı artırmaz", () => { int once = Kisi.KisiSayisi; Throws<ArgumentOutOfRangeException>(() => new Kisi("A", 0, "")); Equal(once, Kisi.KisiSayisi); }),
    ("Tek ad kurucusu varsayılanları atar", () => { var k = new Kisi("Yeni"); Equal("Yeni", k.Ad); Equal(DateTime.Now.Year, k.DogumYili); Equal("Belirtilmedi", k.Meslek); }),
    ("Boş ev ortalaması sıfırdır", () => Equal(0.0, new Ev().OrtalamaYas())),
    ("Ortalama ondalık kısmı korur", () => { var ev = new Ev(); ev.UyeEkle(new Kisi("A", DateTime.Now.Year - 20, "")); ev.UyeEkle(new Kisi("B", DateTime.Now.Year - 21, "")); Equal(20.5, ev.OrtalamaYas()); }),
    ("En yaşlı ilk üye olmak zorunda değildir", () => { var ev = new Ev(); var yasli = new Kisi("Baba", 1968, "Mühendis"); ev.UyeEkle(new Kisi("Ben", 2004, "Öğrenci")); ev.UyeEkle(yasli); ev.UyeEkle(new Kisi("Anne", 1970, "Öğretmen")); True(ReferenceEquals(yasli, ev.EnYasli())); }),
    ("Eşit yaşta ilk eklenen döner", () => { var ev = new Ev(); var ilk = new Kisi("İlk", 1970, ""); ev.UyeEkle(ilk); ev.UyeEkle(new Kisi("İkinci", 1970, "")); True(ReferenceEquals(ilk, ev.EnYasli())); }),
    ("Boş evde en yaşlı için açıklayıcı hata", () => Throws<InvalidOperationException>(() => new Ev().EnYasli())),
    ("Null üye eklenemez", () => Throws<ArgumentNullException>(() => new Ev().UyeEkle(null!))),
    ("Listeleme bütün kişilerin bilgisini sırayla yazar", () => { var ev = new Ev(); ev.UyeEkle(new Kisi("Ada", DateTime.Now.Year - 20, "Öğrenci")); ev.UyeEkle(new Kisi("Ece", DateTime.Now.Year - 21, "")); Equal("Ada - Öğrenci - 20 yaşında\nEce - Belirtilmedi - 21 yaşında\n", Capture(ev.TumUyeleriYaz)); })
};
int failed = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failed++; Console.WriteLine($"FAIL {test.Name}: {ex.Message}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} başarılı; {failed} başarısız.");
return failed == 0 ? 0 : 1;

static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Beklenen {expected}; gelen {actual}."); }
static void True(bool value) { if (!value) throw new Exception("Beklenen nesne dönmedi."); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new Exception($"{typeof(T).Name} bekleniyordu."); }
static string Capture(Action action)
{
    var original = Console.Out;
    using var writer = new StringWriter();
    try { Console.SetOut(writer); action(); return writer.ToString().Replace("\r\n", "\n"); }
    finally { Console.SetOut(original); }
}
