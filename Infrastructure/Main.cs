using IdentityModel.Client;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ModFinder_WOTR.Infrastructure
{
    public class Response
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public int expires_in;
        public int interval;
    }
    public class Response2
    {
        public string access_token;
        public string token_type;
        public string scope;
    }

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

        private static NexusModsNET.NexusModsClient m_NexusClient;
        public static NexusModsNET.NexusModsClient NexusClient
        {
            get
            {
                if (m_NexusClient == null)
                {
#if DEBUG
                    m_NexusClient = (NexusModsNET.NexusModsClient)(NexusModsNET.NexusModsClient.Create(Environment.GetEnvironmentVariable("NEXUS_APITOKEN"), "Modfinder_WOTR", "0"));
#else
                    m_NexusClient = (NexusModsNET.NexusModsClient)(NexusModsNET.NexusModsClient.Create(Main.Settings.NexusAPIKey, "Modfinder_WOTR", "0"));
#endif
                }
                return m_NexusClient;
            }

        }

        public static HttpClient HTTPClient
        {
            get
            {
                if (m_HTTPClient == null)
                {
                    m_HTTPClient = new HttpClient();
                  //  m_HTTPClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                }
                return m_HTTPClient;
            }
        }
        private static HttpClient m_HTTPClient;

        private static GitHubClient m_Client;
        public static GitHubClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    m_Client = new GitHubClient(new ProductHeaderValue("ModFinder_WOTR"));
                    //if (Infrastructure.Main.Settings.GithubAPIKey == null && Infrastructure.Main.Settings.GithubAPIKey == "")
                    {
                        // task.RunSynchronously();

                       // m_Client.Credentials = new Credentials(Infrastructure.Main.Settings.GithubAPIKey);
                    }
#if DEBUG
                   // m_Client.Credentials = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
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
        //Maybe change this to read registry and look for path?
        public static DirectoryInfo WrathPath
        {
            get
            {
                if (Main.Settings.s_WrathPath != null && Main.Settings.s_WrathPath != "" && m_WrathPath == null)
                {
                    m_WrathPath = new DirectoryInfo(Main.Settings.s_WrathPath);
                }
                else if (m_WrathPath == null)
                {
                    var wotrdatadir = new DirectoryInfo((new DirectoryInfo(Environment.GetEnvironmentVariable("appdata")).Parent) + @"\LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous\");
                    var log = new FileInfo(wotrdatadir + "Player.log");
                    var tempfolder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("TMP") + @"\TempModFolder" + new Random().Next());
                    var templog = log.CopyTo(tempfolder.FullName + @"\Player.log");
                    using (StreamReader sr = new StreamReader(templog.OpenRead()))
                    {
                        var firstline = sr.ReadLine();
                        firstline.Remove(0, 16).Replace(@"Wrath_Data/Managed'", "");
                        m_WrathPath = new DirectoryInfo(firstline.Remove(0, 16).Replace(@"Wrath_Data/Managed'", ""));
                    }
                    tempfolder.Delete(true);
                    Main.Settings.s_WrathPath = m_WrathPath.FullName;
                    Main.Settings.Save();
                }
                return m_WrathPath;
                throw new Exception("Unable to find Wrath Installation path, please launch the game once before starting the mod manager.");
            }
        }
        public static DirectoryInfo m_WrathPath;
    }
}
