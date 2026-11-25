using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using NotepadApp.Core.Models;

namespace NotepadApp.Core.Services;

/// <summary>
/// Notların JSON olarak serileştirilmesi, geri yüklenmesi ve TXT olarak dışa aktarılmasını yöneten servis sınıfı.
/// </summary>
public static class NotDepolamaServisi
{
    private static readonly JsonSerializerOptions JsonSecenekleri = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Not listesini belirtilen dosya yoluna JSON formatında serileştirerek kaydeder.
    /// </summary>
    public static void KaydetJson(IEnumerable<Not> notlar, string dosyaYolu)
    {
        ArgumentNullException.ThrowIfNull(notlar);
        if (string.IsNullOrWhiteSpace(dosyaYolu))
            throw new ArgumentException("Dosya yolu boş olamaz.", nameof(dosyaYolu));

        var dizin = Path.GetDirectoryName(dosyaYolu);
        if (!string.IsNullOrEmpty(dizin) && !Directory.Exists(dizin))
        {
            Directory.CreateDirectory(dizin);
        }

        string jsonMetni = JsonSerializer.Serialize(notlar, JsonSecenekleri);
        File.WriteAllText(dosyaYolu, jsonMetni, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Belirtilen JSON dosyasından not listesini geri yükler (deserialization).
    /// Dosya yoksa veya bozuksa güvenli şekilde boş liste döner veya durumu bildirir.
    /// </summary>
    public static List<Not> YukleJson(string dosyaYolu)
    {
        if (string.IsNullOrWhiteSpace(dosyaYolu) || !File.Exists(dosyaYolu))
        {
            return [];
        }

        try
        {
            string jsonMetni = File.ReadAllText(dosyaYolu, System.Text.Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(jsonMetni))
            {
                return [];
            }

            var liste = JsonSerializer.Deserialize<List<Not>>(jsonMetni, JsonSecenekleri);
            return liste ?? [];
        }
        catch (JsonException)
        {
            // Bozuk/hatalı JSON dosyasında uygulamanın çökmesini önler
            return [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    /// <summary>
    /// Tek bir notu okunabilir başlık, kategori ve tarih üst bilgileriyle .txt formatında dışa aktarır.
    /// </summary>
    public static void DisaAktarTxt(Not not, string dosyaYolu)
    {
        ArgumentNullException.ThrowIfNull(not);
        if (string.IsNullOrWhiteSpace(dosyaYolu))
            throw new ArgumentException("Dosya yolu boş olamaz.", nameof(dosyaYolu));

        var dizin = Path.GetDirectoryName(dosyaYolu);
        if (!string.IsNullOrEmpty(dizin) && !Directory.Exists(dizin))
        {
            Directory.CreateDirectory(dizin);
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine($"BAŞLIK         : {not.Baslik}");
        sb.AppendLine($"KATEGORİ       : {not.Kategori}");
        sb.AppendLine($"OLUŞTURULMA    : {not.OlusturmaTarihi:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"SON DÜZENLEME  : {not.DuzenlemeTarihi:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"İSTATİSTİK     : {not.KelimeSayisi} kelime, {not.KarakterSayisi} karakter");
        sb.AppendLine("==================================================");
        sb.AppendLine();
        sb.AppendLine(not.Icerik);

        File.WriteAllText(dosyaYolu, sb.ToString(), System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Tüm notları tek bir toplu .txt belgesi olarak dışa aktarır.
    /// </summary>
    public static void TumunuDisaAktarTxt(IEnumerable<Not> notlar, string dosyaYolu)
    {
        ArgumentNullException.ThrowIfNull(notlar);
        if (string.IsNullOrWhiteSpace(dosyaYolu))
            throw new ArgumentException("Dosya yolu boş olamaz.", nameof(dosyaYolu));

        var dizin = Path.GetDirectoryName(dosyaYolu);
        if (!string.IsNullOrEmpty(dizin) && !Directory.Exists(dizin))
        {
            Directory.CreateDirectory(dizin);
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("YMH219 NOT DEFTERİ — TOPLU NOT DIŞA AKTARIMI");
        sb.AppendLine($"DIŞA AKTARMA TARİHİ: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine("==================================================");
        sb.AppendLine();

        int sira = 1;
        foreach (var not in notlar)
        {
            sb.AppendLine($"--- [NOT #{sira++}] {not.Baslik} ({not.Kategori}) ---");
            sb.AppendLine($"Tarih: {not.DuzenlemeTarihi:dd.MM.yyyy HH:mm} | {not.KelimeSayisi} kelime");
            sb.AppendLine();
            sb.AppendLine(not.Icerik);
            sb.AppendLine();
            sb.AppendLine(new string('-', 50));
            sb.AppendLine();
        }

        File.WriteAllText(dosyaYolu, sb.ToString(), System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Uygulamanın varsayılan not veritabanı dosya yolunu döndürür.
    /// </summary>
    public static string GetVarsayilanVeriYolu()
    {
        string anaKlasor = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(anaKlasor))
        {
            anaKlasor = AppDomain.CurrentDomain.BaseDirectory;
        }

        return Path.Combine(anaKlasor, "YMH219-NotepadApp", "notlar.json");
    }
}
