using System;

namespace Hafta5_Evim
{
    public class Kisi
    {
        public static int KisiSayisi = 0;
        public const string EvAdresi =
            "Örnek Mahallesi Öğrenci Sokak No 5 Elazığ";

        private int dogumYili;
        private string meslek = "Belirtilmedi";

        public string Ad { get; set; }

        public string Meslek
        {
            get { return meslek; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    meslek = "Belirtilmedi";
                else
                    meslek = value.Trim();
            }
        }

        public int DogumYili
        {
            get { return dogumYili; }
            set
            {
                if (value > 1900 && value <= DateTime.Now.Year)
                    dogumYili = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Doğum yılı 1900'den büyük ve mevcut yıldan " +
                        "küçük veya eşit olmalıdır.");
            }
        }

        public Kisi(string ad, int dogumYili, string meslek)
        {
            Ad = ad;
            DogumYili = dogumYili;
            Meslek = meslek;
            KisiSayisi++;
        }

        // Gelecek haftaya hazırlık: aynı sınıfta ikinci bir yapıcı metot.
        public Kisi(string ad) : this(ad, DateTime.Now.Year, "Belirtilmedi")
        {
        }

        public int YasHesapla()
        {
            return DateTime.Now.Year - dogumYili;
        }

        public void BilgiYaz()
        {
            Console.WriteLine($"{Ad} - {Meslek} - {YasHesapla()} yaşında");
        }
    }
}
