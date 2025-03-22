using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using System.Collections;

namespace PEXInstaller
{
    
    class UninstallRegistry
    {
        private string guid;
        private string subkey;
        private RegistryKey mainkey;
        private Hashtable registryList;
        private RegistryKey regLocation = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

        public UninstallRegistry(string projectName, string UninstallerPath, string iconPath) 
        {
            this.guid = $"{{9D3E8E10-8813-491E-B50E-53B826862613}}";
            this.subkey = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{guid}";
            this.mainkey = getSubKey();

            this.registryList = new Hashtable
            {
                {"DisplayIcon", iconPath },
                {"DisplayName", projectName },
                {"DisplayVersion", "1.0.0.0" },
                {"EstimatedSize", getSize() },
                {"Publisher", projectName },
                {"UninstallString",  UninstallerPath}
            };

            AddEntries();
        }
        RegistryKey getSubKey()
        {
            using (var ndpKey = regLocation.OpenSubKey(subkey, true))
            {
                
                if (ndpKey == null)
                {
                    using (RegistryKey newKey = regLocation.CreateSubKey(subkey, true))
                    {
                        if (newKey != null)
                        {
                            return newKey;
                        }
                    }
                }
                return ndpKey;
            }
        }

        void AddEntries()
        {
            using (RegistryKey Key = regLocation.OpenSubKey(subkey, true))
            {
                foreach (DictionaryEntry entry in registryList)
                {
                    string key = (string)entry.Key;
                    if (key == "EstimatedSize")
                    {                  
                        Key.SetValue(key, (int)entry.Value, RegistryValueKind.DWord);
                    }
                    else
                    {
                        Key.SetValue(key, (string)entry.Value);
                    }
                }
            }
           
        }

        int getSize() 
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return (int)((new FileInfo(assembly.Location)).Length / 1024);
        }
    }
}
