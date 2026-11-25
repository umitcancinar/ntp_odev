namespace NotepadApp.Core.Models;

/// <summary>
/// Notlar için standart kategoriler ve yardımcı listeler.
/// </summary>
public static class Kategori
{
    public const string Tumu = "Tümü";
    public const string Genel = "Genel";
    public const string Ders = "Ders";
    public const string Kisisel = "Kişisel";
    public const string Is = "İş";
    public const string Onemli = "Önemli";

    public static readonly string[] VarsayilanKategoriler =
    [
        Genel,
        Ders,
        Kisisel,
        Is,
        Onemli
    ];

    public static readonly string[] FiltreKategorileri =
    [
        Tumu,
        Genel,
        Ders,
        Kisisel,
        Is,
        Onemli
    ];
}
