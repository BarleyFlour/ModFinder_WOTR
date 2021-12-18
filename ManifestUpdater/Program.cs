using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Threading;
using ModFinder_WOTR;
using ModFinder_WOTR.Infrastructure;
using Octokit;

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
using System.Linq;

namespace ManifestUpdater
{
    class AlsoContainerMods
    {
        [JsonProperty] public List<ModDetailsInternal> m_AllMods;
    }
    class Program
    {
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
                    m_Client.Credentials = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
#endif
                }
                return m_Client;
            }
        }
        static void Main(string[] args)
        {
            AlsoContainerMods details = null;
            using var wc = new WebClient();
            {
                string rawmanifest = wc.DownloadString(@"https://raw.githubusercontent.com/BarleyFlour/ModFinder_WOTR/master/ModManifest/Master_Manifest.json");

                details = Newtonsoft.Json.JsonConvert.DeserializeObject<AlsoContainerMods>(rawmanifest);
            }
            var list = new List<ModDetailsInternal>();
            foreach (var mod in details.m_AllMods)
            {
                _ = Task.Run(async () =>
                {

                    if (mod.Source == ModSource.GitHub)
                    {
                        Console.WriteLine("[ModFinder] ModSourceGitDescrip " + mod.Name + "\n");
                        var repo = await Client.Repository.Get(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                        var latest = await Client.Repository.Release.GetLatest(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                        Console.WriteLine("[ModFinder]        got repo name: " + repo.FullName);
                        Console.WriteLine("[ModFinder] got repo description: " + repo.Description);
                        Console.WriteLine("[ModFinder]   got latest release (tag): " + latest.TagName);
                        // await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        // {
                        var release = await Client.Repository.Release.GetLatest(mod.OwnerAndRepo[0], mod.OwnerAndRepo[1]);
                        var todownload = release.Assets.First();
                        mod.DownloadLink = todownload.BrowserDownloadUrl;
                        mod.Description = repo.Description;
                        mod.LatestVersion = latest.TagName.StripV(); //This is not true???
                        Console.WriteLine($"setting mod version to: {mod.LatestVersion}");
                        list.Add(mod);
                        // });

                    }
                    else if (mod.Source == ModFinder_WOTR.Infrastructure.ModSource.Nexus)
                    {
                        // Debug.WriteLine(mod.NexusModID);
                        var nexusmod = await NexusModsNET.NexusModsFactory.New(NexusClient).CreateModsInquirer().GetMod("pathfinderwrathoftherighteous", long.Parse(mod.NexusModID));

                        //  await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        // {

                        mod.Description = nexusmod.Description;
                        mod.LatestVersion = nexusmod.Version.StripV(); //This is not true???
                        var modde = await NexusModsNET.NexusModsFactory.New(NexusClient).CreateModFilesInquirer().GetModFilesAsync("pathfinderwrathoftherighteous", long.Parse(mod.NexusModID));
                        var release = modde.ModFiles.Last();
                        mod.DownloadLink = @"https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/" + mod.NexusModID + @"?tab=files^&file_id=" + release.FileId;
                        Console.WriteLine("Stuff " + mod.Name + $"setting mod version to: {mod.LatestVersion}");
                        list.Add(mod);
                        // });
                    }
                });
            }
            while (list.Count != details.m_AllMods.Count)
            {

            }
            details.m_AllMods = list;
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            var tempfolder = Directory.CreateDirectory(Environment.GetEnvironmentVariable("TMP") + @"\TempModFolder" + new Random().Next());
            using (StreamWriter sw = new StreamWriter(tempfolder.FullName + @"\Stuff.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, details);
                //Push stuff here
                //var j = new UpdateFileRequest("Automatic Update",,);
               // Client.Repository.Content.UpdateFile();
            }
            Console.WriteLine("Hello World!");
        }
    }
}
