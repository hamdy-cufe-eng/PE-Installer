using ePOSOne.btnProduct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PEXInstaller
{
    public partial class Form1 :MetroFramework.Forms.MetroForm
    {
        DataClass Data = new DataClass(); 
        public Form1()
        {
            InitializeComponent();

     
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public void LoadConfigs()
        {
            try
            {
                if (this.Data != null)
                {
                    metroTextBox1.Text = Data.TermsOfService.ToString();
                    metroLabel2.Text = Data.NameInTerms.ToString();
                    pictureBox1.Image = Data.Logo;
                }
                else
                {
                    if (MetroFramework.MetroMessageBox.Show(this, "Something went wrong \n Error Message : Failed to retrive data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                        this.Close();
                }
            }
            catch(Exception ex)
            {
                if (MetroFramework.MetroMessageBox.Show(this, $"Something went wrong \n Error Message : Failed to load config \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    this.Close();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //test tt
            LoadConfigs();
        }
        private void metroButton1_Click_1(object sender, EventArgs e)
        {
            var frm = new Form2(this);
            frm.Show();
            this.Hide();
           // if (MetroFramework.MetroMessageBox.Show(this, "Something went wrong \n Error Message : Failed to run Executable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
               // this.Close();
        }

        private void button_WOC1_Click(object sender, EventArgs e)
        {
            var frm = new Form2(this);
            frm.Show();
            this.Hide();
        }

        private void button_WOC1_MouseHover(object sender, EventArgs e)
        {
            button_WOC1.OnHoverBorderColor = Color.White;
            button_WOC1.BorderColor = Color.White;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

    }
}
