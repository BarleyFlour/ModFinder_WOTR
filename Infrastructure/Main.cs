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
        public static AppSettingsData Settings
        {
            get
            {
                if (m_Settings == null)
                {
                    m_Settings = AppSettingsData.Load();
                }
                return m_Settings;
            }
        }
        private static AppSettingsData m_Settings;

        private static GitHubClient m_Client;
        public static GitHubClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    m_Client = new GitHubClient(new ProductHeaderValue("ModFinder_WOTR"));
                    if(Infrastructure.Main.Settings.GithubAPIKey is null or "" || Infrastructure.Main.Settings.NexusAPIKey is null or "")
                    {
                        //Do bubble popup magic here
                    }
#if DEBUG
                    m_Client.Credentials = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
#endif
                }
                return m_Client;
            }
        }



        public static DirectoryInfo PFWotrAppdataPath = new DirectoryInfo((new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).Parent) + @"\LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\");
        public static OwlcatModificationSettingsManager OwlcatEnabledMods = new OwlcatModificationSettingsManager();
        public static List<ModDetails> AllMods
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
