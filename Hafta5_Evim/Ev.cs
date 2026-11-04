using System;
using System.Collections.Generic;

namespace Hafta5_Evim
{
    public class Ev
    {
        private List<Kisi> uyeler = new List<Kisi>();

        public void UyeEkle(Kisi kisi)
        {
            if (kisi == null)
                throw new ArgumentNullException(nameof(kisi));
            uyeler.Add(kisi);
        }

        public void TumUyeleriYaz()
        {
            foreach (Kisi kisi in uyeler)
                kisi.BilgiYaz();
        }

        public double OrtalamaYas()
        {
            if (uyeler.Count == 0)
                return 0;

            int toplam = 0;
            foreach (Kisi kisi in uyeler)
                toplam += kisi.YasHesapla();
            return (double)toplam / uyeler.Count;
        }

        public Kisi EnYasli()
        {
            if (uyeler.Count == 0)
                throw new InvalidOperationException("Evde henüz üye yok.");

            Kisi enYasli = uyeler[0];
            foreach (Kisi kisi in uyeler)
            {
                if (kisi.YasHesapla() > enYasli.YasHesapla())
                    enYasli = kisi;
            }
            return enYasli;
        }
    }
}
