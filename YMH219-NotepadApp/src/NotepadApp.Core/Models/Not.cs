namespace NotepadApp.Core.Models;

/// <summary>
/// Not defterindeki her bir not kaydını temsil eden temel veri modeli sınıfı.
/// </summary>
public sealed class Not
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Baslik { get; set; } = "Yeni Not";

    public string Icerik { get; set; } = string.Empty;

    public string Kategori { get; set; } = "Genel";

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public DateTime DuzenlemeTarihi { get; set; } = DateTime.Now;

    /// <summary>
    /// Not içeriğindeki toplam kelime sayısı (boşluklara göre ayrıştırılır).
    /// </summary>
    public int KelimeSayisi
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Icerik))
                return 0;

            return Icerik.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }

    /// <summary>
    /// Not içeriğindeki toplam karakter sayısı.
    /// </summary>
    public int KarakterSayisi => Icerik?.Length ?? 0;

    public Not()
    {
    }

    public Not(string baslik, string icerik, string kategori = "Genel")
    {
        Baslik = string.IsNullOrWhiteSpace(baslik) ? "Başlıksız Not" : baslik.Trim();
        Icerik = icerik ?? string.Empty;
        Kategori = string.IsNullOrWhiteSpace(kategori) ? "Genel" : kategori.Trim();
        OlusturmaTarihi = DateTime.Now;
        DuzenlemeTarihi = DateTime.Now;
    }

    /// <summary>
    /// Notun başlığını, içeriğini ve varsa kategorisini günceller, son düzenleme tarihini yeniler.
    /// </summary>
    public void Guncelle(string yeniBaslik, string yeniIcerik, string? yeniKategori = null)
    {
        Baslik = string.IsNullOrWhiteSpace(yeniBaslik) ? "Başlıksız Not" : yeniBaslik.Trim();
        Icerik = yeniIcerik ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(yeniKategori))
        {
            Kategori = yeniKategori.Trim();
        }
        DuzenlemeTarihi = DateTime.Now;
    }

    public override string ToString()
    {
        return $"[{Kategori}] {Baslik} ({DuzenlemeTarihi:dd.MM.yyyy HH:mm})";
    }
}
