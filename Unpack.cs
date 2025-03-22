using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AsmResolver;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.File;
using System.Windows.Forms;

using System.Threading;

namespace PEXInstaller
{
    class Unpack
    {
        private byte[] ProjectName = null;
        private byte[] zipBytes = null;
        private List<byte[]> archiveList = new List<byte[]>();
        private List<string> Paths = new List<string>();
        byte[] zero = new byte[] { 0 };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public ushort ProjectNameLength;
            public long ZipBytesLength;
            public ushort ArchiveCount;
            public List<int> PathLength;
        }

        Header header = new Header();
        public Unpack() 
        {

        }

        public void ReadPESection()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "setup.exe1");
               
                File.Copy(Application.ExecutablePath, tempPath, true);

                using (var service = new MemoryMappedFileService())
                {


                    var peFile = PEFile.FromFile(service.OpenFile(tempPath));
                    //var peFile = PEFile.FromFile(tempPath);

                    var section = peFile.Sections.FirstOrDefault(s => s.Name == ".packed");

                    if (section == null)
                    {
                        SectionNotFound();
                        return;
                    }

                    BinaryStreamReader data = section.CreateReader();
                    header.ProjectNameLength = data.ReadUInt16();
                    ProjectName = data.ReadSegment(header.ProjectNameLength).ToArray();
                    header.ArchiveCount = data.ReadUInt16();
                    for (int i = 0; i < header.ArchiveCount; i++)
                    {
                        Paths.Add(data.ReadSerString());
                        archiveList.Add(data.ReadSegment((uint)data.ReadInt32()).ToArray());
                    }
                    header.ZipBytesLength = data.ReadInt64();
                    zipBytes = data.ReadSegment((uint)header.ZipBytesLength).ToArray();

                    peFile = null;
                    section = null;
                }

                /*using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream))
                    {

                        header.ProjectNameLength = reader.ReadUInt16();
                        ProjectName = reader.ReadBytes(header.ProjectNameLength);
                        header.ArchiveCount = reader.ReadUInt16();
                        for(int i = 0; i < header.ArchiveCount; i++) 
                        {
                            Paths.Add(reader.ReadString());
                            archiveList.Add(reader.ReadBytes(reader.ReadInt32()));
                        }
                        header.ZipBytesLength = reader.ReadInt64();
                        zipBytes = reader.ReadBytes((int)header.ZipBytesLength);

                    }
                }*/
            }
            catch(Exception e) 
            {
                Logger.Error($"Can't read PE Section! Error: {e.Message}, StackTrace: {e.StackTrace}");
            }

            Logger.Debug($"ProjectName String = {Encoding.UTF8.GetString(ProjectName)}, ProjectName Length = {ProjectName.Length}, Zipbytes Length = {zipBytes.Length}");
        }
        public string GetProjectName() 
        {
            if(ProjectName != null) 
            {
                return Encoding.Default.GetString(ProjectName);
            }
            Logger.Warning("ProjectName is Null! Continuing with Default!");
            return "DefaultName";
        }

        ref byte[] GetZipBytes()
        {
            if(zipBytes != null) 
            {
                return ref zipBytes;
            }
            Logger.Warning("zipBytes is Null! Continuing with zero byte!");
            return ref zero;
        }
        int  CurrentCount = 0;
        int CurrentArchiveCount = 0;
        public void DecompressArchive(string destinationPath)
        {
            Form3 frm2 = (Form3)Application.OpenForms["Form3"];
            try
            {
                using (var memoryStream = new MemoryStream(zipBytes))
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                     
                            CurrentCount += 1;
                            int perc = (CurrentCount*100) / archive.Entries.Count ;
                            Thread.Sleep(50);

                            //BaseFolder to not be Skipped
                            if (CurrentCount == 1)
                                frm2.UpdateText(entry.FullName.ToString(), perc);

                            else
                             {
                                    if (entry.FullName.Length > 39)
                                         frm2.UpdateText(entry.FullName.ToString().Substring(12), perc, 1);
                                     else
                                        frm2.UpdateText(entry.FullName.ToString().Substring(12), perc, 0);
                             }
                            string entryPath = Path.Combine(destinationPath, entry.FullName);

                         
                            Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                            if (entry.Length > 0)
                            {
                                using (var entryStream = entry.Open())
                                using (var outputStream = new FileStream(entryPath, FileMode.Create))
                                {
                                    entryStream.CopyTo(outputStream);
                                }
                            }
                            if (CurrentCount == archive.Entries.Count)
                            {
                                frm2.UpdateText("Main Archive Extraction Finished", 0);
                                CurrentCount = 0;
                            }
                        }

                        archive.Dispose();
                    }
                    memoryStream.Dispose(); // to make sure that it closed
                }

                for(int i = 0; i < header.ArchiveCount; i++) 
                {
                    CurrentArchiveCount += 1;
                    CurrentCount = 0;
                 //   MessageBox.Show($"Current Archive :: {CurrentArchiveCount}  ,Current Archive :: {header.ArchiveCount} ");
                    using (var memoryStream = new MemoryStream(archiveList[i]))
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                CurrentCount += 1;
                                int perc = (CurrentCount * 100) / archive.Entries.Count;
                    

                                string entryPath = Path.Combine(destinationPath, entry.FullName);

                                Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                                if (entry.Length > 0)
                                {
                                    using (var entryStream = entry.Open())
                                    using (var outputStream = new FileStream(entryPath, FileMode.Create))
                                    {
                                        entryStream.CopyTo(outputStream);
                                    }
                                }

                                if (CurrentCount == archive.Entries.Count && header.ArchiveCount == CurrentArchiveCount)
                                    frm2.UpdateText("2nd Archive Extraction Finished", 100,      0,true);
                            }
                            archive.Dispose();
                        }
                        memoryStream.Dispose(); // to make sure that it closed
                    }
                }
            }
            catch (Exception e) 
            {
                if(e.GetType().Name == typeof(ArgumentNullException).Name) 
                {
                    Logger.Warning($"zipBytes is Null! Can't Decompress!, Error: {e.Message}, StackTrace: {e.StackTrace}");
                    return;
                }

                Logger.Error($"Can't Decompress!, Error: {e.Message}, StackTrace: {e.StackTrace}");
                return;

            }
        }

        private void SectionNotFound() 
        {
            if (MessageBox.Show("Please load .packed section or the program is going to run with the default set.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                Logger.Warning("Section not Found! Continuing!");
                return;
            }
            Logger.Warning("Section not Found! Exiting!");

            Environment.Exit(1);
        }

    }
}
