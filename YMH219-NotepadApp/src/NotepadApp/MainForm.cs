using System.Diagnostics;
using NotepadApp.Core.Models;
using NotepadApp.Core.Services;

namespace NotepadApp;

public sealed class MainForm : Form
{
    private readonly NotDefteriYonetici _yonetici = new();
    private Not? _seciliNot;
    private bool _yukleniyor;

    // Menü
    private readonly MenuStrip _menu = new();

    // Sol Panel Kontrolleri
    private readonly TextBox _txtArama = new() { Dock = DockStyle.Top, PlaceholderText = "Başlık veya içerikte ara..." };
    private readonly ComboBox _cmbKategoriFiltre = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ListBox _lstNotlar = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly Button _btnYeni = new() { Text = "＋ Yeni Not", Dock = DockStyle.Left, Width = 130, Height = 36 };
    private readonly Button _btnSil = new() { Text = "🗑 Notu Sil", Dock = DockStyle.Right, Width = 130, Height = 36 };

    // Sağ Panel Kontrolleri
    private readonly TextBox _txtBaslik = new() { Dock = DockStyle.Fill, PlaceholderText = "Not Başlığı", Font = new Font("Segoe UI", 12, FontStyle.Bold) };
    private readonly ComboBox _cmbKategori = new() { Dock = DockStyle.Right, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblZaman = new() { Dock = DockStyle.Top, Height = 24, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8.5f) };
    private readonly TextBox _txtIcerik = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 11) };

    // Alt Durum Çubuğu
    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _lblDurum = new() { Text = "Hazır", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _lblSayac = new() { Text = "0 kelime, 0 karakter" };

    // Otomatik Kaydetme Zamanlayıcısı
    private readonly System.Windows.Forms.Timer _otoKayitTimer = new() { Interval = 1200 };

    public MainForm()
    {
        Text = "YMH219 · Not Defteri (Serileştirme & Dosya İşlemleri)";
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        BilesenleriOlustur();
        OlaylariBagla();
    }

    private void BilesenleriOlustur()
    {
        // Menü Çubuğu
        var menuDosya = new ToolStripMenuItem("&Dosya");
        var menuYeni = new ToolStripMenuItem("&Yeni Not", null, (_, _) => YeniNotOlustur(), Keys.Control | Keys.N);
        var menuKaydet = new ToolStripMenuItem("&Kaydet", null, (_, _) => NotuKaydet(true), Keys.Control | Keys.S);
        var menuTxtAktar = new ToolStripMenuItem("Seçili Notu &TXT Olarak Dışa Aktar...", null, (_, _) => SeciliNotuTxtAktar());
        var menuTopluTxt = new ToolStripMenuItem("&Tüm Notları TXT Olarak Dışa Aktar...", null, (_, _) => TumunuTxtAktar());
        var menuKlasor = new ToolStripMenuItem("Veri &Klasörünü Aç", null, (_, _) => VeriKlasorunuAc());
        var menuCikis = new ToolStripMenuItem("&Çıkış", null, (_, _) => Close(), Keys.Alt | Keys.F4);
        menuDosya.DropDownItems.AddRange([menuYeni, menuKaydet, new ToolStripSeparator(), menuTxtAktar, menuTopluTxt, menuKlasor, new ToolStripSeparator(), menuCikis]);

        var menuDuzen = new ToolStripMenuItem("&Düzen");
        var menuSil = new ToolStripMenuItem("&Seçili Notu Sil", null, (_, _) => SeciliNotuSil(), Keys.Delete);
        var menuTemizle = new ToolStripMenuItem("&Tüm Notları Temizle", null, (_, _) => TumNotlariTemizle());
        menuDuzen.DropDownItems.AddRange([menuSil, menuTemizle]);

        var menuHakkinda = new ToolStripMenuItem("&Hakkında", null, (_, _) =>
        {
            MessageBox.Show(
                "YMH219 Nesne Tabanlı Programlama\nEk Uygulama Ödevi — Not Defteri\n\n" +
                "Öğrenci: Ümitcan Çinar\nNumara: 250541009\n\n" +
                "Özellikler:\n" +
                "• JSON Dosya Serileştirme & Otomatik Geri Yükleme\n" +
                "• TXT Dışa Aktarma (Tekil ve Toplu)\n" +
                "• Anlık Arama ve Kategori Filtreleme\n" +
                "• Canlı Kelime/Karakter İstatistiği ve Otomatik Kayıt",
                "Hakkında — YMH219 NotepadApp",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        });

        _menu.Items.AddRange([menuDosya, menuDuzen, menuHakkinda]);
        MainMenuStrip = _menu;
        Controls.Add(_menu);

        // Durum Çubuğu
        _statusStrip.Items.AddRange([_lblDurum, _lblSayac]);
        Controls.Add(_statusStrip);

        // Ana Bölücü Panel
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320,
            SplitterWidth = 6
        };

        // Sol Panel: Arama, Kategori Filtresi, Liste, Butonlar
        var solPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var solUstPanel = new Panel { Dock = DockStyle.Top, Height = 66 };
        _cmbKategoriFiltre.Items.AddRange(Kategori.FiltreKategorileri);
        _cmbKategoriFiltre.SelectedIndex = 0;

        solUstPanel.Controls.Add(_lstNotlar); // yer tutucu sıra
        solUstPanel.Controls.Clear();
        solUstPanel.Controls.Add(_cmbKategoriFiltre);
        solUstPanel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 4 });
        solUstPanel.Controls.Add(_txtArama);

        var solAltPanel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(0, 8, 0, 0) };
        solAltPanel.Controls.Add(_btnYeni);
        solAltPanel.Controls.Add(_btnSil);

        solPanel.Controls.Add(_lstNotlar);
        solPanel.Controls.Add(solUstPanel);
        solPanel.Controls.Add(solAltPanel);
        split.Panel1.Controls.Add(solPanel);

        // Sağ Panel: Başlık, Kategori Seçici, Zaman Bilgisi, İçerik Editörü
        var sagPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var sagUstPanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        _cmbKategori.Items.AddRange(Kategori.VarsayilanKategoriler);
        _cmbKategori.SelectedIndex = 0;

        sagUstPanel.Controls.Add(_txtBaslik);
        sagUstPanel.Controls.Add(_cmbKategori);

        sagPanel.Controls.Add(_txtIcerik);
        sagPanel.Controls.Add(_lblZaman);
        sagPanel.Controls.Add(sagUstPanel);
        split.Panel2.Controls.Add(sagPanel);

        Controls.Add(split);
    }

    private void OlaylariBagla()
    {
        Load += (_, _) => FormYuklendi();
        FormClosing += (_, _) => FormKapanirken();

        _lstNotlar.SelectedIndexChanged += (_, _) => NotSecildi();
        _txtArama.TextChanged += (_, _) => ListeyiYenile();
        _cmbKategoriFiltre.SelectedIndexChanged += (_, _) => ListeyiYenile();

        _btnYeni.Click += (_, _) => YeniNotOlustur();
        _btnSil.Click += (_, _) => SeciliNotuSil();

        _txtBaslik.TextChanged += (_, _) => DegisiklikOldu();
        _txtIcerik.TextChanged += (_, _) =>
        {
            SayaclariGuncelle();
            DegisiklikOldu();
        };
        _cmbKategori.SelectedIndexChanged += (_, _) => DegisiklikOldu();

        _otoKayitTimer.Tick += (_, _) =>
        {
            _otoKayitTimer.Stop();
            NotuKaydet(false);
        };
    }

    private void FormYuklendi()
    {
        try
        {
            int yuklenenSayi = _yonetici.OtomatikYukle();
            _lblDurum.Text = $"{yuklenenSayi} not JSON veritabanından yüklendi.";
        }
        catch (Exception ex)
        {
            _lblDurum.Text = $"Yükleme uyarısı: {ex.Message}";
        }

        ListeyiYenile();

        if (_lstNotlar.Items.Count > 0)
        {
            _lstNotlar.SelectedIndex = 0;
        }
        else
        {
            YeniNotOlustur();
        }
    }

    private void FormKapanirken()
    {
        try
        {
            NotuKaydet(false);
            _yonetici.OtomatikKaydet();
        }
        catch
        {
            // Kapanışta sessiz hata koruması
        }
    }

    private void ListeyiYenile()
    {
        string? aranan = _txtArama.Text;
        string? kategori = _cmbKategoriFiltre.SelectedItem?.ToString();

        var filtrelenmis = _yonetici.Filtrele(aranan, kategori);

        _yukleniyor = true;
        _lstNotlar.BeginUpdate();
        _lstNotlar.Items.Clear();

        foreach (var not in filtrelenmis)
        {
            _lstNotlar.Items.Add(not);
        }

        _lstNotlar.EndUpdate();
        _yukleniyor = false;

        if (_seciliNot != null)
        {
            int index = _lstNotlar.Items.IndexOf(_seciliNot);
            if (index >= 0) _lstNotlar.SelectedIndex = index;
        }
    }

    private void NotSecildi()
    {
        if (_yukleniyor) return;

        // Önceki not varsa kaydet
        if (_seciliNot != null && _otoKayitTimer.Enabled)
        {
            _otoKayitTimer.Stop();
            NotuKaydet(false);
        }

        _seciliNot = _lstNotlar.SelectedItem as Not;
        if (_seciliNot == null)
        {
            FormuTemizle();
            return;
        }

        _yukleniyor = true;
        _txtBaslik.Text = _seciliNot.Baslik;
        _txtIcerik.Text = _seciliNot.Icerik;

        int katIndex = _cmbKategori.Items.IndexOf(_seciliNot.Kategori);
        _cmbKategori.SelectedIndex = katIndex >= 0 ? katIndex : 0;

        _lblZaman.Text = $"Oluşturulma: {_seciliNot.OlusturmaTarihi:dd.MM.yyyy HH:mm}   |   Son Düzenleme: {_seciliNot.DuzenlemeTarihi:dd.MM.yyyy HH:mm}";
        SayaclariGuncelle();
        _yukleniyor = false;
    }

    private void DegisiklikOldu()
    {
        if (_yukleniyor || _seciliNot == null) return;

        _lblDurum.Text = "Düzenleniyor (otomatik kaydedilecek)...";
        _otoKayitTimer.Stop();
        _otoKayitTimer.Start();
    }

    private void NotuKaydet(bool bildirimVer)
    {
        if (_seciliNot == null) return;

        string yeniBaslik = string.IsNullOrWhiteSpace(_txtBaslik.Text) ? "Başlıksız Not" : _txtBaslik.Text.Trim();
        string yeniIcerik = _txtIcerik.Text;
        string yeniKategori = _cmbKategori.SelectedItem?.ToString() ?? Kategori.Genel;

        _yonetici.NotGuncelle(_seciliNot.Id, yeniBaslik, yeniIcerik, yeniKategori);
        _yonetici.OtomatikKaydet();

        _lblZaman.Text = $"Oluşturulma: {_seciliNot.OlusturmaTarihi:dd.MM.yyyy HH:mm}   |   Son Düzenleme: {_seciliNot.DuzenlemeTarihi:dd.MM.yyyy HH:mm}";
        _lblDurum.Text = $"Kaydedildi: {DateTime.Now:HH:mm:ss}";

        // Liste görünümünü güncelle
        int seciliIndex = _lstNotlar.SelectedIndex;
        if (seciliIndex >= 0)
        {
            _lstNotlar.Items[seciliIndex] = _seciliNot;
        }

        if (bildirimVer)
        {
            _lblDurum.Text = "Not başarıyla kaydedildi.";
        }
    }

    private void YeniNotOlustur()
    {
        var yeniNot = _yonetici.NotEkle("Yeni Not", string.Empty, Kategori.Genel);
        _yonetici.OtomatikKaydet();

        ListeyiYenile();
        _lstNotlar.SelectedItem = yeniNot;
        _txtBaslik.Focus();
        _txtBaslik.SelectAll();
        _lblDurum.Text = "Yeni not oluşturuldu.";
    }

    private void SeciliNotuSil()
    {
        if (_seciliNot == null) return;

        var cevap = MessageBox.Show(
            $"\"{_seciliNot.Baslik}\" başlıklı notu silmek istediğinize emin misiniz?",
            "Notu Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (cevap == DialogResult.Yes)
        {
            _yonetici.NotSil(_seciliNot.Id);
            _yonetici.OtomatikKaydet();
            _seciliNot = null;
            ListeyiYenile();

            if (_lstNotlar.Items.Count > 0)
            {
                _lstNotlar.SelectedIndex = 0;
            }
            else
            {
                FormuTemizle();
            }

            _lblDurum.Text = "Not silindi.";
        }
    }

    private void TumNotlariTemizle()
    {
        if (_yonetici.ToplamNotSayisi == 0) return;

        var cevap = MessageBox.Show(
            "Tüm notları silmek istediğinize emin misiniz? Bu işlem geri alınamaz.",
            "Tümünü Temizle",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (cevap == DialogResult.Yes)
        {
            _yonetici.TumunuTemizle();
            _yonetici.OtomatikKaydet();
            _seciliNot = null;
            ListeyiYenile();
            FormuTemizle();
            _lblDurum.Text = "Tüm notlar temizlendi.";
        }
    }

    private void SeciliNotuTxtAktar()
    {
        if (_seciliNot == null)
        {
            MessageBox.Show("Lütfen dışa aktarılacak bir not seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Metin Belgesi (*.txt)|*.txt",
            FileName = $"{GecerliDosyaAdi(_seciliNot.Baslik)}.txt",
            Title = "Notu TXT Olarak Kaydet"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            NotDepolamaServisi.DisaAktarTxt(_seciliNot, sfd.FileName);
            _lblDurum.Text = $"TXT olarak dışa aktarıldı: {Path.GetFileName(sfd.FileName)}";
            MessageBox.Show("Not başarıyla TXT olarak kaydedildi.", "Dışa Aktarma Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void TumunuTxtAktar()
    {
        if (_yonetici.ToplamNotSayisi == 0)
        {
            MessageBox.Show("Dışa aktarılacak not bulunmuyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Metin Belgesi (*.txt)|*.txt",
            FileName = $"Tum_Notlar_{DateTime.Now:yyyyMMdd_HHmm}.txt",
            Title = "Tüm Notları TXT Olarak Kaydet"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            NotDepolamaServisi.TumunuDisaAktarTxt(_yonetici.Notlar, sfd.FileName);
            _lblDurum.Text = $"Tüm notlar TXT olarak kaydedildi.";
            MessageBox.Show($"{_yonetici.ToplamNotSayisi} adet not toplu metin belgesine yazıldı.", "Dışa Aktarma Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void VeriKlasorunuAc()
    {
        string dosyaYolu = _yonetici.VeriDosyaYolu;
        string? klasor = Path.GetDirectoryName(dosyaYolu);
        if (!string.IsNullOrEmpty(klasor) && Directory.Exists(klasor))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = klasor,
                UseShellExecute = true
            });
        }
        else
        {
            MessageBox.Show($"Veri klasörü: {dosyaYolu}", "Klasör Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SayaclariGuncelle()
    {
        if (_seciliNot == null)
        {
            _lblSayac.Text = "0 kelime, 0 karakter";
            return;
        }

        string metin = _txtIcerik.Text;
        int karakter = metin.Length;
        int kelime = string.IsNullOrWhiteSpace(metin)
            ? 0
            : metin.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;

        _lblSayac.Text = $"{kelime} kelime, {karakter} karakter";
    }

    private void FormuTemizle()
    {
        _yukleniyor = true;
        _txtBaslik.Text = string.Empty;
        _txtIcerik.Text = string.Empty;
        _cmbKategori.SelectedIndex = 0;
        _lblZaman.Text = "Not seçilmedi";
        _lblSayac.Text = "0 kelime, 0 karakter";
        _yukleniyor = false;
    }

    private static string GecerliDosyaAdi(string baslik)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            baslik = baslik.Replace(c, '_');
        }
        return string.IsNullOrWhiteSpace(baslik) ? "not" : baslik;
    }
}
