using NotepadApp.Core.Models;
using NotepadApp.Core.Services;

var tests = new (string Name, Action Run)[]
{
    ("Yeni not oluşturma ve varsayılan alan kontrolü", TestYeniNotOlusturma),
    ("Not güncelleme ve son düzenleme tarihi yenilenmesi", TestNotGuncelleme),
    ("Not silme işlemi ve liste boyutu", TestNotSilme),
    ("JSON serileştirme ve geri yükleme veri bütünlüğü", TestJsonSerilestirmeVeYukleme),
    ("Olmayan JSON dosyasında güvenli boş liste dönmesi", TestOlmayanDosyaYukleme),
    ("Bozuk JSON içeriğinde çökmeden güvenli hata yönetimi", TestBozukJsonYonetimi),
    ("Tekil notu .txt olarak biçimlendirilmiş dışa aktarma", TestTxtTekilDisaAktar),
    ("Tüm notları toplu .txt olarak dışa aktarma", TestTxtTopluDisaAktar),
    ("Başlığa göre arama ve filtreleme", TestBasligaGoreArama),
    ("İçeriğe göre büyük/küçük harf duyarsız arama", TestIcerigeGoreArama),
    ("Kategoriye göre filtreleme (Ders, İş, Kişisel)", TestKategoriFiltreleme),
    ("Kelime ve karakter sayısı hesaplama doğruluğu", TestKelimeVeKarakterSayaci),
    ("Türkçe ve özel karakterlerin serileştirme uyumluluğu", TestTurkceKarakterUyumu),
    ("Otomatik yükleme ve tarihe göre en yeni notun üstte olması", TestOtomatikYuklemeVeSiralama),
    ("Boş veya beyaz boşluklu başlıkta güvenli varsayılan atama", TestGecersizBaslikYonetimi)
};

int basarisiz = 0;
Console.WriteLine("==================================================");
Console.WriteLine("YMH219 - NotepadApp Birim ve Entegrasyon Testleri");
Console.WriteLine("==================================================");

foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"[GEÇTİ] {test.Name}");
    }
    catch (Exception ex)
    {
        basarisiz++;
        Console.WriteLine($"[BAŞARISIZ] {test.Name} -> {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine("--------------------------------------------------");
Console.WriteLine($"Sonuç: {tests.Length - basarisiz}/{tests.Length} test geçti. ({basarisiz} başarısız)");
Console.WriteLine("==================================================");

return basarisiz == 0 ? 0 : 1;

#region Test Metotları

static void TestYeniNotOlusturma()
{
    var not = new Not("Matematik Ödevi", "Sayfa 45 soru 1-10 yapılacak.", Kategori.Ders);
    Dogrula(not.Baslik == "Matematik Ödevi", "Başlık eşleşmedi.");
    Dogrula(not.Icerik == "Sayfa 45 soru 1-10 yapılacak.", "İçerik eşleşmedi.");
    Dogrula(not.Kategori == Kategori.Ders, "Kategori eşleşmedi.");
    Dogrula(!string.IsNullOrWhiteSpace(not.Id), "ID boş olamaz.");
    Dogrula(not.OlusturmaTarihi <= DateTime.Now, "Oluşturma tarihi hatalı.");
}

static void TestNotGuncelleme()
{
    var not = new Not("Eski Başlık", "Eski İçerik", Kategori.Genel);
    DateTime eskiTarih = not.DuzenlemeTarihi;
    Thread.Sleep(20);

    not.Guncelle("Yeni Başlık", "Yeni İçerik", Kategori.Is);
    Dogrula(not.Baslik == "Yeni Başlık", "Güncel başlık hatalı.");
    Dogrula(not.Icerik == "Yeni İçerik", "Güncel içerik hatalı.");
    Dogrula(not.Kategori == Kategori.Is, "Güncel kategori hatalı.");
    Dogrula(not.DuzenlemeTarihi >= eskiTarih, "Düzenleme tarihi güncellenmedi.");
}

static void TestNotSilme()
{
    var yonetici = new NotDefteriYonetici(Path.GetTempFileName());
    var not = yonetici.NotEkle("Silinecek Not", "İçerik");
    Dogrula(yonetici.ToplamNotSayisi == 1, "Not eklenemedi.");

    bool silindi = yonetici.NotSil(not.Id);
    Dogrula(silindi, "Not silme başarısız döndü.");
    Dogrula(yonetici.ToplamNotSayisi == 0, "Not listeden düşmedi.");
}

static void TestJsonSerilestirmeVeYukleme()
{
    string geciciDosya = Path.Combine(Path.GetTempPath(), $"notlar_test_{Guid.NewGuid():N}.json");
    try
    {
        var liste = new List<Not>
        {
            new("Not 1", "İçerik 1", Kategori.Ders),
            new("Not 2", "İçerik 2", Kategori.Kisisel)
        };

        NotDepolamaServisi.KaydetJson(liste, geciciDosya);
        Dogrula(File.Exists(geciciDosya), "JSON dosyası oluşturulmadı.");

        var yuklenenler = NotDepolamaServisi.YukleJson(geciciDosya);
        Dogrula(yuklenenler.Count == 2, "Geri yüklenen not sayısı hatalı.");
        Dogrula(yuklenenler[0].Baslik == "Not 1", "İlk not başlığı eşleşmedi.");
        Dogrula(yuklenenler[1].Kategori == Kategori.Kisisel, "İkinci not kategorisi eşleşmedi.");
    }
    finally
    {
        if (File.Exists(geciciDosya)) File.Delete(geciciDosya);
    }
}

static void TestOlmayanDosyaYukleme()
{
    string olmayanDosya = Path.Combine(Path.GetTempPath(), $"olmayan_{Guid.NewGuid():N}.json");
    var sonuc = NotDepolamaServisi.YukleJson(olmayanDosya);
    Dogrula(sonuc != null && sonuc.Count == 0, "Olmayan dosya için boş liste dönmeli.");
}

static void TestBozukJsonYonetimi()
{
    string bozukDosya = Path.Combine(Path.GetTempPath(), $"bozuk_{Guid.NewGuid():N}.json");
    try
    {
        File.WriteAllText(bozukDosya, "{ bu bir geçersiz ve bozuk json içeriğidir... ");
        var sonuc = NotDepolamaServisi.YukleJson(bozukDosya);
        Dogrula(sonuc != null && sonuc.Count == 0, "Bozuk dosyada çökmeden boş liste dönmeli.");
    }
    finally
    {
        if (File.Exists(bozukDosya)) File.Delete(bozukDosya);
    }
}

static void TestTxtTekilDisaAktar()
{
    string txtDosya = Path.Combine(Path.GetTempPath(), $"tekil_{Guid.NewGuid():N}.txt");
    try
    {
        var not = new Not("Alışveriş Listesi", "Süt, Yumurta, Ekmek", Kategori.Kisisel);
        NotDepolamaServisi.DisaAktarTxt(not, txtDosya);
        Dogrula(File.Exists(txtDosya), "TXT dosyası oluşturulamadı.");

        string icerik = File.ReadAllText(txtDosya);
        Dogrula(icerik.Contains("BAŞLIK         : Alışveriş Listesi"), "TXT içinde başlık eksik.");
        Dogrula(icerik.Contains("KATEGORİ       : Kişisel"), "TXT içinde kategori eksik.");
        Dogrula(icerik.Contains("Süt, Yumurta, Ekmek"), "TXT içinde gövde metni eksik.");
    }
    finally
    {
        if (File.Exists(txtDosya)) File.Delete(txtDosya);
    }
}

static void TestTxtTopluDisaAktar()
{
    string topluTxt = Path.Combine(Path.GetTempPath(), $"toplu_{Guid.NewGuid():N}.txt");
    try
    {
        var notlar = new List<Not>
        {
            new("Proje A", "İçerik A", Kategori.Is),
            new("Proje B", "İçerik B", Kategori.Ders)
        };
        NotDepolamaServisi.TumunuDisaAktarTxt(notlar, topluTxt);
        Dogrula(File.Exists(topluTxt), "Toplu TXT oluşturulamadı.");

        string txt = File.ReadAllText(topluTxt);
        Dogrula(txt.Contains("YMH219 NOT DEFTERİ"), "Başlık eksik.");
        Dogrula(txt.Contains("Proje A") && txt.Contains("Proje B"), "Notlar eksik.");
    }
    finally
    {
        if (File.Exists(topluTxt)) File.Delete(topluTxt);
    }
}

static void TestBasligaGoreArama()
{
    var yonetici = new NotDefteriYonetici(Path.GetTempFileName());
    yonetici.NotEkle("Vize Tarihleri", "20 Kasım 2026", Kategori.Ders);
    yonetici.NotEkle("Market Listesi", "Domates, Salatalık", Kategori.Kisisel);

    var sonuclar = yonetici.Filtrele("Vize");
    Dogrula(sonuclar.Count == 1, "Arama sonucu 1 adet olmalıydı.");
    Dogrula(sonuclar[0].Baslik == "Vize Tarihleri", "Doğru not bulunamadı.");
}

static void TestIcerigeGoreArama()
{
    var yonetici = new NotDefteriYonetici(Path.GetTempFileName());
    yonetici.NotEkle("Görevler", "Hafta sonu araba yıkanacak.", Kategori.Kisisel);
    yonetici.NotEkle("Toplantı", "Yönetici ile bütçe planlaması.", Kategori.Is);

    var sonuclar = yonetici.Filtrele("araba");
    Dogrula(sonuclar.Count == 1, "İçerik araması sonucu bulunamadı.");
    Dogrula(sonuclar[0].Baslik == "Görevler", "Yanlış not eşleşti.");
}

static void TestKategoriFiltreleme()
{
    var yonetici = new NotDefteriYonetici(Path.GetTempFileName());
    yonetici.NotEkle("Ders Notu 1", "A", Kategori.Ders);
    yonetici.NotEkle("Ders Notu 2", "B", Kategori.Ders);
    yonetici.NotEkle("İş Notu 1", "C", Kategori.Is);

    var dersNotlari = yonetici.Filtrele(null, Kategori.Ders);
    Dogrula(dersNotlari.Count == 2, "Ders kategorisinde 2 not olmalıydı.");

    var isNotlari = yonetici.Filtrele(null, Kategori.Is);
    Dogrula(isNotlari.Count == 1, "İş kategorisinde 1 not olmalıydı.");
}

static void TestKelimeVeKarakterSayaci()
{
    var not = new Not("Deneme", "Bu bir beş kelimelik nottur.");
    Dogrula(not.KelimeSayisi == 5, $"Kelime sayısı 5 olmalı, bulunan: {not.KelimeSayisi}");
    Dogrula(not.KarakterSayisi == 28, $"Karakter sayısı 28 olmalı, bulunan: {not.KarakterSayisi}");

    var bosNot = new Not("Boş", "   ");
    Dogrula(bosNot.KelimeSayisi == 0, "Boşluklarda kelime sayısı 0 olmalı.");
}

static void TestTurkceKarakterUyumu()
{
    string testYolu = Path.Combine(Path.GetTempPath(), $"turkce_{Guid.NewGuid():N}.json");
    try
    {
        string turkceIcerik = "Çığır açan şölen, ılık öğleden sonra üzüm bağlarında başladı.";
        var not = new Not("Türkçe Karakter Testi ığüşöç", turkceIcerik, Kategori.Onemli);

        NotDepolamaServisi.KaydetJson([not], testYolu);
        var okunan = NotDepolamaServisi.YukleJson(testYolu);

        Dogrula(okunan.Count == 1, "Not okunamadı.");
        Dogrula(okunan[0].Icerik == turkceIcerik, "Türkçe karakterler bozulmuş.");
        Dogrula(okunan[0].Baslik == "Türkçe Karakter Testi ığüşöç", "Başlıktaki Türkçe karakterler bozulmuş.");
    }
    finally
    {
        if (File.Exists(testYolu)) File.Delete(testYolu);
    }
}

static void TestOtomatikYuklemeVeSiralama()
{
    string dosya = Path.Combine(Path.GetTempPath(), $"siralama_{Guid.NewGuid():N}.json");
    try
    {
        var eskiNot = new Not("Eski Not", "İçerik") { DuzenlemeTarihi = DateTime.Now.AddHours(-2) };
        var yeniNot = new Not("Yeni Not", "İçerik") { DuzenlemeTarihi = DateTime.Now };

        NotDepolamaServisi.KaydetJson([eskiNot, yeniNot], dosya);

        var yonetici = new NotDefteriYonetici(dosya);
        yonetici.OtomatikYukle();

        Dogrula(yonetici.ToplamNotSayisi == 2, "Yüklenen not sayısı 2 olmalı.");
        Dogrula(yonetici.Notlar[0].Baslik == "Yeni Not", "En yeni not listenin başında olmalı.");
    }
    finally
    {
        if (File.Exists(dosya)) File.Delete(dosya);
    }
}

static void TestGecersizBaslikYonetimi()
{
    var not = new Not("", "İçerik");
    Dogrula(not.Baslik == "Başlıksız Not", "Boş başlıkta varsayılan atanmalı.");

    not.Guncelle("   ", "Yeni İçerik");
    Dogrula(not.Baslik == "Başlıksız Not", "Boşluklu güncellemede varsayılan atanmalı.");
}

static void Dogrula(bool sart, string mesaj)
{
    if (!sart) throw new InvalidOperationException(mesaj);
}

#endregion
