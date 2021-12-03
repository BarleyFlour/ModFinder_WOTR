using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Diagnostics;

namespace ModFinder_WOTR.Infrastructure.Loader
{
    //Owlcat Mod Info class for deserialization
    public class OwlcatModificationManifest
    {
        public string UniqueName = "";
        public string Version = "";
        public string DisplayName = "";
    }
    //Umm Mod Info class for deserialization
    public class ModInfo
    {
        public string Id;
        public string DisplayName;
        public string Version;
    }
    public class ModInstall
    {
        //Only for git
        public async static void InstallFromManagerGit(ModDetails mod)
        {
            var tempfolder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("TMP") + @"\TempModFolder" + new Random().Next());
            //Download files
            {
                var release = await Infrastructure.Main.Client.Repository.Release.GetLatest(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                var todownload = release.Assets.First();
                Debug.Write(todownload.BrowserDownloadUrl);
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(todownload.BrowserDownloadUrl, tempfolder + @"\" + todownload.Name);
                    Debug.Write(tempfolder + @"\" + todownload.Name);
                    ZipFile.ExtractToDirectory(tempfolder + @"\" + todownload.Name, tempfolder.FullName, true);
                    File.Delete(tempfolder + @"\" + todownload.Name);
                }
            }
            string foldertoinstallto = "";
            string foldertoinstall = "";
            var modfolder = tempfolder.EnumerateDirectories().First();
            {
                if (File.Exists(modfolder.FullName + @"\OwlcatModificationManifest.json"))
                {
                    var OwlcatManifest = Newtonsoft.Json.JsonConvert.DeserializeObject<OwlcatModificationManifest>(File.ReadAllText(modfolder.FullName + @"\OwlcatModificationManifest.json"));
                    foldertoinstallto = Main.PFWotrAppdataPath + @"\Modifications\";
                    foldertoinstall = tempfolder.FullName;
                    Main.OwlcatEnabledMods.Add(OwlcatManifest.UniqueName);
                    FileSystem.CopyDirectory(foldertoinstall, foldertoinstallto, true);
                    FileSystem.DeleteDirectory(foldertoinstall,DeleteDirectoryOption.DeleteAllContents);
                    Main.Settings.AddInstalled(mod);
                    return;
                }
                if (File.Exists(modfolder.FullName + @"\Info.json"))
                {
                    var UMMManifest = Newtonsoft.Json.JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modfolder.FullName + @"\Info.json"));
                    foldertoinstallto = Main.WrathPath + @"\Mods\";
                    foldertoinstall = tempfolder.FullName;
                    FileSystem.CopyDirectory(foldertoinstall, foldertoinstallto, true);
                    FileSystem.DeleteDirectory(foldertoinstall, DeleteDirectoryOption.DeleteAllContents);
                    Main.Settings.AddInstalled(mod);
                    return;
                }
            }
            throw new Exception("Failed to Install Mod ");
        }

        public static void InstallFromZip(string ModZipPath)
        {
            var tempfolder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("TMP") + @"\TempModFolder" + new Random().Next());
            ZipFile.ExtractToDirectory(ModZipPath, tempfolder.FullName, true);
            string foldertoinstallto = "";
            string foldertoinstall = "";
            var modfolder = tempfolder.EnumerateDirectories().First();
            {
                if (File.Exists(modfolder.FullName + @"\OwlcatModificationManifest.json"))
                {
                    var OwlcatManifest = Newtonsoft.Json.JsonConvert.DeserializeObject<OwlcatModificationManifest>(File.ReadAllText(modfolder.FullName + @"\OwlcatModificationManifest.json"));
                    foldertoinstallto = Main.PFWotrAppdataPath + @"\Modifications\";
                    foldertoinstall = tempfolder.FullName;
                    Main.OwlcatEnabledMods.Add(OwlcatManifest.UniqueName);
                    FileSystem.CopyDirectory(foldertoinstall, foldertoinstallto, true);
                    return;
                }
                if (File.Exists(modfolder.FullName + @"\Info.json"))
                {
                    var UMMManifest = Newtonsoft.Json.JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modfolder.FullName + @"\Info.json"));
                    foldertoinstallto = Main.WrathPath + @"\Mods\";
                    foldertoinstall = tempfolder.FullName;
                    FileSystem.CopyDirectory(foldertoinstall, foldertoinstallto, true);
                    return;
                }
            }
            throw new Exception("Failed to Install Mod, Wrongly Structured ZIP?");
        }
    }
}
