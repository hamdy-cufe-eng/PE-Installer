using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using Microsoft.WindowsAPICodePack.Dialogs;
namespace PEXInstaller
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
       public static DataClass Data;
        Form1 Form1Instance;
        public Form2(Form1 frm)
        {
            InitializeComponent();
            Data = new DataClass();
            this.Form1Instance = frm;
        }
        public Task<string> Get_SelectedDir_Text()
        {
            return Task.Run(
        () => {
            return Data.SelectedDirectory.ToString();
        });
  
        }

        public void LoadConfigs()
        {

            try
            {
                if (Data != null)
                {
                    pictureBox1.Image = Data.Logo;
                }
                else
                {
                    if (MetroFramework.MetroMessageBox.Show(this, "Something went wrong \n Error Message#2 : Failed to retrive data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                        this.Close();
                }
            }
            catch (Exception ex)
            {
                if (MetroFramework.MetroMessageBox.Show(this, $"Something went wrong \n Error Message#2 : Failed to load config \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    this.Close();
            }
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            Data.SelectedDirectory = "C:\\Program Files (x86)";
            metroTextBox1.Text = Data.SelectedDirectory;
            LoadConfigs();
        }

        private void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Program Files (x86)";
            dialog.IsFolderPicker = true;
            dialog.Title = "";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Data.SelectedDirectory = dialog.FileName;
                Data.SelectedDirectory += "\\";
                metroTextBox1.Text = Data.SelectedDirectory;
                metroTextBox1.Enabled = false;
                button_WOC1.Enabled = true;
                metroLabel1.Text = "Path selected, click next to be processed to the install !";
                this.BringToFront();
                this.Activate();

            }
            else
            {

                MetroFramework.MetroMessageBox.Show(this, $" Error Message#2 : No file path selected , please do so .. \n ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void metroTextBox1_Click(object sender, EventArgs e)
        {
           
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            
          }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1 frm = (Form1)Application.OpenForms["Form1"];

            if (frm != null)
                frm.Close();



        }

        private void button_WOC1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(metroTextBox1.Text))
            {
                if (MetroFramework.MetroMessageBox.Show(this, $" Error Message#2 : Button override detected ... Closing Client ! \n ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    Form1 frm = (Form1)Application.OpenForms["Form1"];

                    if (frm != null)
                        frm.Close();

                    this.Close();
                }
            }
            else
            {

                var frm = new Form3(this);
                frm.Show();
                this.Hide();
            }
        }
    }
}
