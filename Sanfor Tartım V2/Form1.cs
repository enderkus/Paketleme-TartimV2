using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sanfor_Tartım_V2;
using System.Data.SqlClient;

namespace Sanfor_Tartım_V2
{
    public partial class Form1 : Form
    {
        SanforKantar sfk = new SanforKantar();
        


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {

              if(sfk.personelsorgula(textBox1.Text))
                {
                    if(textBox1.Text == "")
                    {
                        MessageBox.Show("LÜTFEN GEÇERLİ BİR PERSONEL KODU GİRİN !", "HATA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox1.Clear();
                        textBox1.Focus();
                    } else
                    {
                        sfk.personelbilgileri(textBox1.Text); // Personel sayımı sonucu olumlu sonuç döndü ve personel bilgilerini aldığımız metodunu çağırdık.
                        Form2 frm2 = new Form2(); // Form2 yi tanımladık.
                        this.Hide(); // Bu formu gizledik.
                        frm2.ShowDialog(); // Form2 yi ekrana çağırdık.
                    }

                   
                } else // Personel bilgisi bulunamadıysa yapılacak işlemler.
                {
                    textBox1.Clear(); // Textbox içerisindeki metni temizledik.
                    textBox1.Focus(); // Textbox içerisine geri dönüyoruz.
                }
               

            
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
