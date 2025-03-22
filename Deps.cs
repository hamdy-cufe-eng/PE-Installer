using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace PEXInstaller
{
    public class Deps
    {
        public Deps() 
        {
            if (!DotNetCheck()) 
            {
                DownloadAndInstall("https://download.visualstudio.microsoft.com/download/pr/1f5af042-d0e4-4002-9c59-9ba66bcf15f6/124d2afe5c8f67dfa910da5f9e3db9c1/ndp472-kb4054531-web.exe", "dotnetfx.exe");
            }
            if (!VCCheck()) 
            {
                DownloadAndInstall("https://download.visualstudio.microsoft.com/download/pr/d60aa805-26e9-47df-b4e3-cd6fcc392333/A06AAC66734A618AB33C1522920654DDFC44FC13CAFAA0F0AB85B199C3D51DC0/VC_redist.x86.exe", "VC_redist.x86.exe");
            }
        }
        public bool DotNetCheck()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    Logger.Debug($".NET Framework Version: {CheckFor47Version((int)ndpKey.GetValue("Release"))}");
                    if(CheckFor47Version((int)ndpKey.GetValue("Release")) != null) 
                    {
                        return true;
                    }
                    return false;
                    
                }
                else
                {
                    Logger.Debug(".NET Framework Version 4.5 or later is not detected.");
                    return false;
                }
            }

        }

        string CheckFor47Version(int releaseKey)
        {
            if (releaseKey >= 533320)
                return "4.8.1 or later";
            if (releaseKey >= 528040)
                return "4.8";
            if (releaseKey >= 461808)
                return "4.7.2";

            return null;
        }

        public bool VCCheck()
        {
            const string subkey = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Version") != null)
                {
                    Logger.Debug($"VC++ Redistributable Version: {ndpKey.GetValue("Version")}");

                    return CheckForVCVersion((string)ndpKey.GetValue("Version"));

                }
                else
                {
                    Logger.Debug("VC++ Redistributable is not detected.");
                    return false;
                }
            }

        }

        bool CheckForVCVersion(string key) 
        {
            var foundVersion = new Version(key.Replace("v",""));
            var NeededVersion = new Version("14.26.28720.0");

            var result = foundVersion.CompareTo(NeededVersion);

            if(result >= 0) 
            {
                return true;
            }
            return false;

        }

        public bool DownloadAndInstall(string url, string path)
        {
            string outputPath = Path.Combine(Path.GetTempPath(), path);

            if (File.Exists(outputPath)) 
            {
                File.Delete(outputPath);
            }

            for (int retryCount = 0; retryCount < 3; retryCount++)
            {
                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(url, outputPath);
                    }
                    break;
                }
                catch (Exception e)
                {
                    Logger.Error($"error happened: {e.Message}, stacktrace: {e.StackTrace}");
                    continue;
                }
            }
            using (var process = new Process())
            {
                process.StartInfo.FileName = outputPath;
                process.StartInfo.Arguments = "/passive";
                process.Start();
                process.WaitForExit();

                if(process.ExitCode == 0 || process.ExitCode == 1638) // 1638 is error code for existing vc redist
                {
                    Logger.Debug("Deps installed Successfully");
                    return true;
                }
                return false;
            }
        }

    }
}
