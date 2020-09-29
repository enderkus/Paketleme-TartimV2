using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sanfor_Tartım_V2;
using System.Threading;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using System.IO.Ports;
using Sanfor_Tartım_V2.DRL;

namespace Sanfor_Tartım_V2
{
    public partial class Form2 : Form
    {
        SerialPort sp = new SerialPort(Properties.Settings.Default.comport, 9600, Parity.None, 8, StopBits.One);
        Relation relation = new Relation();
        SanforKantar sfk = new SanforKantar();
        SqlConnection con;
        SqlCommand cmd;
        int durum = 0;
        public void satirGetir()
        {
            con = new SqlConnection("Server=192.168.10.250;Database=SentezLive;Uid=sa;Password=boyteks123***;");
            cmd = new SqlCommand();
            cmd.Connection = con;

            // Burada datable içerisine o iş emrine ait satırları getirme işini yüklüyoruz.


            con.Open();
            cmd.CommandText = "SELECT OrderNo,replace(str(GrossQuantity, 10, 2), ' ', '') AS GrossQuantity,replace(str(NetQuantity, 10, 2), ' ', '') AS NetQuantity,replace(str(TareQuantity, 10, 2), ' ', '') AS TareQuantity from Erp_InventorySerialCard WHERE WorkOrderId = @woid  AND WorkOrderReceiptItemId = @sid";
            cmd.Parameters.AddWithValue("woid", PartiBilgileri.PartiId);
            cmd.Parameters.AddWithValue("sid", PartiBilgileri.SatirId);
            cmd.CommandType = CommandType.Text;

            //musteriler tablosundaki tüm kayıtları çekecek olan sql sorgusu.

            //Sorgumuzu ve baglantimizi parametre olarak alan bir SqlCommand nesnesi oluşturuyoruz.
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            //SqlDataAdapter sınıfı verilerin databaseden aktarılması işlemini gerçekleştirir.
            DataTable dt = new DataTable();
            da.Fill(dt);
            //Bir DataTable oluşturarak DataAdapter ile getirilen verileri tablo içerisine dolduruyoruz.
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].HeaderText = "TOP NO";
            dataGridView1.Columns[1].HeaderText = "TOPLAM KİLO";
            dataGridView1.Columns[2].HeaderText = "NET KİLO";
            dataGridView1.Columns[3].HeaderText = "DARA";
            con.Close();
        }


        public Form2()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            sp.Close();
            if (sp.IsOpen == false){
                sp.Open();
            }
            
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            personelid.Text = Personel.Personelid.ToString();
            personeladi.Text = Personel.personeladi;
            personelsoyadi.Text = Personel.personelsoyadi;
            saat.Text = "SAAT :" + DateTime.Now.ToShortTimeString();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            saat.Text = "SAAT :" + DateTime.Now.ToShortTimeString();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                
                if(textBox1.Text.Length == 12)
                {
                    sfk.KullanicidanGelen(textBox1.Text);
                    partiText.Text = PartiBilgileri.PartiNo;
                    satirText.Text = PartiBilgileri.SatirNo.ToString();
                    musteriText.Text = PartiBilgileri.MusteriAdi;
                    enText.Text = PartiBilgileri.En.ToString();
                    gramajText.Text = PartiBilgileri.Gramaj.ToString();
                    kumasText.Text = PartiBilgileri.KumasAdi;
                    renkKoduText.Text = PartiBilgileri.RenkKodu;
                    renkAdiText.Text = PartiBilgileri.RenkAdi;
                    
                    satirGetir();
                    textBox1.Clear();
                    textBox1.Focus();
                    
                }
                 else if (textBox1.Text == "NUMUNE")
                {
                    if(satirText.Text != "0") {
                    relation.numuneYazdir(etiketText.Text);
                    textBox1.Clear();
                    textBox1.Focus();
                    } else
                    {
                        MessageBox.Show("LÜTFEN ÖNCELİKLE İŞ EMRİ OKUTUN !","HATA", MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        textBox1.Clear();
                        textBox1.Focus();
                    }

                } else if (textBox1.Text == "DARA")
                {
                    durum = 1;
                    textBox1.Clear();
                    textBox1.Focus();
                }
                else if (textBox1.Text.Substring(0, 6) == "etiket")
                {
                    sfk.KullanicidanGelen(textBox1.Text);
                    etiketText.Text = PartiBilgileri.EtiketAdi;
                    textBox1.Clear();
                    textBox1.Focus();
                }





            } 
        }




        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Durum 1 olduğunda dara yazacak.
            if(durum == 1)
            {
                string gelenveri2 = sp.ReadLine();


                if (gelenveri2[0] == '+' || gelenveri2.Contains("kg"))
                {

                    string gelenveribastansil = gelenveri2.Substring(4);
                    string gelenverisondansil = gelenveribastansil.Substring(0, 5);
                    daraText.Text = gelenverisondansil;
                    durum = 0;
                }
            } else if (durum == 0)
            {
                // Durum 0 olduğunda direkt tartım yapacak.
                //Tartım gerçekleştiğinde yapılacak işlemler.
                float dara = float.Parse(daraText.Text.Replace('.',','));
                float gelenkilo;
                string gelenveri2 = sp.ReadLine();
                if (gelenveri2[0] == '+' && gelenveri2.Contains("kg"))
                {

                    string gelenveribastansil = gelenveri2.Substring(4);
                    string gelenverisondansil = gelenveribastansil.Substring(0,5);
                    brutText.Text = gelenverisondansil;
                    gelenkilo = float.Parse(gelenverisondansil.Replace('.',','));

                    // Gelen kilogram bilgisinin dara düşerek hesaplanmış hali.
                    float darahesapla = gelenkilo - dara;

                    relation.etiketKaydet(dara, gelenkilo, darahesapla,etiketText.Text);
                    satirGetir();




                }
            }


        }




    }
}
