using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using System.Windows.Forms;
using Sanfor_Tartım_V2;
using System.Diagnostics.Eventing.Reader;
using RawDataPrint;
using System.IO;
using Sanfor_Tartım_V2.DRL;

namespace Sanfor_Tartım_V2
{
    class SanforKantar
    {


        


        Relation relation = new Relation();

        PartiBilgileri pb = new PartiBilgileri();
        SqlConnection con;
        SqlCommand cmd;
        public string[] parcalar;
        public void baglanti() {
            
            con = new SqlConnection("Server=192.168.10.250;Database=SentezLive;Uid=sa;Password=boyteks123***;");
            cmd = new SqlCommand();
            cmd.Connection = con;
            
        }

        public bool personelsorgula(string personelkodu)
        {
         
            // Personel kodu SPR*** şeklinde geliyor bu yüzden baştaki S harfini boşluk ile değiştirip daha sonra trim ile boşlukları temizledik.
            string personelkodu2 = personelkodu.Replace('S',' ').Trim();
            // Burada sayım sorgusu sonrası kontrol yapıyoruz.
            
            if(relation.personelSorgula(personelkodu2)) {
                // Eğer dönen bilgi 0 dan büyükse true döndürüyoruz.
                return true;
            } else
            {
                // Eğer dönen değeri 0 dan büyük değilse hata bastıracak ve false döndürecek.
                MessageBox.Show("GEÇERSİZ BİR PERSONEL KODU GİRDİNİZ !","HATA !", MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
        }

        public void personelbilgileri(string personelkodu) {
           
            relation.personelGetir(personelkodu.Replace('S', ' ').Trim());
          
        }


        public void KullanicidanGelen(string kullaniciverisi)
        {
            if(kullaniciverisi.Length == 12)
            {
                
                // Kullanıcıdan gelen 02-200123-1 gibi pari ve satır numarasını aldık ve parçaladık.
                 parcalar = kullaniciverisi.Split('-');
                 PartiBilgileri.PartiNo = parcalar[0] + "-" + parcalar[1];
                 PartiBilgileri.SatirNo = Convert.ToInt32(parcalar[2]);
                 relation.PartiGetir();
            } else if(kullaniciverisi.Substring(0,6) == "etiket")
            {
                PartiBilgileri.EtiketAdi = kullaniciverisi;
            } 
        }

    }
}
