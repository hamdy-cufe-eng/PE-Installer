using MetroFramework;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Xml.Linq;
using IWshRuntimeLibrary;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Configuration.Assemblies;
using System.Threading;
using System.Diagnostics;
using ePOSOne.btnProduct;

namespace PEXInstaller
{
    public partial class Form3 : MetroFramework.Forms.MetroForm
    {
        Form2 Form2Instance;
        string Store;
        bool IsAhmedSamirBuild = false;
        string BaseFileName = null;
        public Form3(Form2 frm)
        {
            InitializeComponent();
            this.Form2Instance = frm;


        }
        Unpack unpack = new Unpack();
        Deps deps = new Deps();
        string FinalPath;
        public async void doUnpack()
        {
            Logger.Info("[UNPACK]Unpack started proccessing for reading archives.. ");
            unpack.ReadPESection();

            Logger.Info("[UNPACK]Reading archives Done obtaining info.. ");

            FinalPath = await this.Form2Instance.Get_SelectedDir_Text();
            Logger.Info($"[UNPACK]Selected Path : {FinalPath}");
            if (unpack.GetProjectName().Contains("Samir") || unpack.GetProjectName().Contains("samir") || unpack.GetProjectName().Contains("ahmed"))
            {
                IsAhmedSamirBuild = true;
                BaseFileName = "MrAhmedSamir";
              

            }
            else
            {
                IsAhmedSamirBuild = false;
                BaseFileName = "MrOmarRaeia";

            }

            Logger.Info($"[UNPACK]Current Project Belonges to : {BaseFileName}");

            if (SystemControl.Services.IsInstalled(BaseFileName) == true)
            {
                Logger.Info($"[SERVICE_CONTROLLER]Service found with name: {BaseFileName} , proccessing for uninstalling ..");
                SystemControl.Services.Stop(BaseFileName, 0);
                SystemControl.Services.Uninstall(BaseFileName);
            }
            UninstallRegistry uninstallRegistry = new UninstallRegistry(unpack.GetProjectName(), Path.Combine(FinalPath, unpack.GetProjectName(), "Application", "Uninstaller.exe"), Path.Combine(FinalPath, unpack.GetProjectName(), "Application", BaseFileName + "Browser.exe"));
            Logger.Info($"[SERVICE_CONTROLLER]Uninstall Registry appended successfully");
            unpack.DecompressArchive(FinalPath);
            Logger.Info("[UNPACK]Archive Decompressed Done !");

        }
        public void CreateStartMenuShortcut()
        {
            try
            {
                metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = "Creating Start lnk.."; }));
                string pathToExe = FinalPath + "\\" + unpack.GetProjectName() + "\\Application\\" + BaseFileName + "Browser.exe";
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs");

            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);

            string shortcutLocation = Path.Combine(appStartMenuPath, BaseFileName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.TargetPath = pathToExe;
            shortcut.Save();
            }
              catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, $"Something went wrong \n Error Message : Failed to create Startmenu shortcut , processing without close \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Error($"Something went wrong \n Error Message : Failed to create Startmenu shortcut , processing without close \n {ex.Message}");
            }
        }
        public void CreateDesktopShortcut()
        {
            try
            {
                metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = "Creating Desktop lnk.."; }));
                object shortcutdesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string strshortcutpath = (string)shell.SpecialFolders.Item(ref shortcutdesktop) + @"\" + BaseFileName + ".lnk";
                IWshShortcut shortcut = shell.CreateShortcut(strshortcutpath);
                shortcut.TargetPath = FinalPath + "\\" + unpack.GetProjectName() + "\\Application\\" + BaseFileName + "Browser.exe";
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, $"Something went wrong \n Error Message : Failed to create desktop shortcut , processing without close \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Error($"Something went wrong \n Error Message : Failed to create desktop shortcut , processing without close \n {ex.Message}");
            }
        }
        public void PreformServiceTask()
        {

            try
            {

                    metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = "Starting service.."; }));
                string Service_Path = FinalPath + "\\" + unpack.GetProjectName() + @"\\Service\\" + BaseFileName + ".Service.exe";

                if (SystemControl.Services.IsInstalled(BaseFileName) == false)
                {
                    SystemControl.Services.InstallAndStart(
                                                    BaseFileName,
                                                    BaseFileName + " Service",
                                                    @"""" + Service_Path + @""" -k runservice"
                     );
                }
                else
                {

                    SystemControl.Services.Stop(BaseFileName, 0);
                    SystemControl.Services.Uninstall(BaseFileName);

                    Thread.Sleep(500);

                    SystemControl.Services.InstallAndStart(
                                               BaseFileName,
                                              BaseFileName + " Service",
                                               @"""" + Service_Path + @""" -k runservice"
                );

                }
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, $"Something went wrong \n Error Message : Failed to Start Service , processing without close \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Logger.Error($"Something went wrong \n Error Message : Failed to Start Service , processing without close \n {ex.Message}");
            }

        }
        private void Form3_Load(object sender, EventArgs e)
        { 
            if (Directory.Exists(this.Form2Instance.Get_SelectedDir_Text().Result) == false)
              Directory.CreateDirectory(Path.GetDirectoryName(this.Form2Instance.Get_SelectedDir_Text().Result));
            Task task1 = new Task(doUnpack);
            task1.Start();

        
        }
        bool DoOnFinishOnce = true;
        public void UpdateText(string txt, int perc , int DecFont = 0 ,bool AllFileExtracted = false )
        {
            try
            {
                if (label1.InvokeRequired)
                {
                    label1.Invoke(new MethodInvoker(delegate { label1.Text = string.Concat(perc.ToString(), "%"); }));
                }
                if (metroLabel3.InvokeRequired)
                {
                    metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = txt.ToString(); }));
                }

                if (metroProgressBar1.InvokeRequired)
                {
                    metroProgressBar1.Invoke(new MethodInvoker(delegate { metroProgressBar1.Value = perc; }));
                }

                if (DecFont == 0)
                {
                    if (metroLabel3.InvokeRequired)
                    {
                        metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = " Installation Completed Successfully"; }));
                    }
                    if (metroLabel5.InvokeRequired)
                    {
                        metroLabel5.Invoke(new MethodInvoker(delegate {
                            metroLabel5.Text = "Please Click Start to open the application !";
                            metroLabel5.ForeColor = Color.DarkGreen;
                        }));
                    }

                    button_WOC1.Invoke(new MethodInvoker(delegate { button_WOC1.Visible = true; }));
                    if (MetroFramework.MetroMessageBox.Show(this, $"Installation Completed Successfully !\n Do you want to start the application now?", "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start(FinalPath + "\\" + unpack.GetProjectName() + "\\Application\\" + BaseFileName + "Browser.exe");
                        Form1 frm = (Form1)Application.OpenForms["Form1"];
                        Form2 frm2 = (Form2)Application.OpenForms["Form2"];

                        if (frm != null)
                            frm.Close();
                        if (frm2 != null)
                            frm2.Close();
                    }
                    else
                    {
                        Form1 frm = (Form1)Application.OpenForms["Form1"];
                        Form2 frm2 = (Form2)Application.OpenForms["Form2"];

                        if (frm != null)
                            frm.Close();
                        if (frm2 != null)
                            frm2.Close();


                    }


                }
                else
                {
                    if (metroLabel3.InvokeRequired)
                    {
                        metroLabel3.Invoke(new MethodInvoker(delegate
                        {
                            metroLabel3.FontSize = MetroLabelSize.Small;
                            metroLabel3.Location = new Point(219, 123);
                        }));
                    }

                }
                if (perc == 100 && AllFileExtracted == true)
                {

                    if (DoOnFinishOnce == true)
                    {
                        CreateDesktopShortcut();
                        CreateStartMenuShortcut();
                        PreformServiceTask();

                        if (metroLabel3.InvokeRequired)
                        {
                            metroLabel3.Invoke(new MethodInvoker(delegate { metroLabel3.Text = " Installation Completed Successfully"; }));
                        }

                        if (metroLabel5.InvokeRequired)
                        {
                            metroLabel5.Invoke(new MethodInvoker(delegate { metroLabel5.Text = "Please Click Start to open the application !";
                                metroLabel5.ForeColor = Color.DarkGreen;
                            }));
                        }

                        button_WOC1.Invoke(new MethodInvoker(delegate { button_WOC1.Visible = true; }));
                        if (MetroFramework.MetroMessageBox.Show(this, $"Installation Completed Successfully !\n Do you want to start the application now?", "Done", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            Process.Start(FinalPath + "\\" + unpack.GetProjectName() + "\\Application\\" + BaseFileName + "Browser.exe");
                            Form1 frm = (Form1)Application.OpenForms["Form1"];
                            Form2 frm2 = (Form2)Application.OpenForms["Form2"];

                            if (frm != null)
                                frm.Close();
                            if (frm2 != null)
                                frm2.Close();
                        }
                        else
                        {
                            Form1 frm = (Form1)Application.OpenForms["Form1"];
                            Form2 frm2 = (Form2)Application.OpenForms["Form2"];

                            if (frm != null)
                                frm.Close();
                            if (frm2 != null)
                                frm2.Close();


                        }

                        DoOnFinishOnce = false;
                    }
                }
            }
            catch (Exception ex) { Logger.Error($"Something went wrong \n Error Message : Failed to Update Text , processing without close \n {ex.Message}"); }

            }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1 frm = (Form1)Application.OpenForms["Form1"];
            Form2 frm2 = (Form2)Application.OpenForms["Form2"];

            if (frm != null)
                frm.Close();
            if (frm2 != null)
                frm2.Close();
        }

  

        private void timer1_Tick(object sender, EventArgs e)
        {
            metroTextBox1.Text = FinalPath;
        }

        private void metroLabel3_Click(object sender, EventArgs e)
        {

        }

        private void metroLabel5_Click(object sender, EventArgs e)
        {

        }

        private void button_WOC1_Click(object sender, EventArgs e)
        {
            Process.Start(FinalPath + "\\" + unpack.GetProjectName() + "\\Application\\" + BaseFileName + "Browser.exe");
        }
    }
}
