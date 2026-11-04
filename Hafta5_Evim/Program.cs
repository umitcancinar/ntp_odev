using System;
using System.Globalization;
using System.Text;

namespace Hafta5_Evim
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            Ev evim = new Ev();
            evim.UyeEkle(new Kisi("Anne", 1970, "Öğretmen"));
            evim.UyeEkle(new Kisi("Baba", 1968, "Mühendis"));
            evim.UyeEkle(new Kisi("Ümitcan", 2004, "Öğrenci"));
            evim.UyeEkle(new Kisi("Kardeş", 2008, "   "));

            Console.WriteLine("--- EV ÜYELERİ ---");
            evim.TumUyeleriYaz();
            Console.WriteLine();
            Console.WriteLine($"Ortalama yaş : {evim.OrtalamaYas():F1}");
            Console.Write("En yaşlı üye : ");
            evim.EnYasli().BilgiYaz();
            Console.WriteLine($"Toplam kişi : {Kisi.KisiSayisi}");
            Console.WriteLine($"Ev adresi : {Kisi.EvAdresi}");
        }
    }
}
