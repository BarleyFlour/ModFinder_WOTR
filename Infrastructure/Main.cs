using ModFinder_WOTR.Infrastructure;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{


    public static class Main
    {
        public class asd : ICredentialStore
        {
            Task<Credentials> ICredentialStore.GetCredentials()
            {
                return new Task<Credentials>(() =>
                {
                    return new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
                });
                throw new NotImplementedException();
            }
        }
        public static AppSettingsData Settings
        {
            get
            {
                if(m_Settings == null)
                {
                    m_Settings = AppSettingsData.Load();
                }
                return m_Settings;
            }
        }
        private static AppSettingsData m_Settings;

        private static GitHubClient _Client;
        public static GitHubClient Client => _Client ??= new GitHubClient(new ProductHeaderValue("ModFinder_WOTR"));

        [Obsolete("use Client instead")]
        public static GitHubClient client = new GitHubClient(new ProductHeaderValue("ModFinder_WOTR"), new asd());
        public static DirectoryInfo PFWotrAppdataPath = new DirectoryInfo((new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).Parent) + @"\LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\");
        public static OwlcatModificationSettingsManager OwlcatEnabledMods = new OwlcatModificationSettingsManager();
        public static Dictionary<string, ModInfo> AllMods
        {
            get
            {
                if (Infrastructure.ModListLoader.instance == null)
                {
                    ModListLoader.GetModsManifests();
                }
                return Infrastructure.ModListLoader.instance.m_AllMods;
            }
        }
        public static DirectoryInfo WrathPath
        {
            get
            {
                if (m_WrathPath == null)
                {
                    var wotrdatadir = new DirectoryInfo((new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).Parent) + @"\LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\");
                    var log = new FileInfo(wotrdatadir + "Player.log");
                    using (StreamReader sr = new StreamReader(log.FullName))
                    {
                        var firstline = sr.ReadLine();
                        firstline.Remove(0, 16).Replace(@"Wrath_Data/Managed'", "");
                        return new DirectoryInfo(firstline.Remove(0, 16).Replace(@"Wrath_Data/Managed'", ""));
                    }
                }
                throw new Exception("Unable to find Wrath Installation path, please launch the game once before starting the mod manager.");
            }
        }
        public static DirectoryInfo m_WrathPath;
    }
}
