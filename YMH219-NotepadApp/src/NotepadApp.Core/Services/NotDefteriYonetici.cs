using NotepadApp.Core.Models;

namespace NotepadApp.Core.Services;

/// <summary>
/// Not koleksiyonunu (List&lt;Not&gt;), arama, filtreleme ve CRUD operasyonlarını yöneten çekirdek yönetici sınıfı.
/// </summary>
public sealed class NotDefteriYonetici
{
    private readonly List<Not> _notlar = [];
    private readonly string _veriDosyaYolu;

    public IReadOnlyList<Not> Notlar => _notlar.AsReadOnly();

    public int ToplamNotSayisi => _notlar.Count;

    public int ToplamKelimeSayisi => _notlar.Sum(n => n.KelimeSayisi);

    public string VeriDosyaYolu => _veriDosyaYolu;

    public NotDefteriYonetici(string? ozelVeriDosyaYolu = null)
    {
        _veriDosyaYolu = string.IsNullOrWhiteSpace(ozelVeriDosyaYolu)
            ? NotDepolamaServisi.GetVarsayilanVeriYolu()
            : ozelVeriDosyaYolu;
    }

    /// <summary>
    /// Yeni bir not oluşturur, listeye ekler ve son düzenlenen notu döndürür.
    /// </summary>
    public Not NotEkle(string baslik, string icerik, string kategori = "Genel")
    {
        var yeniNot = new Not(baslik, icerik, kategori);
        _notlar.Insert(0, yeniNot); // En yeni notu en üste ekle
        return yeniNot;
    }

    /// <summary>
    /// Belirtilen ID'ye sahip notu bulup günceller.
    /// </summary>
    public bool NotGuncelle(string id, string yeniBaslik, string yeniIcerik, string? yeniKategori = null)
    {
        var hedef = NotGetir(id);
        if (hedef is null) return false;

        hedef.Guncelle(yeniBaslik, yeniIcerik, yeniKategori);
        
        // Düzenlenen notu listenin en üstüne taşı
        _notlar.Remove(hedef);
        _notlar.Insert(0, hedef);
        return true;
    }

    /// <summary>
    /// Belirtilen ID'ye sahip notu listeden siler.
    /// </summary>
    public bool NotSil(string id)
    {
        var hedef = NotGetir(id);
        if (hedef is null) return false;

        return _notlar.Remove(hedef);
    }

    /// <summary>
    /// ID ile not arar.
    /// </summary>
    public Not? NotGetir(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _notlar.FirstOrDefault(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tüm notları listeden temizler.
    /// </summary>
    public void TumunuTemizle()
    {
        _notlar.Clear();
    }

    /// <summary>
    /// Arama terimine ve/veya seçili kategoriye göre notları anlık olarak filtreler.
    /// </summary>
    public List<Not> Filtrele(string? aramaMetni, string? kategori = null)
    {
        IEnumerable<Not> sonuc = _notlar;

        // Kategori filtresi
        if (!string.IsNullOrWhiteSpace(kategori) && !kategori.Equals(Kategori.Tumu, StringComparison.OrdinalIgnoreCase))
        {
            sonuc = sonuc.Where(n => n.Kategori.Equals(kategori.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Arama metni filtresi (başlık veya içerik)
        if (!string.IsNullOrWhiteSpace(aramaMetni))
        {
            string aranan = aramaMetni.Trim();
            sonuc = sonuc.Where(n =>
                n.Baslik.Contains(aranan, StringComparison.OrdinalIgnoreCase) ||
                n.Icerik.Contains(aranan, StringComparison.OrdinalIgnoreCase));
        }

        return sonuc.OrderByDescending(n => n.DuzenlemeTarihi).ToList();
    }

    /// <summary>
    /// Notları JSON dosyasından otomatik yükler.
    /// </summary>
    public int OtomatikYukle()
    {
        _notlar.Clear();
        var yuklenenler = NotDepolamaServisi.YukleJson(_veriDosyaYolu);
        _notlar.AddRange(yuklenenler.OrderByDescending(n => n.DuzenlemeTarihi));
        return _notlar.Count;
    }

    /// <summary>
    /// Mevcut notları otomatik olarak JSON dosyasına kaydeder.
    /// </summary>
    public void OtomatikKaydet()
    {
        NotDepolamaServisi.KaydetJson(_notlar, _veriDosyaYolu);
    }
}
