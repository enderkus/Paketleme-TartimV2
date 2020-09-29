using RawDataPrint;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Sanfor_Tartım_V2.DRL
{
    class Relation
    {
        public string StringReplace(string text)
        {
            text = text.Replace("İ", "I");
            text = text.Replace("ı", "i");
            text = text.Replace("Ğ", "G");
            text = text.Replace("ğ", "g");
            text = text.Replace("Ö", "O");
            text = text.Replace("ö", "o");
            text = text.Replace("Ü", "U");
            text = text.Replace("ü", "u");
            text = text.Replace("Ş", "S");
            text = text.Replace("ş", "s");
            text = text.Replace("Ç", "C");
            text = text.Replace("ç", "c");
            return text;
        }

        SqlConnection conn;
        SqlCommand sqlcmd;

        public Relation()
        {
        }
        private void ConnectDB() //db bağlantı
        {
            conn = new SqlConnection(Properties.Settings.Default.baglanti);
            sqlcmd = new SqlCommand();
            sqlcmd.Connection = conn;
        }
        private void OpenConnection(string sqltext)
        {
            ConnectDB();
            sqlcmd.CommandText = sqltext;
            sqlcmd.CommandType = CommandType.Text;
            conn.Open();
        }
        private void CloseConnection()
        {
            conn.Close();
            sqlcmd.Dispose();
        }

        public bool personelSorgula(string personelKodu)
        {
            ConnectDB();
            OpenConnection("SELECT COUNT(RecId) FROM Erp_Employee WHERE EmployeeCode = @perkod");
            sqlcmd.Parameters.AddWithValue("perkod", personelKodu);
            object donen = sqlcmd.ExecuteScalar();
            if (Convert.ToInt32(donen) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            conn.Close();
            sqlcmd.Dispose();
        }

        public void personelGetir(string personelKodu)
        {
            ConnectDB();
            OpenConnection("SELECT * FROM Erp_Employee WHERE EmployeeCode = @perkod");
            sqlcmd.Parameters.AddWithValue("perkod", personelKodu);
            SqlDataReader personelbilgileri = sqlcmd.ExecuteReader();

            while (personelbilgileri.Read())
            {
                // Sql'den gelen personel RecId(Kayıt değeri) bilgisini personelid değişkenine atıyoruz.
                Personel.Personelid = Convert.ToInt32(personelbilgileri["RecId"]);
                // Sql'den gelen personel adı bilgisini personeladi değişkenine aktarıyoruz.
                Personel.personeladi = personelbilgileri["EmployeeName"].ToString();
                // Sql'den gelen personel soyadı bilgisini personelsoyadi değişkenine aktarıyoruz.
                Personel.personelsoyadi = personelbilgileri["EmployeeSurname"].ToString();
            }
            conn.Close();
            sqlcmd.Dispose();
        }

        public void PartiGetir()
        {
            ConnectDB();
            OpenConnection("SELECT * FROM Erp_WorkOrder WHERE WorkOrderNo = @workorderno");
            sqlcmd.Parameters.AddWithValue("workorderno",PartiBilgileri.PartiNo);
            SqlDataReader partibilgileri = sqlcmd.ExecuteReader();

            while (partibilgileri.Read())
            {
                PartiBilgileri.PartiId = Convert.ToInt32(partibilgileri["RecId"]);
                PartiBilgileri.MusteriId = Convert.ToInt32(partibilgileri["CurrentAccountId"]);
                PartiBilgileri.SiparisNo = partibilgileri["CustomerOrderNo"].ToString();
                PartiBilgileri.ReceteId = Convert.ToInt32(partibilgileri["LabRecipeId"]);
            }

            partibilgileri.Close();

            conn.Close();
            sqlcmd.Dispose();
            satirGetir();
        }

        public void satirGetir()
        {
            ConnectDB();
            OpenConnection("SELECT * FROM Erp_WorkOrderItem WHERE WorkOrderId = @workorderid AND ItemOrderNo = @itemorderno");
            sqlcmd.Parameters.AddWithValue("workorderid", PartiBilgileri.PartiId);
            sqlcmd.Parameters.AddWithValue("itemorderno", PartiBilgileri.SatirNo);
            SqlDataReader satirbilgileri = sqlcmd.ExecuteReader();

            while (satirbilgileri.Read())
            {
                PartiBilgileri.SatirId = Convert.ToInt32(satirbilgileri["RecId"]);
                PartiBilgileri.KumasId = Convert.ToInt32(satirbilgileri["InventoryId"]);
                PartiBilgileri.En = Convert.ToInt32(satirbilgileri["FabricGram"]);
                PartiBilgileri.Gramaj = Convert.ToInt32(satirbilgileri["FabricWidth"]);
            }
            musteriGetir();
        }

        public void musteriGetir()
        {
            ConnectDB();
            OpenConnection("SELECT CurrentAccountName FROM Erp_CurrentAccount WHERE RecId = @musteriid");
            sqlcmd.Parameters.AddWithValue("musteriid", PartiBilgileri.MusteriId);
            SqlDataReader musteribilgileri = sqlcmd.ExecuteReader();

            while (musteribilgileri.Read())
            {
                PartiBilgileri.MusteriAdi = musteribilgileri["CurrentAccountName"].ToString();
            }
            kumasGetir();

        }

        public void kumasGetir()
        {
            ConnectDB();
            OpenConnection("SELECT * FROM Erp_Inventory WHERE RecId = @kumasid");
            sqlcmd.Parameters.AddWithValue("kumasid", PartiBilgileri.KumasId);
            SqlDataReader kumasbilgileri = sqlcmd.ExecuteReader();

            while (kumasbilgileri.Read())
            {
                PartiBilgileri.KumasAdi = kumasbilgileri["InventoryName"].ToString();
            }
            renkGetir();
        }

        public void renkGetir()
        {
            ConnectDB();
            OpenConnection("SELECT * FROM Erp_LabRecipe WHERE RecId = @receteid");
            sqlcmd.Parameters.AddWithValue("receteid", PartiBilgileri.ReceteId);
            SqlDataReader renkbilgileri = sqlcmd.ExecuteReader();

            while (renkbilgileri.Read())
            {
                PartiBilgileri.RenkKodu = renkbilgileri["LabRecipeCode"].ToString();
                PartiBilgileri.RenkAdi = renkbilgileri["LabRecipeName"].ToString();
            }
        }

        public void numuneYazdir(string etiket)
        {

            String tempFile = "etiket1.prn";
            String PrinterName = Properties.Settings.Default.yazici.ToString();
            StreamReader SR = new StreamReader(tempFile, Encoding.Default);
            String all = SR.ReadToEnd();
            SR.Close();
            all = all.Replace("{CariAdi}", StringReplace(PartiBilgileri.MusteriAdi));
            all = all.Replace("{StokAdi}", StringReplace(PartiBilgileri.KumasAdi));
            all = all.Replace("{PartiNo}", PartiBilgileri.PartiNo);
            all = all.Replace("{PartiSatir}", PartiBilgileri.SatirNo.ToString());
            all = all.Replace("{RenkAdi}", StringReplace(PartiBilgileri.RenkAdi));
            all = all.Replace("{RenkKodu}", StringReplace(PartiBilgileri.RenkKodu));
            all = all.Replace("{En}", PartiBilgileri.En.ToString());
            all = all.Replace("{Grm}", PartiBilgileri.Gramaj.ToString());
            all = all.Replace("{BrutYazi}", "0");
            all = all.Replace("{NetYazi}", "0");
            all = all.Replace("{SiparisNo}", StringReplace(PartiBilgileri.SiparisNo));
            all = all.Replace("{SysTarih}", DateTime.Now.ToString());
            all = all.Replace("{TopSira}", "0");
            RawPrinterHelper.SendStringToPrinter(PrinterName, all);
            // Gelen metin numune ise numune etiketi yazdıracağımız alan
        }

        public void etiketKaydet(float dara, float kilo, float hesaplanan,string etiketadi)
        {
            // Son top numarasını alıyoruz.
            ConnectDB();
            OpenConnection("SELECT COUNT(OrderNo) FROM Erp_InventorySerialCard WHERE WorkOrderReceiptItemId = @satir");
            sqlcmd.Parameters.AddWithValue("satir",PartiBilgileri.SatirId);
            object topnodonen = sqlcmd.ExecuteScalar();

            int topsayint = Convert.ToInt32(topnodonen);
            int toparttir = topsayint + 1;

            sqlcmd.Dispose();
            CloseConnection();

            // Son top no alma işlemi bitti.

            // Toplam serialcard sayısını alıyoruz 

            ConnectDB();
            OpenConnection("SELECT MAX (SerialCode) FROM Erp_InventorySerialCard");
            object toplamserialcard = sqlcmd.ExecuteScalar();
            int toplamserialcardint = Convert.ToInt32(toplamserialcard) + 1;

            sqlcmd.Dispose();
            CloseConnection();
            // Toplam serialcard sayısı alma işlemi bitti


            // Top kayıt işlemi başladı.
            ConnectDB();
            OpenConnection("INSERT INTO Erp_InventorySerialCard (CompanyId,SerialCode,InventoryId,Quantity,ManufacturingDate,EmployeeId,WorkOrderId,WorkOrderReceiptItemId,ResourceId,InUse,InsertedAt,InsertedBy,OrderNo,PartNumber,GrossQuantity,NetQuantity,TareQuantity,UD_SiraNo) VALUES (@companyid,@serialcode,@inventoryid,@tartilan,@manufacturingdate,@employeeid,@workorderid,@workorderrei,@resourceid,@inuse,@i_at,@i_by,@orderno,@partnumber,@grossq,@netq,@tareq,@sirano)");
            sqlcmd.Parameters.AddWithValue("companyid",1);
            sqlcmd.Parameters.AddWithValue("serialcode", toplamserialcardint.ToString().PadLeft(8, '0'));
            sqlcmd.Parameters.AddWithValue("inventoryid", PartiBilgileri.KumasId);
            sqlcmd.Parameters.AddWithValue("tartilan", Convert.ToDecimal(kilo));
            sqlcmd.Parameters.AddWithValue("manufacturingdate", DateTime.Now);
            sqlcmd.Parameters.AddWithValue("employeeid", Personel.Personelid);
            sqlcmd.Parameters.AddWithValue("workorderid", PartiBilgileri.PartiId);
            sqlcmd.Parameters.AddWithValue("workorderrei", PartiBilgileri.SatirId);
            sqlcmd.Parameters.AddWithValue("resourceid", Properties.Settings.Default.makineid);
            sqlcmd.Parameters.AddWithValue("inuse", 1);
            sqlcmd.Parameters.AddWithValue("i_at", DateTime.Now);
            sqlcmd.Parameters.AddWithValue("i_by", Personel.Personelid);
            sqlcmd.Parameters.AddWithValue("orderno", toparttir);
            sqlcmd.Parameters.AddWithValue("partnumber", 1);
            sqlcmd.Parameters.AddWithValue("grossq", Convert.ToDecimal(kilo));
            sqlcmd.Parameters.AddWithValue("netq", Convert.ToDecimal(hesaplanan));
            sqlcmd.Parameters.AddWithValue("tareq", Convert.ToDecimal(dara));
            sqlcmd.Parameters.AddWithValue("sirano", toparttir.ToString());
            sqlcmd.ExecuteNonQuery();

            sqlcmd.Dispose();
            CloseConnection();

            // Top kayıt işlemi bitti.


            String tempFile = etiketadi + ".prn";
            String PrinterName = Properties.Settings.Default.yazici.ToString();
            StreamReader SR = new StreamReader(tempFile, Encoding.Default);
            String all = SR.ReadToEnd();
            SR.Close();
            all = all.Replace("{CariAdi}", StringReplace(PartiBilgileri.MusteriAdi));
            all = all.Replace("{StokAdi}", StringReplace(PartiBilgileri.KumasAdi));
            all = all.Replace("{PartiNo}", PartiBilgileri.PartiNo);
            all = all.Replace("{PartiSatir}", PartiBilgileri.SatirNo.ToString());
            all = all.Replace("{RenkAdi}", StringReplace(PartiBilgileri.RenkAdi));
            all = all.Replace("{RenkKodu}", StringReplace(PartiBilgileri.RenkKodu));
            all = all.Replace("{En}", PartiBilgileri.En.ToString());
            all = all.Replace("{Grm}", PartiBilgileri.Gramaj.ToString());
            all = all.Replace("{BrutYazi}", kilo.ToString());
            all = all.Replace("{NetYazi}", hesaplanan.ToString());
            all = all.Replace("{SiparisNo}", StringReplace(PartiBilgileri.SiparisNo));
            all = all.Replace("{SysTarih}", DateTime.Now.ToString());
            all = all.Replace("{TopSira}", toparttir.ToString());
            RawPrinterHelper.SendStringToPrinter(PrinterName, all);
            // Gelen metin numune ise numune etiketi yazdıracağımız alan


        }






    }


}
